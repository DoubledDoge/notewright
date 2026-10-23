using CommunityToolkit.Mvvm.ComponentModel;

namespace Notewright.UI.ViewModels;

internal sealed partial class StatusBarViewModel : ObservableObject
{
	[ObservableProperty]
	public partial string StatusMessage { get; set; } = "Ready to play";

	[ObservableProperty]
	public partial string InfoText { get; set; } = "Notewright / v1.0";
}
