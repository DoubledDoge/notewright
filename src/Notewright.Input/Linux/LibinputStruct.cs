using System.Runtime.InteropServices;

namespace Notewright.Input.Linux;

[StructLayout(LayoutKind.Sequential)]
internal struct LibinputStruct
{
	internal nint OpenRestricted;
	internal nint CloseRestricted;
}
