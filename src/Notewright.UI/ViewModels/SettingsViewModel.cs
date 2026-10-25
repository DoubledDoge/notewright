using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Notewright.UI.Services;
using Notewright.UI.Services.Enums;

namespace Notewright.UI.ViewModels;

internal sealed partial class SettingsViewModel(
	HotkeyBindings bindings,
	HotkeyCoordinator hotkeys,
	StatusBarViewModel statusBarViewModel,
	Action navigateBack
) : ObservableObject
{
	private Action _navigateBack = navigateBack;

	public IReadOnlyList<HotkeyEditorViewModel> Editors { get; } =
	[
		new(
			HotkeyAction.Play,
			"Play / resume",
			"Automated mode only. Starts playback, or resumes it after a pause.",
			bindings,
			hotkeys,
			statusBarViewModel
		),
		new(
			HotkeyAction.Pause,
			"Pause",
			"Automated mode only. Pauses at the current beat.",
			bindings,
			hotkeys,
			statusBarViewModel
		),
		new(
			HotkeyAction.Stop,
			"Stop",
			"Stops playback. In manual mode, restarts the sheet from the first beat.",
			bindings,
			hotkeys,
			statusBarViewModel
		),
		new(
			HotkeyAction.Advance,
			"Play next chord",
			"Manual mode only. Plays the next note, chord or roll.",
			bindings,
			hotkeys,
			statusBarViewModel
		),
	];

	public void SetNavigateBack(Action navigateBack) => _navigateBack = navigateBack;

	[RelayCommand]
	private void NavigateBack() => _navigateBack();

	[RelayCommand]
	private void SaveAll()
	{
		foreach (HotkeyEditorViewModel editor in Editors)
		{
			if (editor.CanSave)
				editor.Save();
		}
	}

	[RelayCommand]
	private void Cancel()
	{
		foreach (HotkeyEditorViewModel editor in Editors)
			editor.Clear();

		_navigateBack();
	}
}
