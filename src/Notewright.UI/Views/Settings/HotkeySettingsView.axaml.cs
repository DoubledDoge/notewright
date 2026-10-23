using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;

namespace Notewright.UI.Views.Settings;

[SuppressMessage(
	"Performance",
	"CA1812:Avoid uninstantiated internal classes",
	Justification = "Instantiated by Avalonia's XAML loader at runtime."
)]
internal sealed partial class HotkeySettingsView : UserControl
{
	public HotkeySettingsView() => InitializeComponent();
}
