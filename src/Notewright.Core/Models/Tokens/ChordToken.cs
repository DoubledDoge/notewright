namespace Notewright.Core.Models.Tokens;

public sealed record ChordToken(string Raw, IReadOnlyList<NoteToken> Notes) : TokenBase(Raw);
