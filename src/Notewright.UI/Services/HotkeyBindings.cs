namespace Notewright.UI.Services;

using Core;
using Enums;

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

	public bool IsUsedByOther(HotkeyAction action, char key) =>
		_keys.Any(pair => pair.Key != action && pair.Value == key);

	public void Apply(IReadOnlyDictionary<HotkeyAction, char>? saved)
	{
		if (saved is null)
		{
			return;
		}

		Dictionary<HotkeyAction, char> candidate = new(_keys);
		foreach ((HotkeyAction action, char key) in saved)
		{
			if (HotkeyKeys.Supported.Contains(key))
			{
				candidate[action] = key;
			}
		}

		// if two actions would share a key, ignore them
		if (candidate.Values.Distinct().Count() != candidate.Count)
		{
			return;
		}

		foreach ((HotkeyAction action, char key) in candidate)
		{
			_keys[action] = key;
		}
	}

	public IReadOnlyDictionary<HotkeyAction, char> Snapshot() =>
		new Dictionary<HotkeyAction, char>(_keys);
}
