using Avalonia;

namespace Notewright.UI;

internal static class Program
{
	[STAThread]
	internal static void Main(string[] args) =>
		BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);

	private static AppBuilder BuildAvaloniaApp() =>
		AppBuilder.Configure<App>().UsePlatformDetect().WithInterFont().LogToTrace();
}
