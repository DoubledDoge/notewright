using Notewright.Core.Models;
using Notewright.Core.Models.Tokens;

namespace Notewright.Core.Parsing;

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
