using System.Runtime.InteropServices;

namespace Notewright.Input.MacOS;

internal static partial class CoreGraphics
{
	private const string CoreGraphicsLib =
		"/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics";

	private const string CoreFoundationLib =
		"/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation";

	internal const uint KcgEventKeyDown = 10;
	internal const uint KcghidEventTap = 0;
	internal const uint KcgEventFlagMaskShift = 0x20000;
	internal const uint KcgHeadInsertEventTap = 0;
	internal const uint KcgSessionEventTap = 1;
	internal const uint KcgEventTapOptionDefault = 0;
	internal const ulong KcgEventMaskKeyDown = 1ul << (int)KcgEventKeyDown;

	[
		DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories),
		LibraryImport(CoreGraphicsLib)
	]
	internal static partial nint CGEventSourceCreate(int stateId);

	[
		DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories),
		LibraryImport(CoreGraphicsLib)
	]
	internal static partial nint CgEventCreateKeyboardEvent(
		nint source,
		ushort virtualKey,
		[MarshalAs(UnmanagedType.Bool)] bool keyDown
	);

	[
		DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories),
		LibraryImport(CoreGraphicsLib)
	]
	internal static partial void CGEventSetFlags(nint @event, ulong flags);

	[
		DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories),
		LibraryImport(CoreGraphicsLib)
	]
	internal static partial void CGEventPost(uint tap, nint @event);

	[
		DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories),
		LibraryImport(CoreGraphicsLib)
	]
	internal static partial ushort CGEventGetIntegerValueField(nint @event, int field);

	[
		DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories),
		LibraryImport(CoreGraphicsLib)
	]
	internal static partial ulong CGEventGetFlags(nint @event);

	[
		DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories),
		LibraryImport(CoreGraphicsLib)
	]
	internal static partial nint CGEventTapCreate(
		uint tap,
		uint place,
		uint options,
		ulong eventsOfInterest,
		nint callback,
		nint userInfo
	);

	[
		DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories),
		LibraryImport(CoreGraphicsLib)
	]
	internal static partial void CgEventTapEnable(
		nint tap,
		[MarshalAs(UnmanagedType.Bool)] bool enable
	);

	[
		DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories),
		LibraryImport(CoreFoundationLib)
	]
	internal static partial nint CFMachPortCreateRunLoopSource(
		nint allocator,
		nint port,
		nint order
	);

	[
		DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories),
		LibraryImport(CoreFoundationLib)
	]
	internal static partial nint CFRunLoopGetCurrent();

	[
		DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories),
		LibraryImport(CoreFoundationLib)
	]
	internal static partial void CFRunLoopAddSource(nint runLoop, nint source, nint mode);

	[
		DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories),
		LibraryImport(CoreFoundationLib)
	]
	internal static partial void CFRunLoopRun();

	[
		DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories),
		LibraryImport(CoreFoundationLib)
	]
	internal static partial void CFRunLoopStop(nint runLoop);

	[
		DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories),
		LibraryImport(CoreFoundationLib)
	]
	internal static partial void CFRelease(nint cf);
}
