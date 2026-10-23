using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Notewright.Input.Windows;

using Core.Models.Tokens;

[SupportedOSPlatform("windows")]
public sealed class WindowsKeyboardSimulator : IKeyboardSimulator
{
	private const uint InputKeyboard = 1;
	private static readonly int InputStructSize = Environment.Is64BitProcess ? 40 : 28;

	public void KeyPress(NoteToken note)
	{
		ArgumentNullException.ThrowIfNull(note);

		ushort vkKeyScan = Win32.VkKeyScan(note.Key);
		ushort virtualKey = (ushort)(vkKeyScan & 0xFF);

		Win32.Input[] inputs = note.IsShifted
			? BuildShiftedInputs(virtualKey)
			: BuildInputs(virtualKey);

		foreach (Win32.Input input in inputs)
		{
			System.Diagnostics.Debug.WriteLine(
				$"Input: Type={input.Type} Wvk=0x{input.Data.Ki.Wvk:X4} "
					+ $"WScan=0x{input.Data.Ki.WScan:X4} DwFlags=0x{input.Data.Ki.DwFlags:X8} "
					+ $"InputSize={Win32.InputSize}"
			);
		}

		uint result = Win32.SendInput((uint)inputs.Length, inputs, Win32.InputSize);

		System.Diagnostics.Debug.WriteLine(
			$"SendInput result={result} lastError={Marshal.GetLastWin32Error()}"
		);

		if (result == 0)
		{
			throw new Win32Exception(Marshal.GetLastWin32Error());
		}
	}

	private static Win32.Input[] BuildInputs(ushort virtualKey) =>
		[CreateKeyInput(virtualKey, 0), CreateKeyInput(virtualKey, Win32.KeyeventfKeyup)];

	private static Win32.Input[] BuildShiftedInputs(ushort virtualKey) =>
		[
			CreateKeyInput((ushort)VirtualKey.Shift, 0),
			CreateKeyInput(virtualKey, 0),
			CreateKeyInput(virtualKey, Win32.KeyeventfKeyup),
			CreateKeyInput((ushort)VirtualKey.Shift, Win32.KeyeventfKeyup),
		];

	private static Win32.Input CreateKeyInput(ushort virtualKey, uint flags) =>
		new()
		{
			Type = InputKeyboard,
			Data = new InputUnion
			{
				Ki = new Win32.KeybdInput
				{
					Wvk = virtualKey,
					WScan = 0,
					DwFlags = flags,
					Time = 0,
					DwExtraInfo = nint.Zero,
				},
			},
		};

	private enum VirtualKey : ushort
	{
		Shift = 0x10,
	}

	[SuppressMessage(
		"Interoperability",
		"CA1421:This method uses runtime marshalling even when the \'DisableRuntimeMarshallingAttribute\' is applied"
	)]
	static WindowsKeyboardSimulator() =>
		System.Diagnostics.Debug.Assert(
			Marshal.SizeOf<Win32.Input>() == InputStructSize,
			$"Win32.Input size mismatch: expected {InputStructSize}, got {Marshal.SizeOf<Win32.Input>()}"
		);

	public void Dispose() { }
}
