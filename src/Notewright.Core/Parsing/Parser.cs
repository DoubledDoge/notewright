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

		return lexResult.IsSuccess
			? new ParseResult<Sheet>(new Sheet(lexResult.Value!, config), true, [])
			: new ParseResult<Sheet>(null, false, lexResult.Errors);
	}
}
