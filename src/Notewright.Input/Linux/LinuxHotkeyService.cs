using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Notewright.Input.Linux;

[
	SupportedOSPlatform("linux"),
	SuppressMessage(
		"Minor Code Smell",
		"S1450:Private fields only used as local variables in methods should become local variables"
	),
]
public sealed class LinuxHotkeyService : IGlobalHotkeyService
{
	private static readonly ConcurrentDictionary<char, Action> Hotkeys = new();
	private readonly Thread _eventThread;
	private readonly nint _libinput;

	private readonly nint _udev;
	private bool _disposed;

	public unsafe LinuxHotkeyService()
	{
		_udev = LibInput.UdevNew();

		if (_udev == nint.Zero)
		{
			throw new InvalidOperationException(
				"Failed to create udev context. Ensure that libudev is installed on your system."
			);
		}

		LibinputStruct iface = new()
		{
			OpenRestricted = (nint)
				(delegate* unmanaged[Stdcall]<byte*, int, nint, int>)&OpenRestricted,
			CloseRestricted = (nint)(delegate* unmanaged[Stdcall]<int, nint, void>)&CloseRestricted,
		};

		_libinput = LibInput.LibinputUdevCreateContext(
			(nint)(LibinputStruct*)Unsafe.AsPointer(ref iface),
			nint.Zero,
			_udev
		);

		if (_libinput == nint.Zero)
		{
			LibInput.UdevUnref(_udev);
			throw new InvalidOperationException(
				"Failed to create libinput context. Ensure that your user has the necessary input group permissions."
			);
		}

		if (LibInput.LibinputUdevAssignSeat(_libinput, "seat0") != 0)
		{
			LibInput.LibinputUnref(_libinput);
			LibInput.UdevUnref(_udev);
			throw new InvalidOperationException(
				"Failed to assign libinput seat. Ensure that seat0 is available on your system."
			);
		}

		_eventThread = new Thread(RunEventLoop)
		{
			IsBackground = true,
			Name = "Notewright Linux Hotkey Event Loop",
		};

		_eventThread.Start();
	}

	public void Register(char key, Action onPressed)
	{
		ArgumentNullException.ThrowIfNull(onPressed);
		ObjectDisposedException.ThrowIf(_disposed, this);

		Hotkeys[key] = onPressed;
	}

	public void Unregister(char key)
	{
		ObjectDisposedException.ThrowIf(_disposed, this);
		Hotkeys.TryRemove(key, out _);
	}

	public void Dispose()
	{
		if (_disposed)
		{
			return;
		}

		_disposed = true;

		LibInput.LibinputUnref(_libinput);
		LibInput.UdevUnref(_udev);

		GC.SuppressFinalize(this);
	}

	[UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
	private static unsafe int OpenRestricted(byte* path, int flags, nint userData)
	{
		int fd = LibC.Open(path, flags);

		return fd < 0 ? -Marshal.GetLastWin32Error() : fd;
	}

	[UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
	private static void CloseRestricted(int fd, nint userData)
	{
		if (LibC.Close(fd) != 0)
		{
			throw new InvalidOperationException(
				$"Failed to close file descriptor {fd} with error {Marshal.GetLastWin32Error()}."
			);
		}
	}

	private void RunEventLoop()
	{
		const bool isShifted = false;

		while (!_disposed)
		{
			if (LibInput.LibinputDispatch(_libinput) != 0)
			{
				throw new InvalidOperationException(
					"libinput dispatch failed. The input event loop cannot continue."
				);
			}

			nint libinputEvent = LibInput.LibinputGetEvent(_libinput);

			while (libinputEvent != nint.Zero)
			{
				uint type = LibInput.LibinputEventGetType(libinputEvent);

				if (type == LibInput.LibinputEventKeyboardKeyPress)
				{
					nint keyboardEvent = LibInput.LibinputEventGetKeyboardEvent(libinputEvent);
					uint keyCode = LibInput.LibinputEventKeyboardGetKey(keyboardEvent);
					uint keyState = LibInput.LibinputEventKeyboardGetKeyState(keyboardEvent);

					if (keyState == LibInput.LibinputEventKeyboardKeyPress)
					{
						char key = LinuxKeyCodeMap.ToChar(keyCode, isShifted);

						if (key != '\0' && Hotkeys.TryGetValue(key, out Action? onPressed))
						{
							onPressed();
						}
					}
				}

				LibInput.LibinputEventDestroy(libinputEvent);
				libinputEvent = LibInput.LibinputGetEvent(_libinput);
			}

			Thread.Sleep(1);
		}
	}

	~LinuxHotkeyService() => Dispose();
}
