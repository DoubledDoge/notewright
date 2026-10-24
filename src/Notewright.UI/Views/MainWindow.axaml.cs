using Avalonia.Controls;
using Notewright.UI.ViewModels;

namespace Notewright.UI.Views;

internal sealed partial class MainWindow : Window
{
	public MainWindow()
	{
		Activated += (_, _) => Playback?.SetAutoHotkeyActive(false);
		Deactivated += (_, _) => Playback?.SetAutoHotkeyActive(true);

		InitializeComponent();
	}

	private PlaybackViewModel? Playback => (DataContext as MainWindowViewModel)?.PlaybackViewModel;
}
