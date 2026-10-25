using System.ComponentModel;
using System.Diagnostics;
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

	[SuppressMessage(
		"Interoperability",
		"CA1421:This method uses runtime marshalling even when the \'DisableRuntimeMarshallingAttribute\' is applied"
	)]
	static WindowsKeyboardSimulator() =>
		Debug.Assert(
			Marshal.SizeOf<Win32.Input>() == InputStructSize,
			$"Win32.Input size mismatch: expected {InputStructSize}, got {Marshal.SizeOf<Win32.Input>()}"
		);

	public void KeyPress(NoteToken note)
	{
		ArgumentNullException.ThrowIfNull(note);

		ushort vkKeyScan = Win32.VkKeyScan(note.Key);
		ushort virtualKey = (ushort)(vkKeyScan & 0xFF);

		Win32.Input[] inputs = note.IsShifted
			? BuildShiftedInputs(virtualKey)
			: BuildInputs(virtualKey);

		uint result = Win32.SendInput((uint)inputs.Length, inputs, Win32.InputSize);

		if (result == 0)
		{
			throw new Win32Exception(Marshal.GetLastWin32Error());
		}
	}

	public void Dispose() { }

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
}
