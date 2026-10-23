namespace Notewright.Core.Parsing;

using Models;
using Models.Tokens;

public static class Parser
{
	public static ParseResult<Sheet> Parse(
		ParseResult<IReadOnlyList<TokenBase>> lexResult,
		PlaybackConfig config
	)
	{
		ArgumentNullException.ThrowIfNull(lexResult);
		ArgumentNullException.ThrowIfNull(config);

		if (lexResult is { IsSuccess: false })
		{
			return new ParseResult<Sheet>(Value: null, IsSuccess: false, Errors: lexResult.Errors);
		}

		List<string> errors = [];

		ValidateTokenSequence(lexResult.Value!, errors);

		return errors.Count > 0
			? new ParseResult<Sheet>(Value: null, IsSuccess: false, Errors: errors.AsReadOnly())
			: new ParseResult<Sheet>(
				Value: new Sheet(lexResult.Value!, config),
				IsSuccess: true,
				Errors: []
			);
	}

	private static void ValidateTokenSequence(IReadOnlyList<TokenBase> tokens, List<string> errors)
	{
		foreach (TokenBase token in tokens)
		{
			switch (token)
			{
				case ChordToken { Notes.Count: 0 } chord:
					errors.Add($"Empty chord token at raw '{chord.Raw}'.");
					break;

				case RollToken { Notes.Count: 0 } roll:
					errors.Add($"Empty roll token at raw '{roll.Raw}'.");
					break;

				case LegatoRunToken { Notes.Count: < 2 } legato:
					errors.Add(
						$"Legato run token at raw '{legato.Raw}' must contain at least two notes."
					);
					break;

				case RestToken { Units: < 1 } rest:
					errors.Add($"Rest token at raw '{rest.Raw}' has invalid unit count.");
					break;
			}
		}
	}
}
