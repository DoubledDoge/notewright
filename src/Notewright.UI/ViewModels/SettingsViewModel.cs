using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Notewright.UI.ViewModels;

using Input;

internal sealed partial class SettingsViewModel(
	IGlobalHotkeyService hotkeyService,
	PlaybackViewModel playbackViewModel,
	StatusBarViewModel statusBarViewModel,
	Action navigateBack
) : ObservableObject
{
	private Action _navigateBack = navigateBack;

	[ObservableProperty, NotifyCanExecuteChangedFor(nameof(SaveHotkeyCommand))]
	public partial string ManualHotkeyInput { get; set; } = string.Empty;

	[ObservableProperty]
	public partial char CurrentManualHotkey { get; set; } = '=';

	[ObservableProperty]
	public partial string? HotkeyErrorMessage { get; set; }

	[ObservableProperty]
	public partial bool HasConflict { get; set; }

	[ObservableProperty]
	public partial bool IsSaved { get; set; }

	[ObservableProperty]
	public partial string ConflictStatus { get; set; } = "Available · No conflicts";

	[ObservableProperty]
	public partial bool IsChangingHotkey { get; set; }

	public void SetNavigateBack(Action navigateBack) => _navigateBack = navigateBack;

	private bool CanSaveHotkey =>
		!string.IsNullOrWhiteSpace(ManualHotkeyInput)
		&& ManualHotkeyInput.Length == 1
		&& !HasConflict;

	partial void OnManualHotkeyInputChanged(string value)
	{
		if (string.IsNullOrWhiteSpace(value) || value.Length != 1)
		{
			HasConflict = false;
			ConflictStatus = "Available · No conflicts";
			return;
		}

		char key = value[0];

		// Check against known system reserved keys
		HasConflict = key is '\t' or '\r' or '\n' or ' ' or '\b';

		ConflictStatus = HasConflict
			? "Conflict detected · Choose a different key"
			: "Available · No conflicts";

		IsSaved = false;
	}

	[RelayCommand]
	private void NavigateBack() => _navigateBack();

	[RelayCommand]
	private void ChangeHotkey() => IsChangingHotkey = !IsChangingHotkey;

	[RelayCommand(CanExecute = nameof(CanSaveHotkey))]
	private void SaveHotkey()
	{
		char newKey = ManualHotkeyInput[0];

		if (newKey == CurrentManualHotkey)
		{
			IsSaved = true;
			statusBarViewModel.StatusMessage = "Preferences saved locally";
			return;
		}

		if (playbackViewModel.IsManualMode)
		{
			hotkeyService.Unregister(CurrentManualHotkey);
			hotkeyService.Register(newKey, () => { });
		}

		CurrentManualHotkey = newKey;
		HotkeyErrorMessage = null;
		ManualHotkeyInput = string.Empty;
		IsSaved = true;
		statusBarViewModel.StatusMessage = "Preferences saved locally";
	}

	[RelayCommand]
	private void ResetHotkey()
	{
		const char defaultHotkey = '=';

		if (CurrentManualHotkey == defaultHotkey)
		{
			return;
		}

		if (playbackViewModel.IsManualMode)
		{
			hotkeyService.Unregister(CurrentManualHotkey);
			hotkeyService.Register(defaultHotkey, () => { });
		}

		CurrentManualHotkey = defaultHotkey;
		ManualHotkeyInput = string.Empty;
		HotkeyErrorMessage = null;
		IsSaved = true;
		statusBarViewModel.StatusMessage = "Preferences saved locally";
	}

	[RelayCommand]
	private void Cancel()
	{
		ManualHotkeyInput = string.Empty;
		HotkeyErrorMessage = null;
		HasConflict = false;
		ConflictStatus = "Available · No conflicts";
		IsSaved = false;
		_navigateBack();
	}
}
