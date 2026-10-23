using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;

namespace Notewright.UI.Views.Player;

[SuppressMessage(
	"Performance",
	"CA1812:Avoid uninstantiated internal classes",
	Justification = "Instantiated by Avalonia's XAML loader at runtime."
)]
internal sealed partial class SheetView : UserControl
{
	public SheetView() => InitializeComponent();
}
