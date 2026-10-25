namespace Notewright.Core;

public static class HotkeyKeys
{
	public static IReadOnlySet<char> Supported { get; } =
		new HashSet<char> { '-', '=', '\\', '/', ';', '\'', '`' };
}
