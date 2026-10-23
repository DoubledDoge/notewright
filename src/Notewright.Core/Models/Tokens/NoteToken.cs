namespace Notewright.Core.Models.Tokens;

public sealed record NoteToken(
	string Raw,
	char Key,
	bool IsShifted,
	DelayAfter DelayAfter = DelayAfter.None
) : TokenBase(Raw);
