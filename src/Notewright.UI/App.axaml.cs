using System.Diagnostics.CodeAnalysis;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace Notewright.UI;

using Input;
using Services;
using ViewModels;
using Views;

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
				HotkeyCoordinator hotkeys = new(hotkeyService, bindings);
				playbackViewModel = new(simulator, hotkeys);

				StatusBarViewModel statusBarViewModel = new();
				MainWindow mainWindow = new();
				FileService fileService = new(mainWindow);
				SheetViewModel sheetViewModel = new(
					fileService,
					playbackViewModel,
					statusBarViewModel
				);

				SettingsViewModel settingsViewModel = new(
					bindings,
					hotkeys,
					statusBarViewModel,
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
