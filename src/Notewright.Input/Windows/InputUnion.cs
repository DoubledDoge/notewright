using System.Runtime.InteropServices;

namespace Notewright.Input.Windows;

[StructLayout(LayoutKind.Explicit)]
internal struct InputUnion
{
	[FieldOffset(0)]
	internal Win32.MouseInput Mi;

	[FieldOffset(0)]
	internal Win32.KeybdInput Ki;

	[FieldOffset(0)]
	internal Win32.HardwareInput Hi;
}
