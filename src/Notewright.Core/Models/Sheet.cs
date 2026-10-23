using Notewright.Core.Models.Tokens;

namespace Notewright.Core.Models;

public sealed record Sheet(IReadOnlyList<TokenBase> Tokens, PlaybackConfig Config);
