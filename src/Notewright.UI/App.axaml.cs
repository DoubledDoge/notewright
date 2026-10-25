using System.Diagnostics.CodeAnalysis;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace Notewright.UI;

using Input;
using Services;
using Services.Enums;
using ViewModels;
using Views;

[SuppressMessage("ReSharper", "AccessToDisposedClosure")]
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
			SettingsStore settingsStore = new();
			AppSettings saved = settingsStore.Load();
			bindings.Apply(saved.Hotkeys);

			try
			{
				simulator = SimulatorFactory.CreateKeyboardSimulator();
				hotkeyService = SimulatorFactory.CreateHotkeyService();
				HotkeyCoordinator hotkeys = new(hotkeyService, bindings);
				playbackViewModel = new(simulator, hotkeys);

				playbackViewModel.Restore(saved.Bpm, saved.Transposition);

				settingsStore.Attach(() =>
					new AppSettings
					{
						Hotkeys = new Dictionary<HotkeyAction, char>(bindings.Snapshot()),
						Bpm = (int)playbackViewModel.Bpm,
						Transposition = (int)playbackViewModel.Transposition,
					}
				);

				// loading doesn't trigger a pointless save
				hotkeys.BindingsChanged += (_, _) => settingsStore.RequestSave();
				playbackViewModel.PropertyChanged += (_, e) =>
				{
					if (
						e.PropertyName
						is nameof(PlaybackViewModel.Bpm)
							or nameof(PlaybackViewModel.Transposition)
					)
					{
						settingsStore.RequestSave();
					}
				};

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
					settingsStore.Flush();
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
