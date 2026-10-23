using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Notewright.UI.ViewModels;

internal sealed partial class MainWindowViewModel(
	SheetViewModel sheetViewModel,
	PlaybackViewModel playbackViewModel,
	SettingsViewModel settingsViewModel,
	StatusBarViewModel statusBarViewModel
) : ObservableObject
{
	[
		ObservableProperty,
		NotifyPropertyChangedFor(nameof(CurrentPageTitle)),
		NotifyPropertyChangedFor(nameof(IsPlayerPage)),
		NotifyPropertyChangedFor(nameof(IsSettingsPage))
	]
	public partial ObservableObject CurrentPage { get; set; } = sheetViewModel;

	public SheetViewModel SheetViewModel { get; } = sheetViewModel;
	public PlaybackViewModel PlaybackViewModel { get; } = playbackViewModel;
	public SettingsViewModel SettingsViewModel { get; } = settingsViewModel;
	public StatusBarViewModel StatusBarViewModel { get; } = statusBarViewModel;

	public IRelayCommand ImportSheetCommand => SheetViewModel.ImportSheetCommand;
	public IRelayCommand ExportSheetCommand => SheetViewModel.ExportSheetCommand;

	public bool IsPlayerPage => CurrentPage is SheetViewModel;
	public bool IsSettingsPage => CurrentPage is SettingsViewModel;

	public string CurrentPageTitle =>
		CurrentPage is SettingsViewModel ? "Settings" : "Piano player";

	public void NavigateBack() => NavigateToPlayer();

	[RelayCommand]
	private void NavigateToPlayer()
	{
		CurrentPage = SheetViewModel;
		StatusBarViewModel.StatusMessage = "Ready to play";
		StatusBarViewModel.InfoText = $"UTF-8 · Plain Text · {SheetViewModel.LineCount} lines";
	}

	[RelayCommand]
	private void NavigateToSettings()
	{
		CurrentPage = SettingsViewModel;
		StatusBarViewModel.StatusMessage = "Preferences saved locally";
		StatusBarViewModel.InfoText = "Notewright / v1.0";
	}
}
