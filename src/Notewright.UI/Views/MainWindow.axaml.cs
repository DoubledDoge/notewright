using Avalonia.Controls;
using Notewright.UI.ViewModels;

namespace Notewright.UI.Views;

internal sealed partial class MainWindow : Window
{
	public MainWindow()
	{
		Activated += (_, _) => Playback?.SetWindowActive(true);
		Deactivated += (_, _) => Playback?.SetWindowActive(false);

		InitializeComponent();
	}

	private PlaybackViewModel? Playback => (DataContext as MainWindowViewModel)?.PlaybackViewModel;
}
