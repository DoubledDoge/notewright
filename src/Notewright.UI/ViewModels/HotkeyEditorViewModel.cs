using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Notewright.Core;
using Notewright.UI.Services;
using Notewright.UI.Services.Enums;

namespace Notewright.UI.ViewModels;

internal sealed partial class HotkeyEditorViewModel(
	HotkeyAction action,
	string title,
	string description,
	HotkeyBindings bindings,
	HotkeyCoordinator hotkeys,
	StatusBarViewModel statusBar
) : ObservableObject
{
	private const string Available = "Available - No conflicts";

	public string Title { get; } = title;
	public string Description { get; } = description;
	public char DefaultKey => HotkeyBindings.Defaults[action];
	public string ResetText => $"Reset to {DefaultKey}";

	[ObservableProperty]
	[NotifyCanExecuteChangedFor(nameof(SaveCommand))]
	public partial string Input { get; set; } = string.Empty;

	[ObservableProperty]
	[NotifyCanExecuteChangedFor(nameof(SaveCommand))]
	public partial bool HasConflict { get; set; }

	[ObservableProperty]
	public partial char CurrentKey { get; set; } = bindings[action];

	[ObservableProperty]
	public partial string ConflictStatus { get; set; } = Available;

	[ObservableProperty]
	public partial bool IsSaved { get; set; }

	[ObservableProperty]
	public partial bool IsChanging { get; set; }

	public bool CanSave => Input.Length == 1 && !HasConflict;

	public void Clear()
	{
		Input = string.Empty;
		HasConflict = false;
		ConflictStatus = Available;
		IsSaved = false;
	}

	partial void OnInputChanged(string value)
	{
		IsSaved = false;

		if (value.Length != 1)
		{
			HasConflict = false;
			ConflictStatus = Available;
			return;
		}

		string? conflict = FindConflict(value[0]);
		HasConflict = conflict is not null;
		ConflictStatus = conflict ?? Available;
	}

	[RelayCommand]
	private void Change() => IsChanging = !IsChanging;

	[RelayCommand(CanExecute = nameof(CanSave))]
	public void Save() => Commit(Input[0]);

	[RelayCommand]
	private void Reset() => Commit(DefaultKey);

	private void Commit(char key)
	{
		if (key != CurrentKey)
		{
			if (FindConflict(key) is { } conflict)
			{
				HasConflict = true;
				ConflictStatus = conflict;
				return;
			}

			hotkeys.Rebind(action, key);
			CurrentKey = key;
		}

		Input = string.Empty; // raises OnInputChanged, which clears IsSaved
		IsSaved = true; // so set it afterwards
		statusBar.StatusMessage = "Preferences saved locally";
	}

	private string? FindConflict(char key) =>
		!HotkeyKeys.Supported.Contains(key) ? "Unsupported key. Use one of:  -  =  \\  /  ;  '  `"
		: bindings.IsUsedByOther(action, key) ? "Already used by another hotkey."
		: null;
}
