using System.Runtime.Versioning;

namespace Notewright.Input.MacOS;

using Core;
using Core.Models.Tokens;

[SupportedOSPlatform("macos")]
public sealed class MacKeyboardSimulator : IKeyboardSimulator
{
	private const ulong KcgEventFlagMaskShift = 0x20000;
	private readonly nint _eventSource;
	private bool _disposed;

	public MacKeyboardSimulator()
	{
		_eventSource = CoreGraphics.CGEventSourceCreate(0);

		if (_eventSource == nint.Zero)
		{
			throw new InvalidOperationException(
				"Failed to create CoreGraphics event source. Ensure that the application has the necessary accessibility permissions."
			);
		}
	}

	public void KeyPress(NoteToken note)
	{
		ObjectDisposedException.ThrowIf(_disposed, this);
		ArgumentNullException.ThrowIfNull(note);

		if (!VirtualPianoKeyMap.Keys.TryGetValue(note.Key, out char mappedKey))
		{
			throw new InvalidOperationException($"Key '{note.Key}' is not a valid key.");
		}

		if (!MacKeyCodeMap.KeyCodes.TryGetValue(mappedKey, out ushort keyCode))
		{
			throw new InvalidOperationException($"No macOS key code found for key '{mappedKey}'.");
		}

		nint keyDown = CoreGraphics.CgEventCreateKeyboardEvent(_eventSource, keyCode, true);
		nint keyUp = CoreGraphics.CgEventCreateKeyboardEvent(_eventSource, keyCode, false);

		try
		{
			if (note.IsShifted)
			{
				CoreGraphics.CGEventSetFlags(keyDown, KcgEventFlagMaskShift);
				CoreGraphics.CGEventSetFlags(keyUp, KcgEventFlagMaskShift);
			}

			CoreGraphics.CGEventPost(CoreGraphics.KcghidEventTap, keyDown);
			CoreGraphics.CGEventPost(CoreGraphics.KcghidEventTap, keyUp);
		}
		finally
		{
			if (keyDown != nint.Zero)
			{
				CoreGraphics.CFRelease(keyDown);
			}

			if (keyUp != nint.Zero)
			{
				CoreGraphics.CFRelease(keyUp);
			}
		}
	}

	~MacKeyboardSimulator() => Dispose();

	public void Dispose()
	{
		if (_disposed)
		{
			return;
		}

		if (_eventSource != nint.Zero)
		{
			CoreGraphics.CFRelease(_eventSource);
		}

		_disposed = true;

		GC.SuppressFinalize(this);
	}
}
