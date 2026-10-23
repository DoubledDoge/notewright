using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Notewright.Input.MacOS;

[
	SupportedOSPlatform("macos"),
	SuppressMessage(
		"Minor Code Smell",
		"S1450:Private fields only used as local variables in methods should become local variables"
	)
]
public sealed class MacHotkeyService : IGlobalHotkeyService
{
	private const int KcgKeyboardEventKeycode = 9;

	private static readonly ConcurrentDictionary<char, Action> Hotkeys = new();

	private readonly nint _eventTap;
	private readonly nint _runLoopSource;
	private readonly nint _runLoop;
	private readonly Thread _eventThread;
	private bool _disposed;

	public unsafe MacHotkeyService()
	{
		_eventTap = CoreGraphics.CGEventTapCreate(
			CoreGraphics.KcgSessionEventTap,
			CoreGraphics.KcgHeadInsertEventTap,
			CoreGraphics.KcgEventTapOptionDefault,
			CoreGraphics.KcgEventMaskKeyDown,
			(nint)(delegate* unmanaged[Stdcall]<nint, uint, nint, nint, nint>)&EventTapCallback,
			nint.Zero
		);

		if (_eventTap == nint.Zero)
		{
			throw new InvalidOperationException(
				"Failed to create CGEventTap. Ensure that the application has been granted Accessibility permissions under System Settings > Privacy & Security > Accessibility."
			);
		}

		_runLoopSource = CoreGraphics.CFMachPortCreateRunLoopSource(
			nint.Zero,
			_eventTap,
			nint.Zero
		);

		if (_runLoopSource == nint.Zero)
		{
			CoreGraphics.CFRelease(_eventTap);
			throw new InvalidOperationException("Failed to create run loop source from event tap.");
		}

		TaskCompletionSource<nint> runLoopSource = new();

		_eventThread = new Thread(() => RunEventLoop(runLoopSource))
		{
			IsBackground = true,
			Name = "Notewright macOS Hotkey Event Loop",
		};

		_eventThread.Start();

		_runLoop = runLoopSource.Task.GetAwaiter().GetResult();
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

	private void RunEventLoop(TaskCompletionSource<nint> runLoopSource)
	{
		nint runLoop = CoreGraphics.CFRunLoopGetCurrent();

		CoreGraphics.CFRunLoopAddSource(runLoop, _runLoopSource, nint.Zero);

		CoreGraphics.CgEventTapEnable(_eventTap, true);

		runLoopSource.SetResult(runLoop);

		CoreGraphics.CFRunLoopRun();
	}

	[UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
	private static nint EventTapCallback(nint proxy, uint type, nint @event, nint userInfo)
	{
		if (type == CoreGraphics.KcgEventKeyDown)
		{
			ushort keyCode = CoreGraphics.CGEventGetIntegerValueField(
				@event,
				KcgKeyboardEventKeycode
			);

			ulong flags = CoreGraphics.CGEventGetFlags(@event);
			bool isShifted = (flags & CoreGraphics.KcgEventFlagMaskShift) != 0;

			char key = MacKeyCodeMap.ToChar(keyCode, isShifted);

			if (key != '\0' && Hotkeys.TryGetValue(key, out Action? onPressed))
			{
				onPressed();
			}
		}

		return @event;
	}

	~MacHotkeyService() => Dispose();

	public void Dispose()
	{
		if (_disposed)
		{
			return;
		}

		_disposed = true;

		if (_runLoop != nint.Zero)
		{
			CoreGraphics.CFRunLoopStop(_runLoop);
		}

		if (_eventTap != nint.Zero)
		{
			CoreGraphics.CgEventTapEnable(_eventTap, false);
			CoreGraphics.CFRelease(_eventTap);
		}

		if (_runLoopSource != nint.Zero)
		{
			CoreGraphics.CFRelease(_runLoopSource);
		}

		GC.SuppressFinalize(this);
	}
}
