using Avalonia.Data.Converters;
using Avalonia.Media;

namespace Notewright.UI.Converters;

internal static class PlaybackConverters
{
	public static readonly IValueConverter ModeBadgeText = new FuncValueConverter<bool, string>(
		isManual => isManual ? "MANUAL MODE" : "AUTOMATED MODE"
	);

	public static readonly IValueConverter ModeBadgeColor = new FuncValueConverter<bool, IBrush>(
		isManual =>
			isManual
				? new SolidColorBrush(Color.Parse("#a78bfa"))
				: new SolidColorBrush(Color.Parse("#4ade80"))
	);

	public static readonly IValueConverter ModeBadgeBackground = new FuncValueConverter<
		bool,
		IBrush
	>(isManual =>
		isManual
			? new SolidColorBrush(Color.Parse("#2a1e3a"))
			: new SolidColorBrush(Color.Parse("#1e3a2f"))
	);

	public static readonly IValueConverter ModeBadgeBorder = new FuncValueConverter<bool, IBrush>(
		isManual =>
			isManual
				? new SolidColorBrush(Color.Parse("#a78bfa"))
				: new SolidColorBrush(Color.Parse("#4ade80"))
	);

	public static readonly IValueConverter ModeDescriptionText = new FuncValueConverter<
		bool,
		string
	>(isManual =>
		isManual ? "Press hotkey to advance one beat." : "Notes advance at your set tempo."
	);

	public static readonly IValueConverter IsNonZero = new FuncValueConverter<int, bool>(v =>
		v > 0
	);
}
