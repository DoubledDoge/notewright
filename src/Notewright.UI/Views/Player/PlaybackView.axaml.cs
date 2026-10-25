using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using Notewright.UI.ViewModels;

namespace Notewright.UI.Views.Player;

[SuppressMessage(
	"Performance",
	"CA1812:Avoid uninstantiated internal classes",
	Justification = "Instantiated by Avalonia's XAML loader at runtime."
)]
internal sealed partial class PlaybackView : UserControl
{
	public PlaybackView() => InitializeComponent();

	private PlaybackViewModel? ViewModel => DataContext as PlaybackViewModel;

	private void OnBpmLostFocus(object? sender, FocusChangedEventArgs e) =>
		ViewModel?.CommitBpmCommand.Execute(null);

	private void OnTranspositionLostFocus(object? sender, FocusChangedEventArgs e) =>
		ViewModel?.CommitTranspositionCommand.Execute(null);

	[SuppressMessage(
		"Minor Code Smell",
		"S2325:Methods and properties that don\'t access instance data should be static"
	)]
	private void OnValueGotFocus(object? sender, FocusChangedEventArgs e)
	{
		if (sender is TextBox box)
		{
			Dispatcher.UIThread.Post(box.SelectAll); // posted so the click doesn't undo the selection
		}
	}

	private void OnBpmKeyDown(object? sender, KeyEventArgs e) =>
		CommitOnEnter(sender, e, ViewModel?.CommitBpmCommand);

	private void OnTranspositionKeyDown(object? sender, KeyEventArgs e) =>
		CommitOnEnter(sender, e, ViewModel?.CommitTranspositionCommand);

	private static void CommitOnEnter(object? sender, KeyEventArgs e, ICommand? commit)
	{
		if (e.Key != Key.Enter)
		{
			return;
		}

		commit?.Execute(null);
		(sender as TextBox)?.SelectAll();
		e.Handled = true;
	}
}
