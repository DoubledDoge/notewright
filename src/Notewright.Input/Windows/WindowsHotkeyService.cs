using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Notewright.Input.Windows;

[
	SupportedOSPlatform("windows"),
	SuppressMessage(
		"Minor Code Smell",
		"S1450:Private fields only used as local variables in methods should become local variables"
	),
]
public sealed class WindowsHotkeyService : IGlobalHotkeyService
{
	private static readonly ConcurrentDictionary<char, (int Id, Action OnPressed)> Hotkeys = new();
	private readonly nint _hwnd;
	private readonly Thread _messageThread;
	private int _nextId = 1;
	private bool _disposed;

	public unsafe WindowsHotkeyService()
	{
		fixed (char* classNamePtr = "NotewrightHotkeyWindow")
		{
			WndClassEx wndClass = new()
			{
				CbSize = (uint)sizeof(WndClassEx),
				LpfnWndProc = (nint)
					(delegate* unmanaged[Stdcall]<nint, uint, nint, nint, nint>)&WndProcCallback,
				LpszClassName = (nint)classNamePtr,
				HInstance = nint.Zero,
			};

			if (Win32.RegisterClassExW(in wndClass) == 0)
			{
				throw new Win32Exception(
					Marshal.GetLastWin32Error(),
					"Failed to register hotkey window class."
				);
			}
		}

		TaskCompletionSource<nint> hwndSource = new();

		_messageThread = new Thread(() => RunMessageLoop(hwndSource))
		{
			IsBackground = true,
			Name = "Notewright Hotkey Message Loop",
		};

		_messageThread.Start();

		nint hwnd = hwndSource.Task.GetAwaiter().GetResult();

		_hwnd =
			hwnd == nint.Zero
				? throw new Win32Exception(
					Marshal.GetLastWin32Error(),
					"Failed to create message-only hotkey window."
				)
				: hwnd;
	}

	private static void RunMessageLoop(TaskCompletionSource<nint> hwndSource)
	{
		nint hwnd = Win32.CreateWindowExW(
			0,
			"NotewrightHotkeyWindow",
			string.Empty,
			0,
			0,
			0,
			0,
			0,
			Win32.HwndMessage,
			nint.Zero,
			nint.Zero,
			nint.Zero
		);

		hwndSource.SetResult(hwnd);

		while (Win32.GetMessage(out Msg msg, nint.Zero, 0, 0))
		{
			Win32.TranslateMessage(in msg);
			Win32.DispatchMessageW(in msg);
		}
	}

	public void Register(char key, Action onPressed)
	{
		ArgumentNullException.ThrowIfNull(onPressed);
		ObjectDisposedException.ThrowIf(_disposed, this);

		ushort vkKeyScan = Win32.VkKeyScan(key);
		ushort virtualKey = (ushort)(vkKeyScan & 0xFF);
		int id = Interlocked.Increment(ref _nextId);

		Hotkeys[key] = (id, onPressed);
		Win32.PostMessageW(_hwnd, Win32.WmRegisterHotkey, id, virtualKey);
	}

	public void Unregister(char key)
	{
		ObjectDisposedException.ThrowIf(_disposed, this);

		if (!Hotkeys.TryRemove(key, out (int Id, Action OnPressed) entry))
		{
			return;
		}

		Win32.PostMessageW(_hwnd, Win32.WmUnregisterHotkey, entry.Id, 0);
	}

	[UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
	private static nint WndProcCallback(nint hWnd, uint msg, nint wParam, nint lParam)
	{
		if (msg == Win32.WmRegisterHotkey)
		{
			Win32.RegisterHotKey(hWnd, (int)wParam, 0, (uint)lParam);
			return nint.Zero;
		}

		if (msg == Win32.WmUnregisterHotkey)
		{
			Win32.UnregisterHotKey(hWnd, (int)wParam);
			return nint.Zero;
		}

		if (msg != Win32.WmHotkey)
		{
			return Win32.DefWindowProcW(hWnd, msg, wParam, lParam);
		}

		int id = (int)wParam;

		foreach ((char _, (int Id, Action OnPressed) entry) in Hotkeys)
		{
			if (entry.Id != id)
			{
				continue;
			}

			entry.OnPressed();
			break;
		}

		return Win32.DefWindowProcW(hWnd, msg, wParam, lParam);
	}

	public void Dispose()
	{
		if (_disposed)
		{
			return;
		}

		foreach ((char key, _) in Hotkeys)
		{
			Unregister(key);
		}

		if (_hwnd != nint.Zero)
		{
			Win32.DestroyWindow(_hwnd);
		}

		_disposed = true;
	}
}
