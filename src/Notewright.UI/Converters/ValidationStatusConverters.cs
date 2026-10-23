using Avalonia.Data.Converters;
using Avalonia.Media;

namespace Notewright.UI.Converters;

using ViewModels.Enums;

internal static class ValidationStatusConverters
{
	public static readonly IValueConverter IsValid = new FuncValueConverter<ValidationStatus, bool>(
		v => v == ValidationStatus.Valid
	);

	public static readonly IValueConverter IsInvalid = new FuncValueConverter<
		ValidationStatus,
		bool
	>(v => v == ValidationStatus.Invalid);

	public static readonly IValueConverter ToStatusText = new FuncValueConverter<
		ValidationStatus,
		string
	>(v =>
		v switch
		{
			ValidationStatus.Valid => "Valid notation",
			ValidationStatus.Invalid => "Invalid notation",
			_ => string.Empty,
		}
	);

	public static readonly IValueConverter ToStatusColor = new FuncValueConverter<
		ValidationStatus,
		IBrush
	>(v =>
		v switch
		{
			ValidationStatus.Valid => new SolidColorBrush(Color.Parse("#4ade80")),
			ValidationStatus.Invalid => new SolidColorBrush(Color.Parse("#f87171")),
			_ => new SolidColorBrush(Color.Parse("#555555")),
		}
	);

	public static readonly IValueConverter ToStatusDotColor = new FuncValueConverter<
		ValidationStatus,
		IBrush
	>(v =>
		v switch
		{
			ValidationStatus.Valid => new SolidColorBrush(Color.Parse("#4ade80")),
			ValidationStatus.Invalid => new SolidColorBrush(Color.Parse("#f87171")),
			_ => Brushes.Transparent,
		}
	);
}
