using System.Diagnostics;
using System.Runtime.Versioning;

namespace Notewright.Input.Linux;

using Core.Models.Tokens;

[SupportedOSPlatform("linux")]
public sealed class LinuxKeyboardSimulator : IKeyboardSimulator
{
	private const string DotoolBinary = "dotool";
	private readonly string _dotoolPath =
		ResolveDotoolPath()
		?? throw new InvalidOperationException(
			"dotool was not found. Ensure that dotool is installed and that the user has the appropriate uinput permissions. Refer to the Notewright README for installation instructions."
		);
	private bool _disposed;

	public void KeyPress(NoteToken note)
	{
		ObjectDisposedException.ThrowIf(_disposed, this);
		ArgumentNullException.ThrowIfNull(note);

		string keysequence = note.IsShifted ? $"shift+{note.Key}" : note.Key.ToString();

		using Process process = new();
		process.StartInfo = new ProcessStartInfo
		{
			FileName = _dotoolPath,
			RedirectStandardInput = true,
			RedirectStandardError = true,
			UseShellExecute = false,
			CreateNoWindow = true,
		};

		process.Start();
		process.StandardInput.WriteLine($"key {keysequence}");
		process.StandardInput.Close();
		process.WaitForExit();

		if (process.ExitCode == 0)
		{
			return;
		}

		string error = process.StandardError.ReadToEnd();
		throw new InvalidOperationException(
			$"dotool failed to send key sequence '{keysequence}': {error}"
		);
	}

	public void Dispose()
	{
		if (_disposed)
		{
			return;
		}

		_disposed = true;
	}

	private static string? ResolveDotoolPath()
	{
		using Process process = new();
		process.StartInfo = new ProcessStartInfo
		{
			FileName = "/usr/bin/which",
			Arguments = DotoolBinary,
			RedirectStandardOutput = true,
			UseShellExecute = false,
			CreateNoWindow = true,
		};

		process.Start();
		string output = process.StandardOutput.ReadToEnd().Trim();
		process.WaitForExit();

		return process.ExitCode == 0 ? output : null;
	}
}
