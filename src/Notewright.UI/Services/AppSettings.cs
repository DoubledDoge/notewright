namespace Notewright.UI.Services;

using Enums;

internal sealed record AppSettings
{
	public int Version { get; init; } = 1;
	public Dictionary<HotkeyAction, char> Hotkeys { get; init; } = [];
	public int? Bpm { get; init; }
	public int? Transposition { get; init; }
}
