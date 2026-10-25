using Notewright.Core;
using Notewright.Core.Models;
using Notewright.Core.Models.Tokens;

namespace Notewright.Input;

public sealed class PlaybackEngine(IKeyboardSimulator simulator)
{
	public async Task PlayAsync(
		Sheet sheet,
		Func<PlaybackConfig> getConfig,
		int startIndex = 0,
		Action<int>? onTokenStarted = null,
		CancellationToken cancellationToken = default
	)
	{
		ArgumentNullException.ThrowIfNull(sheet);
		ArgumentNullException.ThrowIfNull(getConfig);

		for (int i = startIndex; i < sheet.Tokens.Count; i++)
		{
			cancellationToken.ThrowIfCancellationRequested();
			onTokenStarted?.Invoke(i);

			await PlayTokenAsync(sheet.Tokens[i], getConfig(), false, cancellationToken)
				.ConfigureAwait(true);
		}
	}

	public async Task PlayTokenAsync(
		TokenBase token,
		PlaybackConfig config,
		bool manual = false,
		CancellationToken cancellationToken = default
	)
	{
		ArgumentNullException.ThrowIfNull(token);
		ArgumentNullException.ThrowIfNull(config);

		double msPerBeat = PlaybackTiming.MillisecondsPerMinute / config.Bpm;

		switch (token)
		{
			case NoteToken note:
				simulator.KeyPress(note);

				if (!manual)
				{
					await ApplyDelayAsync(note.DelayAfter, msPerBeat, cancellationToken)
						.ConfigureAwait(true);
				}

				break;

			case ChordToken chord:
				foreach (NoteToken note in chord.Notes)
				{
					simulator.KeyPress(note);
				}

				if (!manual)
				{
					await Task.Delay(TimeSpan.FromMilliseconds(msPerBeat), cancellationToken)
						.ConfigureAwait(true);
				}

				break;

			case RollToken roll:
				foreach (NoteToken note in roll.Notes)
				{
					simulator.KeyPress(note);
					await Task.Delay(
							TimeSpan.FromMilliseconds(PlaybackTiming.RollDelayMs),
							cancellationToken
						)
						.ConfigureAwait(true);
				}

				if (!manual)
				{
					await Task.Delay(TimeSpan.FromMilliseconds(msPerBeat), cancellationToken)
						.ConfigureAwait(true);
				}

				break;

			case LegatoRunToken legato:
				foreach (NoteToken note in legato.Notes)
				{
					simulator.KeyPress(note);
					await Task.Delay(
							TimeSpan.FromMilliseconds(
								msPerBeat * PlaybackTiming.LegatoBeatFraction
							),
							cancellationToken
						)
						.ConfigureAwait(true);
				}

				break;

			case RestToken rest when !manual:
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
			DelayAfter.Short => msPerBeat * PlaybackTiming.ShortDelayBeats,
			DelayAfter.Medium => msPerBeat * PlaybackTiming.MediumDelayBeats,
			_ => msPerBeat,
		};

		await Task.Delay(TimeSpan.FromMilliseconds(ms), cancellationToken).ConfigureAwait(true);
	}
}
