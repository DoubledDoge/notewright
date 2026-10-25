namespace Notewright.UI.Services;

using Enums;
using Input;

internal sealed class HotkeyCoordinator(IGlobalHotkeyService service, HotkeyBindings bindings)
{
	private readonly Dictionary<HotkeyAction, (HotkeyModes Modes, Action Handler)> _actions = [];
	private readonly Dictionary<HotkeyAction, char> _registered = [];
	public event EventHandler? BindingsChanged;
	private bool _manualMode;
	private bool _windowActive = true; // the window is focused at launch

	public void Add(HotkeyAction action, HotkeyModes modes, Action handler)
	{
		_actions[action] = (modes, handler);
		Sync(action);
	}

	public void SetWindowActive(bool active)
	{
		_windowActive = active;
		SyncAll();
	}

	public void SetManualMode(bool manual)
	{
		_manualMode = manual;
		SyncAll();
	}

	public void Rebind(HotkeyAction action, char key)
	{
		bindings[action] = key;
		Sync(action);
		BindingsChanged?.Invoke(this, EventArgs.Empty);
	}

	private void SyncAll()
	{
		foreach (HotkeyAction action in _actions.Keys)
		{
			Sync(action);
		}
	}

	private void Sync(HotkeyAction action)
	{
		(HotkeyModes modes, Action handler) = _actions[action];
		char key = bindings[action];
		bool wanted =
			!_windowActive
			&& modes.HasFlag(_manualMode ? HotkeyModes.Manual : HotkeyModes.Automated);

		if (_registered.TryGetValue(action, out char registeredKey))
		{
			if (wanted && registeredKey == key)
			{
				return;
			}

			service.Unregister(registeredKey);
			_registered.Remove(action);
		}

		if (!wanted)
		{
			return;
		}

		service.Register(key, handler);
		_registered[action] = key;
	}
}
