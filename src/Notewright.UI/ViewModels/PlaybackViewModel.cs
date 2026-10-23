using System.Globalization;
using System.Timers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Notewright.UI.ViewModels;

using Core.Models;
using Core.Models.Tokens;
using Enums;
using Input;

internal sealed partial class PlaybackViewModel : ObservableObject, IDisposable
{
	private readonly IKeyboardSimulator _simulator;
	private readonly IGlobalHotkeyService _hotkeyService;
	private readonly System.Timers.Timer _elapsedTimer;
	private CancellationTokenSource? _cts;
	private DateTime _playbackStarted;
	private TimeSpan _elapsedBeforePause;

	public PlaybackViewModel(IKeyboardSimulator simulator, IGlobalHotkeyService hotkeyService)
	{
		_simulator = simulator;
		_hotkeyService = hotkeyService;
		_elapsedTimer = new System.Timers.Timer(100) { AutoReset = true };
		_elapsedTimer.Elapsed += OnElapsedTimerTick;
	}

	[
		ObservableProperty,
		NotifyCanExecuteChangedFor(nameof(PlayCommand)),
		NotifyCanExecuteChangedFor(nameof(PauseCommand)),
		NotifyCanExecuteChangedFor(nameof(StopCommand)),
	]
	public partial PlaybackStatus Status { get; set; } = PlaybackStatus.Idle;

	[ObservableProperty]
	public partial decimal Bpm { get; set; } = 120;

	[ObservableProperty]
	public partial decimal Transposition { get; set; }

	[ObservableProperty, NotifyCanExecuteChangedFor(nameof(PlayCommand))]
	public partial Sheet? Sheet { get; set; }

	[ObservableProperty, NotifyPropertyChangedFor(nameof(ManualModeToggleText))]
	public partial bool IsManualMode { get; set; }

	[ObservableProperty, NotifyPropertyChangedFor(nameof(ElapsedTimeFormatted))]
	public partial TimeSpan ElapsedTime { get; set; }

	[ObservableProperty, NotifyPropertyChangedFor(nameof(TotalDurationFormatted))]
	public partial TimeSpan TotalDuration { get; set; }

	[ObservableProperty]
	public partial int CurrentBeat { get; set; }

	[ObservableProperty]
	public partial int TotalBeats { get; set; }

	[ObservableProperty]
	public partial string StatusText { get; set; } = "Ready";

	private int _manualBeatIndex;

	public PlaybackConfig CurrentConfig => new((int)Bpm, (int)Transposition);

	public string ElapsedTimeFormatted =>
		ElapsedTime.ToString(@"mm\:ss", CultureInfo.InvariantCulture);
	public string TotalDurationFormatted =>
		TotalDuration.ToString(@"mm\:ss", CultureInfo.InvariantCulture);
	public string ManualModeToggleText => IsManualMode ? "Switch to auto" : "Switch to manual";

	public void UpdateSheet(Sheet sheet)
	{
		ArgumentNullException.ThrowIfNull(sheet);
		System.Diagnostics.Debug.WriteLine($"Sheet updated: {sheet.Tokens.Count} tokens");
		Sheet = sheet;
		TotalBeats = sheet.Tokens.Count;
		TotalDuration = CalculateTotalDuration(sheet);
		CurrentBeat = 0;
		ElapsedTime = TimeSpan.Zero;
		StatusText = $"Ready · Beat 1 of {TotalBeats}";
	}

	public void ClearSheet()
	{
		Sheet = null;
		TotalBeats = 0;
		TotalDuration = TimeSpan.Zero;
		CurrentBeat = 0;
		ElapsedTime = TimeSpan.Zero;
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
		Avalonia.Threading.Dispatcher.UIThread.Post(() =>
		{
			ElapsedTime = _elapsedBeforePause + (DateTime.UtcNow - _playbackStarted);
			StatusText = $"Playing · Beat {CurrentBeat} of {TotalBeats}";
		});

	partial void OnIsManualModeChanged(bool value)
	{
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

	private void OnManualBeatPressed() =>
		Avalonia.Threading.Dispatcher.UIThread.Post(async () =>
		{
			if (Sheet is null || _manualBeatIndex >= Sheet.Tokens.Count)
			{
				return;
			}

			int beatIndex = _manualBeatIndex;
			_manualBeatIndex++;
			CurrentBeat = _manualBeatIndex;
			StatusText = $"Manual · Beat {CurrentBeat} of {TotalBeats}";

			PlaybackEngine engine = new(_simulator);
			await engine
				.PlayTokenAsync(Sheet.Tokens[beatIndex], CurrentConfig, CancellationToken.None)
				.ConfigureAwait(true);

			if (_manualBeatIndex >= Sheet.Tokens.Count)
			{
				_manualBeatIndex = 0;
				CurrentBeat = 0;
				IsManualMode = false;
			}
		});

	private bool CanPlay => Status == PlaybackStatus.Idle && Sheet is not null;
	private bool CanPause => Status == PlaybackStatus.Playing;
	private bool CanStop => Status is PlaybackStatus.Playing or PlaybackStatus.Paused;

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

	public void Dispose()
	{
		_cts?.Dispose();
		_cts = null;
		_elapsedTimer.Dispose();
		GC.SuppressFinalize(this);
	}
}
