using System.Runtime.InteropServices;

namespace Notewright.Input.Linux;

internal static partial class LibC
{
	private const string LibCLib = "libc.so.6";

	[
		DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories),
		LibraryImport(LibCLib, SetLastError = true)
	]
	internal static unsafe partial int Open(byte* path, int flags);

	[
		DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories),
		LibraryImport(LibCLib, SetLastError = true)
	]
	internal static partial int Close(int fd);
}
