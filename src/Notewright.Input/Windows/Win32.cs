using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Notewright.Input.Windows;

internal static partial class Win32
{
	internal const int KeyeventfKeyup = 0x0002;
	internal const int KeyeventfExtendedkey = 0x0001;
	internal const int WmHotkey = 0x0312;
	internal const int HwndMessage = -3;
	internal const uint WmRegisterHotkey = 0x8001;
	internal const uint WmUnregisterHotkey = 0x8002;

	[
		DefaultDllImportSearchPaths(DllImportSearchPath.System32),
		DllImport("user32.dll", SetLastError = true),
		SuppressMessage(
			"Interoperability",
			"SYSLIB1054:Use LibraryImportAttribute instead of DllImportAttribute",
			Justification = "LibraryImport does not correctly marshal Input[] arrays with DisableRuntimeMarshalling."
		),
	]
	internal static extern uint SendInput(uint nInputs, [In] Input[] pInputs, int cbSize);

	[
		DefaultDllImportSearchPaths(DllImportSearchPath.System32),
		DllImport("user32.dll", EntryPoint = "VkKeyScanW", SetLastError = false),
		SuppressMessage(
			"Interoperability",
			"SYSLIB1054:Use LibraryImportAttribute instead of DllImportAttribute",
			Justification = "LibraryImport does not correctly marshal char parameters without DisableRuntimeMarshalling."
		)
	]
	internal static extern ushort VkKeyScan(char ch);

	[
		DefaultDllImportSearchPaths(DllImportSearchPath.System32),
		LibraryImport("user32.dll", SetLastError = true)
	]
	[return: MarshalAs(UnmanagedType.Bool)]
	internal static partial bool RegisterHotKey(nint hWnd, int id, uint fsModifiers, uint vk);

	[
		DefaultDllImportSearchPaths(DllImportSearchPath.System32),
		LibraryImport("user32.dll", SetLastError = true)
	]
	[return: MarshalAs(UnmanagedType.Bool)]
	internal static partial bool UnregisterHotKey(nint hWnd, int id);

	[
		DefaultDllImportSearchPaths(DllImportSearchPath.System32),
		LibraryImport("user32.dll", SetLastError = true)
	]
	internal static partial nint CreateWindowExW(
		uint dwExStyle,
		[MarshalAs(UnmanagedType.LPWStr)] string lpClassName,
		[MarshalAs(UnmanagedType.LPWStr)] string lpWindowName,
		uint dwStyle,
		int x,
		int y,
		int nWidth,
		int nHeight,
		nint hWndParent,
		nint hMenu,
		nint hInstance,
		nint lpParam
	);

	[
		DefaultDllImportSearchPaths(DllImportSearchPath.System32),
		LibraryImport("user32.dll", SetLastError = true)
	]
	[return: MarshalAs(UnmanagedType.Bool)]
	internal static partial bool DestroyWindow(nint hWnd);

	[DefaultDllImportSearchPaths(DllImportSearchPath.System32), LibraryImport("user32.dll")]
	internal static partial nint DefWindowProcW(nint hWnd, uint msg, nint wParam, nint lParam);

	[
		DefaultDllImportSearchPaths(DllImportSearchPath.System32),
		LibraryImport("user32.dll", EntryPoint = "GetMessageW")
	]
	[return: MarshalAs(UnmanagedType.Bool)]
	internal static partial bool GetMessage(
		out Msg lpMsg,
		nint hWnd,
		uint wMsgFilterMin,
		uint wMsgFilterMax
	);

	[DefaultDllImportSearchPaths(DllImportSearchPath.System32), LibraryImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	internal static partial bool TranslateMessage(in Msg lpMsg);

	[DefaultDllImportSearchPaths(DllImportSearchPath.System32), LibraryImport("user32.dll")]
	internal static partial nint DispatchMessageW(in Msg lpMsg);

	[
		DefaultDllImportSearchPaths(DllImportSearchPath.System32),
		LibraryImport("user32.dll", SetLastError = true),
	]
	internal static partial ushort RegisterClassExW(in WndClassEx lpwcx);

	[
		DefaultDllImportSearchPaths(DllImportSearchPath.System32),
		LibraryImport("user32.dll", SetLastError = true),
	]
	[return: MarshalAs(UnmanagedType.Bool)]
	internal static partial bool UnregisterClassW(
		[MarshalAs(UnmanagedType.LPWStr)] string lpClassName,
		nint hInstance
	);

	[
		DefaultDllImportSearchPaths(DllImportSearchPath.System32),
		LibraryImport("user32.dll", SetLastError = true),
	]
	[return: MarshalAs(UnmanagedType.Bool)]
	internal static partial bool PostMessageW(nint hWnd, uint msg, nint wParam, nint lParam);

	[StructLayout(LayoutKind.Sequential)]
	internal struct MouseInput
	{
		internal int Dx;
		internal int Dy;
		internal uint MouseData;
		internal uint DwFlags;
		internal uint Time;
		internal nint DwExtraInfo;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct HardwareInput
	{
		internal uint UMsg;
		internal ushort WParamL;
		internal ushort WParamH;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct KeybdInput
	{
		internal ushort Wvk;
		internal ushort WScan;
		internal uint DwFlags;
		internal uint Time;
		internal nint DwExtraInfo;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct Input
	{
		internal uint Type;
		internal InputUnion Data;
	}

	[SuppressMessage(
		"Interoperability",
		"CA1421:This method uses runtime marshalling even when the \'DisableRuntimeMarshallingAttribute\' is applied"
	)]
	internal static readonly int InputSize = Marshal.SizeOf<Input>();
}
