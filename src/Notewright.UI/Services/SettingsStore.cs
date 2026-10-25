using System.Diagnostics;
using System.Text.Json;
using Avalonia.Threading;

namespace Notewright.UI.Services;

internal sealed class SettingsStore
{
	private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

	private readonly string _path = Path.Combine(
		Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
		"Notewright",
		"settings.json"
	);

	private readonly DispatcherTimer _debounce = new()
	{
		Interval = TimeSpan.FromMilliseconds(500),
	};
	private Func<AppSettings>? _snapshot;

	public SettingsStore() => _debounce.Tick += (_, _) => Flush();

	public AppSettings Load()
	{
		try
		{
			if (!File.Exists(_path))
			{
				return new AppSettings();
			}

			using FileStream stream = File.OpenRead(_path);
			return JsonSerializer.Deserialize<AppSettings>(stream, Options) ?? new AppSettings();
		}
		catch (Exception ex)
			when (ex is IOException or UnauthorizedAccessException or JsonException)
		{
			Debug.WriteLine($"Settings load failed: {ex.Message}");
			return new AppSettings(); // unreadable or corrupt: start from defaults
		}
	}

	public void Attach(Func<AppSettings> snapshot) => _snapshot = snapshot;

	public void RequestSave()
	{
		_debounce.Stop();
		_debounce.Start();
	}

	public void Flush()
	{
		_debounce.Stop();

		if (_snapshot is null)
		{
			return;
		}

		try
		{
			Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
			string temp = _path + ".tmp";
			File.WriteAllText(temp, JsonSerializer.Serialize(_snapshot(), Options));
			File.Move(temp, _path, overwrite: true); // a crash shouldn't leave a half-written settings file
		}
		catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
		{
			Debug.WriteLine($"Settings save failed: {ex.Message}");
		}
	}
}
