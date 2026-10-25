using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Timers;
using System.Windows.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Timer = System.Timers.Timer;

namespace Notewright.UI.ViewModels;

using Core;
using Core.Models;
using Core.Models.Tokens;
using Enums;
using Input;
using Services;
using Services.Enums;

internal sealed partial class PlaybackViewModel : ObservableObject, IDisposable
{
	private const string ManualReadyText = "Manual mode - Press hotkey to advance";
	private readonly Timer _elapsedTimer;
	private readonly HotkeyCoordinator _hotkeys;
	private readonly IKeyboardSimulator _simulator;
	private CancellationTokenSource? _cts;
	private TimeSpan _elapsedBeforePause;
	private int _manualBeatIndex;
	private IReadOnlyList<TokenBase> _manualTokens = [];
	private DateTime _playbackStarted;
	private int _resumeIndex;
	private double _sheetBeats;
	private double _sheetFixedMs;

	private const decimal MinBpm = 20;
	private const decimal MaxBpm = 300;
	private const decimal MinTransposition = -12;
	private const decimal MaxTransposition = 12;

	public PlaybackViewModel(IKeyboardSimulator simulator, HotkeyCoordinator hotkeys)
	{
		_simulator = simulator;
		_hotkeys = hotkeys;
		_elapsedTimer = new Timer(100) { AutoReset = true };
		_elapsedTimer.Elapsed += OnElapsedTimerTick;

		hotkeys.Add(HotkeyAction.Advance, HotkeyModes.Manual, OnManualBeatPressed);
		hotkeys.Add(HotkeyAction.Play, HotkeyModes.Automated, () => RunOnUiThread(PlayCommand));
		hotkeys.Add(HotkeyAction.Pause, HotkeyModes.Automated, () => RunOnUiThread(PauseCommand));
		hotkeys.Add(HotkeyAction.Stop, HotkeyModes.Both, () => RunOnUiThread(StopCommand));
	}

	[
		ObservableProperty,
		NotifyCanExecuteChangedFor(nameof(PlayCommand)),
		NotifyCanExecuteChangedFor(nameof(PauseCommand)),
		NotifyCanExecuteChangedFor(nameof(StopCommand)),
	]
	private partial PlaybackStatus Status { get; set; } = PlaybackStatus.Idle;

	[ObservableProperty]
	public partial decimal Bpm { get; set; } = 120;

	[ObservableProperty]
	public partial decimal Transposition { get; set; }

	[ObservableProperty]
	public partial string BpmText { get; set; } = "120";

	[ObservableProperty]
	public partial string TranspositionText { get; set; } = "+0";

	[ObservableProperty, NotifyCanExecuteChangedFor(nameof(PlayCommand))]
	private partial Sheet? Sheet { get; set; }

	[
		ObservableProperty,
		NotifyPropertyChangedFor(nameof(ManualModeToggleText)),
		NotifyCanExecuteChangedFor(nameof(PlayCommand)),
		NotifyCanExecuteChangedFor(nameof(StopCommand))
	]
	public partial bool IsManualMode { get; set; }

	[ObservableProperty, NotifyPropertyChangedFor(nameof(ElapsedTimeFormatted))]
	private partial TimeSpan ElapsedTime { get; set; }

	[ObservableProperty, NotifyPropertyChangedFor(nameof(TotalDurationFormatted))]
	private partial TimeSpan TotalDuration { get; set; }

	[ObservableProperty]
	private partial int CurrentBeat { get; set; }

	[ObservableProperty]
	public partial int TotalBeats { get; set; }

	[ObservableProperty]
	public partial string StatusText { get; set; } = "Ready";

	public PlaybackConfig CurrentConfig => new((int)Bpm, (int)Transposition);

	public string ElapsedTimeFormatted =>
		ElapsedTime.ToString(@"mm\:ss", CultureInfo.InvariantCulture);

	public string TotalDurationFormatted =>
		TotalDuration.ToString(@"mm\:ss", CultureInfo.InvariantCulture);

	public string ManualModeToggleText => IsManualMode ? "Switch to auto" : "Switch to manual";

	private bool CanPlay => Status != PlaybackStatus.Playing && Sheet is not null && !IsManualMode;
	private bool CanPause => Status == PlaybackStatus.Playing;

	private bool CanStop =>
		Status is PlaybackStatus.Playing or PlaybackStatus.Paused || IsManualMode;

	public void Dispose()
	{
		_cts?.Dispose();
		_cts = null;
		_elapsedTimer.Dispose();
		GC.SuppressFinalize(this);
	}

	private static void RunOnUiThread(ICommand command) =>
		Dispatcher.UIThread.Post(() =>
		{
			if (command.CanExecute(null))
			{
				command.Execute(null);
			}
		});

	public void SetWindowActive(bool active) => _hotkeys.SetWindowActive(active);

	public void UpdateSheet(Sheet sheet)
	{
		ArgumentNullException.ThrowIfNull(sheet);
		Sheet = sheet;
		_manualTokens = [.. sheet.Tokens.Where(t => t is not RestToken)];

		_sheetBeats = 0;
		_sheetFixedMs = 0;
		foreach (TokenBase token in sheet.Tokens)
		{
			(double beats, double fixedMs) = PlaybackTiming.Measure(token);
			_sheetBeats += beats;
			_sheetFixedMs += fixedMs;
		}

		RefreshBeatCount();
		RefreshTotalDuration();
		_cts?.Cancel();
		ResetProgress();
	}

	public void ClearSheet()
	{
		Sheet = null;
		_manualTokens = [];
		_sheetBeats = 0;
		_sheetFixedMs = 0;
		RefreshBeatCount();
		RefreshTotalDuration();
		_cts?.Cancel();
		ResetProgress();
		PlayCommand.NotifyCanExecuteChanged();
	}

	partial void OnBpmChanged(decimal value)
	{
		BpmText = FormatBpm(value);
		RefreshTotalDuration();
	}

	partial void OnTranspositionChanged(decimal value) =>
		TranspositionText = FormatTransposition(value);

	private void RefreshTotalDuration() =>
		TotalDuration = TimeSpan.FromMilliseconds(
			_sheetBeats * PlaybackTiming.MillisecondsPerMinute / (double)Math.Max(Bpm, 1)
				+ _sheetFixedMs
		);

	private void RefreshBeatCount() =>
		TotalBeats = IsManualMode ? _manualTokens.Count : Sheet?.Tokens.Count ?? 0;

	private void OnElapsedTimerTick(object? sender, ElapsedEventArgs e) =>
		Dispatcher.UIThread.Post(() =>
		{
			if (Status != PlaybackStatus.Playing)
			{
				return;
			}

			ElapsedTime = _elapsedBeforePause + (DateTime.UtcNow - _playbackStarted);
			StatusText = $"Playing · Beat {CurrentBeat} of {TotalBeats}";
		});

	partial void OnIsManualModeChanged(bool value)
	{
		_hotkeys.SetManualMode(value);
		RefreshBeatCount();
		_cts?.Cancel(); // leaving automated mode ends any run in progress
		ResetProgress();

		if (value)
		{
			StatusText = ManualReadyText;
		}
	}

	[SuppressMessage(
		"Design",
		"CA1031:Do not catch general exception types",
		Justification = "Catches general playback exceptions to prevent crashing async void UI event handlers."
	)]
	private void OnManualBeatPressed() =>
		Dispatcher.UIThread.Post(async void () =>
		{
			try
			{
				if (Sheet is null || _manualBeatIndex >= _manualTokens.Count)
				{
					return;
				}

				int beatIndex = _manualBeatIndex++;
				CurrentBeat = _manualBeatIndex;
				StatusText = $"Manual · Beat {CurrentBeat} of {TotalBeats}";

				PlaybackEngine engine = new(_simulator);
				await engine
					.PlayTokenAsync(
						_manualTokens[beatIndex],
						CurrentConfig,
						true,
						CancellationToken.None
					)
					.ConfigureAwait(true);

				if (_manualBeatIndex < _manualTokens.Count)
				{
					return;
				}

				_manualBeatIndex = 0;
				CurrentBeat = 0;
				IsManualMode = false;
			}
			catch (Exception e)
			{
				Debug.WriteLine($"Playback error: {e.Message}");
				StatusText = $"Error: {e.Message}";
			}
		});

	[RelayCommand(CanExecute = nameof(CanPlay))]
	private async Task PlayAsync()
	{
		Sheet? sheet = Sheet;
		if (sheet is null)
		{
			return;
		}

		_cts?.Dispose();
		_cts = new CancellationTokenSource();
		Status = PlaybackStatus.Playing;
		_playbackStarted = DateTime.UtcNow;
		_elapsedTimer.Start();

		try
		{
			PlaybackEngine engine = new(_simulator);
			await engine
				.PlayAsync(sheet, () => CurrentConfig, _resumeIndex, OnTokenStarted, _cts.Token)
				.ConfigureAwait(true);
			ResetProgress(); // reached the end
		}
		catch (OperationCanceledException)
		{
			// Pause() and Stop() have already set the status
		}
		finally
		{
			_elapsedTimer.Stop();
		}
	}

	[RelayCommand(CanExecute = nameof(CanPause))]
	private void Pause()
	{
		_elapsedBeforePause += DateTime.UtcNow - _playbackStarted;
		ElapsedTime = _elapsedBeforePause;
		Status = PlaybackStatus.Paused;
		StatusText = $"Paused · Beat {CurrentBeat} of {TotalBeats}";
		_cts?.Cancel();
	}

	[RelayCommand(CanExecute = nameof(CanStop))]
	private void Stop()
	{
		_cts?.Cancel();
		ResetProgress();

		if (IsManualMode)
		{
			StatusText = ManualReadyText;
		}
	}

	private void OnTokenStarted(int index)
	{
		_resumeIndex = index + 1;
		CurrentBeat = index + 1; // auto mode never advanced this before
	}

	private void ResetProgress()
	{
		_resumeIndex = 0;
		_manualBeatIndex = 0;
		_elapsedBeforePause = TimeSpan.Zero;
		ElapsedTime = TimeSpan.Zero;
		CurrentBeat = 0;
		Status = PlaybackStatus.Idle;
		StatusText = Sheet is null ? "Ready" : $"Ready · Beat 1 of {TotalBeats}";
	}

	[RelayCommand]
	private void CommitBpm()
	{
		Bpm = ParseClamped(BpmText, Bpm, MinBpm, MaxBpm);
		BpmText = FormatBpm(Bpm); // potentially snaps the box back
	}

	[RelayCommand]
	private void CommitTransposition()
	{
		Transposition = ParseClamped(
			TranspositionText,
			Transposition,
			MinTransposition,
			MaxTransposition
		);
		TranspositionText = FormatTransposition(Transposition);
	}

	private static decimal ParseClamped(string? text, decimal fallback, decimal min, decimal max) =>
		decimal.TryParse(
			text?.Trim(),
			NumberStyles.AllowLeadingSign,
			CultureInfo.InvariantCulture,
			out decimal value
		)
			? Math.Clamp(value, min, max)
			: fallback;

	private static string FormatBpm(decimal value) =>
		value.ToString("0", CultureInfo.InvariantCulture);

	private static string FormatTransposition(decimal value) =>
		value.ToString("+0;-0;+0", CultureInfo.InvariantCulture);

	[RelayCommand]
	private void IncrementBpm() => Bpm = Math.Min(MaxBpm, Bpm + 1);

	[RelayCommand]
	private void DecrementBpm() => Bpm = Math.Max(MinBpm, Bpm - 1);

	[RelayCommand]
	private void IncrementTransposition() =>
		Transposition = Math.Min(MaxTransposition, Transposition + 1);

	[RelayCommand]
	private void DecrementTransposition() =>
		Transposition = Math.Max(MinTransposition, Transposition - 1);

	[RelayCommand]
	private void ToggleManualMode() => IsManualMode = !IsManualMode;

	~PlaybackViewModel() => Dispose();
}
