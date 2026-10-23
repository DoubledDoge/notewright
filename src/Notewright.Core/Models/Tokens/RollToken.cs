namespace Notewright.Core.Models.Tokens;

public sealed record RollToken(string Raw, IReadOnlyList<NoteToken> Notes) : TokenBase(Raw);
