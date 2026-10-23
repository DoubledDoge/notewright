using System.Runtime.InteropServices;

namespace Notewright.Input.Windows;

[StructLayout(LayoutKind.Sequential)]
internal struct Msg
{
	internal nint HWnd;
	internal uint Message;
	internal nint WParam;
	internal nint LParam;
	internal uint Time;
	internal int Px;
	internal int Py;
}
