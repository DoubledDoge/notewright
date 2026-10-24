using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Timers;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Notewright.Core.Models;
using Notewright.Core.Models.Tokens;
using Notewright.Input;
using Notewright.UI.ViewModels.Enums;
using Timer = System.Timers.Timer;

namespace Notewright.UI.ViewModels;

internal sealed partial class PlaybackViewModel : ObservableObject, IDisposable
{
	private readonly Timer _elapsedTimer;
	private readonly IGlobalHotkeyService _hotkeyService;
	private readonly IKeyboardSimulator _simulator;
	private CancellationTokenSource? _cts;
	private TimeSpan _elapsedBeforePause;
	private int _manualBeatIndex;
	private IReadOnlyList<TokenBase> _manualTokens = [];
	private DateTime _playbackStarted;

	public PlaybackViewModel(IKeyboardSimulator simulator, IGlobalHotkeyService hotkeyService)
	{
		_simulator = simulator;
		_hotkeyService = hotkeyService;
		_elapsedTimer = new Timer(100) { AutoReset = true };
		_elapsedTimer.Elapsed += OnElapsedTimerTick;
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

	[ObservableProperty, NotifyCanExecuteChangedFor(nameof(PlayCommand))]
	private partial Sheet? Sheet { get; set; }

	[ObservableProperty, NotifyPropertyChangedFor(nameof(ManualModeToggleText))]
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

	private bool CanPlay => Status == PlaybackStatus.Idle && Sheet is not null;
	private bool CanPause => Status == PlaybackStatus.Playing;
	private bool CanStop => Status is PlaybackStatus.Playing or PlaybackStatus.Paused;

	public void Dispose()
	{
		_cts?.Dispose();
		_cts = null;
		_elapsedTimer.Dispose();
		GC.SuppressFinalize(this);
	}

	public void UpdateSheet(Sheet sheet)
	{
		ArgumentNullException.ThrowIfNull(sheet);
		Debug.WriteLine($"Sheet updated: {sheet.Tokens.Count} tokens");
		Sheet = sheet;
		_manualTokens = [.. sheet.Tokens.Where(t => t is not RestToken)];
		_manualBeatIndex = 0;
		RefreshBeatCount();
		TotalDuration = CalculateTotalDuration(sheet);
		CurrentBeat = 0;
		ElapsedTime = TimeSpan.Zero;
		StatusText = $"Ready · Beat 1 of {TotalBeats}";
	}

	private void RefreshBeatCount() =>
		TotalBeats = IsManualMode ? _manualTokens.Count : Sheet?.Tokens.Count ?? 0;

	public void ClearSheet()
	{
		Sheet = null;
		TotalBeats = 0;
		TotalDuration = TimeSpan.Zero;
		CurrentBeat = 0;
		ElapsedTime = TimeSpan.Zero;
		_manualTokens = [];
		_manualBeatIndex = 0;
		StatusText = "Ready";
		PlayCommand.NotifyCanExecuteChanged();
	}

	private TimeSpan CalculateTotalDuration(Sheet sheet)
	{
		double msPerBeat = 60000.0 / (double)Bpm;
		double totalMs = sheet.Tokens.Sum(token =>
			token switch
			{
				RestToken rest => msPerBeat * rest.Units,
				LegatoRunToken legato => msPerBeat / 4.0 * legato.Notes.Count,
				RollToken roll => roll.Notes.Count + msPerBeat,
				_ => msPerBeat,
			}
		);

		return TimeSpan.FromMilliseconds(totalMs);
	}

	private void OnElapsedTimerTick(object? sender, ElapsedEventArgs e) =>
		Dispatcher.UIThread.Post(() =>
		{
			ElapsedTime = _elapsedBeforePause + (DateTime.UtcNow - _playbackStarted);
			StatusText = $"Playing · Beat {CurrentBeat} of {TotalBeats}";
		});

	partial void OnIsManualModeChanged(bool value)
	{
		RefreshBeatCount();
		OnPropertyChanged(nameof(ManualModeToggleText));

		if (value)
		{
			_hotkeyService.Register('=', OnManualBeatPressed);
			StatusText = "Manual mode · Press hotkey to advance";
		}
		else
		{
			_hotkeyService.Unregister('=');
			StatusText = Sheet is not null ? $"Ready · Beat 1 of {TotalBeats}" : "Ready";
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
				if (Sheet is null || _manualBeatIndex >= Sheet.Tokens.Count)
				{
					return;
				}

				int beatIndex = _manualBeatIndex++;
				_manualBeatIndex++;
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
		if (Sheet is null)
		{
			return;
		}

		_cts = new CancellationTokenSource();
		Status = PlaybackStatus.Playing;
		_playbackStarted = DateTime.UtcNow;
		_elapsedTimer.Start();

		try
		{
			PlaybackEngine engine = new(_simulator);
			await engine.PlayAsync(Sheet, _cts.Token).ConfigureAwait(true);
			Status = PlaybackStatus.Idle;
			ElapsedTime = TimeSpan.Zero;
			CurrentBeat = 0;
			StatusText = $"Ready · Beat 1 of {TotalBeats}";
		}
		catch (OperationCanceledException)
		{
			Status = PlaybackStatus.Idle;
		}
		finally
		{
			_elapsedTimer.Stop();
		}
	}

	[RelayCommand]
	private void IncrementBpm() => Bpm = Math.Min(300, Bpm + 1);

	[RelayCommand]
	private void DecrementBpm() => Bpm = Math.Max(20, Bpm - 1);

	[RelayCommand]
	private void IncrementTransposition() => Transposition = Math.Min(12, Transposition + 1);

	[RelayCommand]
	private void DecrementTransposition() => Transposition = Math.Max(-12, Transposition - 1);

	[RelayCommand]
	private void ToggleManualMode() => IsManualMode = !IsManualMode;

	[RelayCommand(CanExecute = nameof(CanPause))]
	private async Task PauseAsync()
	{
		if (_cts is null)
		{
			return;
		}

		await _cts.CancelAsync().ConfigureAwait(true);
		_elapsedBeforePause = ElapsedTime;
		_elapsedTimer.Stop();
		Status = PlaybackStatus.Paused;
		StatusText = $"Paused · Beat {CurrentBeat} of {TotalBeats}";
	}

	[RelayCommand(CanExecute = nameof(CanStop))]
	private async Task StopAsync()
	{
		if (_cts is null)
		{
			return;
		}

		await _cts.CancelAsync().ConfigureAwait(true);
		_elapsedTimer.Stop();
		ElapsedTime = TimeSpan.Zero;
		_elapsedBeforePause = TimeSpan.Zero;
		CurrentBeat = 0;
		_manualBeatIndex = 0;
		Status = PlaybackStatus.Idle;
		StatusText = $"Ready · Beat 1 of {TotalBeats}";
	}

	~PlaybackViewModel() => Dispose();
}
