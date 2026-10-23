namespace Notewright.Core.Models.Tokens;

public sealed record LegatoRunToken(string Raw, IReadOnlyList<NoteToken> Notes) : TokenBase(Raw);
