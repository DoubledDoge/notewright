using Notewright.Core.Models.Tokens;

namespace Notewright.Core;

public static class PlaybackTiming
{
	public const double MillisecondsPerMinute = 60000;
	public const double RollDelayMs = 1;
	public const double LegatoBeatFraction = 0.25;
	public const double ShortDelayBeats = 0.5;
	public const double MediumDelayBeats = 0.75;

	// Beats scale with tempo
	public static (double Beats, double FixedMs) Measure(TokenBase token)
	{
		ArgumentNullException.ThrowIfNull(token);

		return token switch
		{
			NoteToken note => (
				note.DelayAfter switch
				{
					DelayAfter.Short => ShortDelayBeats,
					DelayAfter.Medium => MediumDelayBeats,
					_ => 1.0,
				},
				0.0
			),
			RollToken roll => (1.0, roll.Notes.Count * RollDelayMs),
			LegatoRunToken legato => (legato.Notes.Count * LegatoBeatFraction, 0.0),
			RestToken rest => (rest.Units, 0.0),
			_ => (1.0, 0.0), // chord
		};
	}
}
