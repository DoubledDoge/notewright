namespace Notewright.UI.ViewModels;

internal sealed record DiagnosticItem(int Start, int Length, int Line, int Column, string Message)
{
	public string Location => $"Line {Line}, col {Column}";
}
