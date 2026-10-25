namespace Notewright.UI.Services.Enums;

[Flags]
internal enum HotkeyModes
{
	None = 0,
	Automated = 1,
	Manual = 2,
	Both = Automated | Manual,
}
