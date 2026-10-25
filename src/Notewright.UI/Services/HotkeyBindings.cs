using Notewright.UI.Services.Enums;

namespace Notewright.UI.Services;

internal sealed class HotkeyBindings
{
	private readonly Dictionary<HotkeyAction, char> _keys = new(Defaults);

	public static IReadOnlyDictionary<HotkeyAction, char> Defaults { get; } =
		new Dictionary<HotkeyAction, char>
		{
			[HotkeyAction.Advance] = '=',
			[HotkeyAction.Play] = '-',
			[HotkeyAction.Pause] = '\\',
			[HotkeyAction.Stop] = '/',
		};

	public char this[HotkeyAction action]
	{
		get => _keys[action];
		set => _keys[action] = value;
	}

	public bool IsUsedByOther(HotkeyAction action, char key)
	{
		return _keys.Any(pair => pair.Key != action && pair.Value == key);
	}
}
