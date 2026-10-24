using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;

namespace Notewright.UI.Views.Player;

[SuppressMessage(
	"Performance",
	"CA1812:Avoid uninstantiated internal classes",
	Justification = "Instantiated by Avalonia's XAML loader at runtime."
)]
internal sealed partial class SyntaxLegendView : UserControl
{
	public SyntaxLegendView() => InitializeComponent();

	public static IReadOnlyList<SyntaxLegendEntry> Entries { get; } =
	[
		new("q", "Single note", "one beat"),
		new("Q  !", "Shifted note", "uppercase and ! @ $ % ^ * ( hold Shift"),
		new("[qet]", "Chord", "all keys at once"),
		new("{qet}", "Roll", "keys in quick succession"),
		new("q-w-e", "Legato run", "quarter beat apart, no spaces around '-'"),
		new("q -", "Short pause", "half a beat, needs a space before '-'"),
		new("q,", "Medium pause", "three-quarters of a beat"),
		new("| or .", "Rest", "one silent beat each, || is two"),
		new("space", "Separator", "optional, adds no pause"),
	];
}
