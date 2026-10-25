using Notewright.Core;

namespace Notewright.Input.Linux;

internal static class LinuxKeyCodeMap
{
	private const uint KeyLeftShift = 42;
	private const uint KeyRightShift = 54;

	private static readonly Dictionary<uint, char> Unshifted = new()
	{
		// Numbers
		[2] = '1',
		[3] = '2',
		[4] = '3',
		[5] = '4',
		[6] = '5',
		[7] = '6',
		[8] = '7',
		[9] = '8',
		[10] = '9',
		[11] = '0',
		// Letters
		[16] = 'q',
		[17] = 'w',
		[18] = 'e',
		[19] = 'r',
		[20] = 't',
		[21] = 'y',
		[22] = 'u',
		[23] = 'i',
		[24] = 'o',
		[25] = 'p',
		[30] = 'a',
		[31] = 's',
		[32] = 'd',
		[33] = 'f',
		[34] = 'g',
		[35] = 'h',
		[36] = 'j',
		[37] = 'k',
		[38] = 'l',
		[44] = 'z',
		[45] = 'x',
		[46] = 'c',
		[47] = 'v',
		[48] = 'b',
		[49] = 'n',
		[50] = 'm',
	};

	private static readonly Dictionary<uint, char> Shifted = new()
	{
		[2] = '!',
		[3] = '@',
		[5] = '$',
		[6] = '%',
		[7] = '^',
		[9] = '*',
		[10] = '(',
	};

	private static readonly Dictionary<uint, char> HotkeyOnly = new()
	{
		[12] = '-',
		[13] = '=',
		[39] = ';',
		[40] = '\'',
		[41] = '`',
		[43] = '\\',
		[53] = '/',
	};

	private static readonly IReadOnlyDictionary<uint, char> UnshiftedKeyMap = VirtualPianoKeyMap
		.Keys.Values.ToHashSet()
		.Select(c => Unshifted.FirstOrDefault(kvp => kvp.Value == c))
		.Where(kvp => kvp.Key != 0)
		.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

	private static readonly IReadOnlyDictionary<uint, char> ShiftedKeyMap = VirtualPianoKeyMap
		.Keys.Values.ToHashSet()
		.Select(c => Shifted.FirstOrDefault(kvp => kvp.Value == c))
		.Where(kvp => kvp.Key != 0)
		.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

	internal static bool IsShiftKey(uint keyCode) => keyCode is KeyLeftShift or KeyRightShift;

	internal static char ToChar(uint keyCode, bool isShifted) =>
		isShifted
			? ShiftedKeyMap.GetValueOrDefault(keyCode, '\0')
			: UnshiftedKeyMap.GetValueOrDefault(
				keyCode,
				HotkeyOnly.GetValueOrDefault(keyCode, '\0')
			);
}
