using System.Runtime.InteropServices;

namespace Notewright.Input.Linux;

internal static partial class LibInput
{
	private const string LibInputLib = "libinput.so.10";
	private const string LibUdev = "libudev.so.1";

	internal const uint LibinputEventKeyboardKeyPress = 300;

	[DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories), LibraryImport(LibUdev)]
	internal static partial nint UdevNew();

	[DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories), LibraryImport(LibUdev)]
	internal static partial void UdevUnref(nint udev);

	[
		DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories),
		LibraryImport(LibInputLib, SetLastError = true),
	]
	internal static partial nint LibinputUdevCreateContext(
		nint @interface,
		nint userData,
		nint udev
	);

	[
		DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories),
		LibraryImport(LibInputLib, SetLastError = true),
	]
	internal static partial int LibinputUdevAssignSeat(
		nint libinput,
		[MarshalAs(UnmanagedType.LPStr)] string seatId
	);

	[DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories), LibraryImport(LibInputLib)]
	internal static partial void LibinputUnref(nint libinput);

	[DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories), LibraryImport(LibInputLib)]
	internal static partial int LibinputDispatch(nint libinput);

	[DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories), LibraryImport(LibInputLib)]
	internal static partial nint LibinputGetEvent(nint libinput);

	[DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories), LibraryImport(LibInputLib)]
	internal static partial uint LibinputEventGetType(nint @event);

	[DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories), LibraryImport(LibInputLib)]
	internal static partial nint LibinputEventGetKeyboardEvent(nint @event);

	[DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories), LibraryImport(LibInputLib)]
	internal static partial uint LibinputEventKeyboardGetKey(nint keyboardEvent);

	[DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories), LibraryImport(LibInputLib)]
	internal static partial uint LibinputEventKeyboardGetKeyState(nint keyboardEvent);

	[DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories), LibraryImport(LibInputLib)]
	internal static partial void LibinputEventDestroy(nint @event);
}
