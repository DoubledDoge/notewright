using System.Diagnostics.CodeAnalysis;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Notewright.Input;
using Notewright.UI.Services;
using Notewright.UI.ViewModels;
using Notewright.UI.Views;

namespace Notewright.UI;

internal sealed class App : Application
{
	public override void Initialize() => AvaloniaXamlLoader.Load(this);

	[SuppressMessage(
		"Reliability",
		"CA2000:Dispose objects before losing scope",
		Justification = "Disposal is handled by ShutdownRequested on success and catch block on failure."
	)]
	public override void OnFrameworkInitializationCompleted()
	{
		if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
		{
			IKeyboardSimulator? simulator = null;
			IGlobalHotkeyService? hotkeyService = null;
			PlaybackViewModel? playbackViewModel = null;
			HotkeyBindings bindings = new();

			try
			{
				simulator = SimulatorFactory.CreateKeyboardSimulator();
				hotkeyService = SimulatorFactory.CreateHotkeyService();
				playbackViewModel = new(simulator, hotkeyService, bindings);

				StatusBarViewModel statusBarViewModel = new();
				MainWindow mainWindow = new();
				FileService fileService = new(mainWindow);
				SheetViewModel sheetViewModel = new(
					fileService,
					playbackViewModel,
					statusBarViewModel
				);

				SettingsViewModel settingsViewModel = new(
					hotkeyService,
					playbackViewModel,
					statusBarViewModel,
					bindings,
					() => { }
				);

				MainWindowViewModel mainWindowViewModel = new(
					sheetViewModel,
					playbackViewModel,
					settingsViewModel,
					statusBarViewModel
				);

				settingsViewModel.SetNavigateBack(mainWindowViewModel.NavigateBack);

				mainWindow.DataContext = mainWindowViewModel;
				desktop.MainWindow = mainWindow;

				desktop.ShutdownRequested += (_, _) =>
				{
					playbackViewModel.Dispose();
					simulator.Dispose();
					hotkeyService.Dispose();
				};
			}
			catch
			{
				playbackViewModel?.Dispose();
				hotkeyService?.Dispose();
				simulator?.Dispose();
				throw;
			}
		}

		base.OnFrameworkInitializationCompleted();
	}
}
