using Avalonia;
using Avalonia.Media;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;
using Notewright.UI.ViewModels;

namespace Notewright.UI.Views.Player;

internal sealed class DiagnosticRenderer : IBackgroundRenderer
{
	private static readonly Pen SquigglePen = new(new SolidColorBrush(Color.Parse("#f87171")));

	public IReadOnlyList<DiagnosticItem> Diagnostics { get; set; } = [];
	public KnownLayer Layer => KnownLayer.Selection;

	public void Draw(TextView textView, DrawingContext drawingContext)
	{
		if (Diagnostics.Count == 0 || textView.Document is null || !textView.VisualLinesValid)
		{
			return;
		}

		int length = textView.Document.TextLength;
		foreach (DiagnosticItem d in Diagnostics)
		{
			if (d.Start >= length)
			{
				continue;
			}

			TextSegment segment = new()
			{
				StartOffset = d.Start,
				Length = Math.Min(Math.Max(1, d.Length), length - d.Start),
			};
			foreach (Rect rect in BackgroundGeometryBuilder.GetRectsForSegment(textView, segment))
			{
				DrawSquiggle(drawingContext, rect);
			}
		}
	}

	private static void DrawSquiggle(DrawingContext context, Rect rect)
	{
		const double step = 2,
			height = 2;
		double baseline = rect.Bottom - height;
		StreamGeometry geometry = new();
		using (StreamGeometryContext g = geometry.Open())
		{
			g.BeginFigure(new Point(rect.Left, baseline), false);
			bool down = true;
			for (double x = rect.Left + step; x < rect.Right + step; x += step)
			{
				g.LineTo(new Point(Math.Min(x, rect.Right), baseline + (down ? height : 0)));
				down = !down;
			}

			g.EndFigure(false);
		}

		context.DrawGeometry(null, SquigglePen, geometry);
	}
}
