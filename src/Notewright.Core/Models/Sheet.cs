namespace Notewright.Core.Models;

using Tokens;

public sealed record Sheet(IReadOnlyList<TokenBase> Tokens, PlaybackConfig Config);
