using System.Runtime.InteropServices;

namespace Notewright.Input.Windows;

[StructLayout(LayoutKind.Sequential)]
internal struct WndClassEx
{
	internal uint CbSize;
	internal uint Style;
	internal nint LpfnWndProc;
	internal int CbClsExtra;
	internal int CbWndExtra;
	internal nint HInstance;
	internal nint HIcon;
	internal nint HCursor;
	internal nint HbrBackground;
	internal nint LpszMenuName;
	internal nint LpszClassName;
	internal nint HIconSm;
}
