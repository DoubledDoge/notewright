using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace Notewright.UI.Services;

[SuppressMessage("Reliability", "CA2007:Consider calling ConfigureAwait on the awaited task")]
internal sealed class FileService(Window window)
{
	private static readonly FilePickerFileType SheetFileType = new("Sheet Files")
	{
		Patterns = ["*.txt"],
		MimeTypes = ["text/plain"],
	};

	internal async Task<(string? Content, string? FileName)> ImportSheetAsync()
	{
		IReadOnlyList<IStorageFile> files = await window
			.StorageProvider.OpenFilePickerAsync(
				new FilePickerOpenOptions
				{
					Title = "Import Sheet",
					AllowMultiple = false,
					FileTypeFilter = [SheetFileType],
				}
			)
			.ConfigureAwait(true);

		if (files.Count == 0)
		{
			return (null, null);
		}

		await using Stream stream = await files[0].OpenReadAsync();
		using StreamReader reader = new(stream);
		string content = await reader.ReadToEndAsync().ConfigureAwait(true);
		string fileName = files[0].Name;

		return (content, fileName);
	}

	internal async Task ExportSheetAsync(string content)
	{
		ArgumentNullException.ThrowIfNull(content);

		IStorageFile? file = await window
			.StorageProvider.SaveFilePickerAsync(
				new FilePickerSaveOptions
				{
					Title = "Export Sheet",
					DefaultExtension = "txt",
					FileTypeChoices = [SheetFileType],
				}
			)
			.ConfigureAwait(true);

		if (file is null)
		{
			return;
		}

		await using Stream stream = await file.OpenWriteAsync();
		await using StreamWriter writer = new(stream);
		await writer.WriteAsync(content).ConfigureAwait(true);
	}
}
