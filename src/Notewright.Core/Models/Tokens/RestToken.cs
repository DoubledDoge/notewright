namespace Notewright.Core.Models.Tokens;

public sealed record RestToken(string Raw, int Units) : TokenBase(Raw);
