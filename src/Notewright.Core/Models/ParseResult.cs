namespace Notewright.Core.Models;

public sealed record ParseResult<T>(T? Value, bool IsSuccess, IReadOnlyList<string> Errors);
