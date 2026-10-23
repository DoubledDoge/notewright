using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Notewright.Core.Models;
using Notewright.Core.Models.Tokens;
using Notewright.Core.Parsing;
using Notewright.UI.Services;
using Notewright.UI.ViewModels.Enums;

namespace Notewright.UI.ViewModels;

internal sealed partial class SheetViewModel : ObservableObject
{
	private readonly FileService _fileService;
	private readonly PlaybackViewModel _playbackViewModel;
	private readonly StatusBarViewModel _statusBarViewModel;

	public SheetViewModel(
		FileService fileService,
		PlaybackViewModel playbackViewModel,
		StatusBarViewModel statusBarViewModel
	)
	{
		_fileService = fileService;
		_playbackViewModel = playbackViewModel;
		_statusBarViewModel = statusBarViewModel;

		ImportSheetCommand = new AsyncRelayCommand(ImportSheetAsync);
		ExportSheetCommand = new AsyncRelayCommand(ExportSheetAsync, () => HasSheetText);

		PropertyChanged += (_, e) =>
		{
			if (e.PropertyName == nameof(SheetText))
			{
				((AsyncRelayCommand)ExportSheetCommand).NotifyCanExecuteChanged();
			}
		};
	}

	[ObservableProperty]
	public partial string SheetText { get; set; } = string.Empty;

	[ObservableProperty]
	public partial string? ErrorMessage { get; set; }

	[ObservableProperty]
	private partial Sheet? ParsedSheet { get; set; }

	[ObservableProperty]
	public partial string FileName { get; set; } = "Untitled";

	[ObservableProperty]
	public partial ValidationStatus ValidationStatus { get; set; } = ValidationStatus.Empty;

	public int LineCount => SheetText.Split('\n').Length;

	public IRelayCommand ImportSheetCommand { get; }
	public IRelayCommand ExportSheetCommand { get; }

	private bool HasSheetText => !string.IsNullOrWhiteSpace(SheetText);

	private async Task ImportSheetAsync()
	{
		(string? content, string? fileName) = await _fileService
			.ImportSheetAsync()
			.ConfigureAwait(true);

		if (content is null)
		{
			return;
		}

		SheetText = content;
		FileName = fileName ?? "Untitled";
		ParseSheet();
	}

	private async Task ExportSheetAsync() =>
		await _fileService.ExportSheetAsync(SheetText).ConfigureAwait(true);

	partial void OnSheetTextChanged(string value)
	{
		ParseSheet();
		_statusBarViewModel.InfoText = $"UTF-8 · Plain Text · {LineCount} lines {value}";
	}

	private void ParseSheet()
	{
		ErrorMessage = null;
		ParsedSheet = null;

		if (string.IsNullOrWhiteSpace(SheetText))
		{
			ValidationStatus = ValidationStatus.Empty;
			_playbackViewModel.ClearSheet();
			return;
		}

		Lexer lexer = new(SheetText);
		ParseResult<IReadOnlyList<TokenBase>> lexResult = lexer.Tokenize();
		ParseResult<Sheet> parseResult = Parser.Parse(lexResult, _playbackViewModel.CurrentConfig);

		if (!parseResult.IsSuccess)
		{
			ErrorMessage = string.Join(Environment.NewLine, parseResult.Errors);
			ValidationStatus = ValidationStatus.Invalid;
			_playbackViewModel.ClearSheet();
			return;
		}

		ParsedSheet = parseResult.Value;
		ValidationStatus = ValidationStatus.Valid;
		_playbackViewModel.UpdateSheet(ParsedSheet!);
		_playbackViewModel.PlayCommand.NotifyCanExecuteChanged();
	}
}
