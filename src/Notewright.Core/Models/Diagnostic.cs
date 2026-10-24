namespace Notewright.Core.Models;

public sealed record Diagnostic(int Start, int Length, string Message);
