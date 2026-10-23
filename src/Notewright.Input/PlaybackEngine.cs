namespace Notewright.Input;

using Core.Models;
using Core.Models.Tokens;

public sealed class PlaybackEngine(IKeyboardSimulator simulator)
{
	private const int RollDelay = 1;
	private const double LegatoDelayFraction = 4;
	private const double ShortDelayFraction = 2;
	private const double MediumDelayFraction = 0.75;
	private const double MillisecondsPerMinute = 60000;

	public async Task PlayAsync(Sheet sheet, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(sheet);

		double msPerBeat = MillisecondsPerMinute / sheet.Config.Bpm;

		foreach (TokenBase token in sheet.Tokens)
		{
			cancellationToken.ThrowIfCancellationRequested();

			switch (token)
			{
				case NoteToken note:
					simulator.KeyPress(note);

					await ApplyDelayAsync(note.DelayAfter, msPerBeat, cancellationToken)
						.ConfigureAwait(true);
					break;

				case ChordToken chord:
					foreach (NoteToken note in chord.Notes)
					{
						simulator.KeyPress(note);
					}

					await Task.Delay(TimeSpan.FromMilliseconds(msPerBeat), cancellationToken)
						.ConfigureAwait(true);
					break;

				case RollToken roll:
					foreach (NoteToken note in roll.Notes)
					{
						simulator.KeyPress(note);

						await Task.Delay(TimeSpan.FromMilliseconds(RollDelay), cancellationToken)
							.ConfigureAwait(true);
					}
					await Task.Delay(TimeSpan.FromMilliseconds(msPerBeat), cancellationToken)
						.ConfigureAwait(true);
					break;

				case LegatoRunToken legato:
					foreach (NoteToken note in legato.Notes)
					{
						simulator.KeyPress(note);

						await Task.Delay(
								TimeSpan.FromMilliseconds(msPerBeat / LegatoDelayFraction),
								cancellationToken
							)
							.ConfigureAwait(true);
					}
					break;

				case RestToken rest:
					await Task.Delay(
							TimeSpan.FromMilliseconds(msPerBeat * rest.Units),
							cancellationToken
						)
						.ConfigureAwait(true);
					break;
			}
		}
	}

	public async Task PlayTokenAsync(
		TokenBase token,
		PlaybackConfig config,
		CancellationToken cancellationToken = default
	)
	{
		ArgumentNullException.ThrowIfNull(token);
		ArgumentNullException.ThrowIfNull(config);

		double msPerBeat = MillisecondsPerMinute / config.Bpm;

		switch (token)
		{
			case NoteToken note:
				simulator.KeyPress(note);
				await ApplyDelayAsync(note.DelayAfter, msPerBeat, cancellationToken)
					.ConfigureAwait(true);
				break;

			case ChordToken chord:
				foreach (NoteToken note in chord.Notes)
				{
					simulator.KeyPress(note);
				}

				await Task.Delay(TimeSpan.FromMilliseconds(msPerBeat), cancellationToken)
					.ConfigureAwait(true);
				break;

			case RollToken roll:
				foreach (NoteToken note in roll.Notes)
				{
					simulator.KeyPress(note);
					await Task.Delay(TimeSpan.FromMilliseconds(RollDelay), cancellationToken)
						.ConfigureAwait(true);
				}
				await Task.Delay(TimeSpan.FromMilliseconds(msPerBeat), cancellationToken)
					.ConfigureAwait(true);
				break;

			case LegatoRunToken legato:
				foreach (NoteToken note in legato.Notes)
				{
					simulator.KeyPress(note);
					await Task.Delay(
							TimeSpan.FromMilliseconds(msPerBeat / LegatoDelayFraction),
							cancellationToken
						)
						.ConfigureAwait(true);
				}
				break;

			case RestToken rest:
				await Task.Delay(
						TimeSpan.FromMilliseconds(msPerBeat * rest.Units),
						cancellationToken
					)
					.ConfigureAwait(true);
				break;
		}
	}

	private static async Task ApplyDelayAsync(
		DelayAfter delay,
		double msPerBeat,
		CancellationToken cancellationToken
	)
	{
		double ms = delay switch
		{
			DelayAfter.None => msPerBeat,
			DelayAfter.Short => msPerBeat / ShortDelayFraction,
			DelayAfter.Medium => msPerBeat * MediumDelayFraction,
			_ => msPerBeat,
		};

		await Task.Delay(TimeSpan.FromMilliseconds(ms), cancellationToken).ConfigureAwait(true);
	}
}
