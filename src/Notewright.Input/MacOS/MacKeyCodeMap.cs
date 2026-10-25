namespace Notewright.Input.MacOS;

using Core;

internal static class MacKeyCodeMap
{
	private static readonly Dictionary<char, ushort> RawKeyCodes = new()
	{
		// Numbers
		['1'] = 18,
		['2'] = 19,
		['3'] = 20,
		['4'] = 21,
		['5'] = 23,
		['6'] = 22,
		['7'] = 26,
		['8'] = 28,
		['9'] = 25,
		['0'] = 29,
		// Letters
		['a'] = 0,
		['b'] = 11,
		['c'] = 8,
		['d'] = 2,
		['e'] = 14,
		['f'] = 3,
		['g'] = 5,
		['h'] = 4,
		['i'] = 34,
		['j'] = 38,
		['k'] = 40,
		['l'] = 37,
		['m'] = 46,
		['n'] = 45,
		['o'] = 31,
		['p'] = 35,
		['q'] = 12,
		['r'] = 15,
		['s'] = 1,
		['t'] = 17,
		['u'] = 32,
		['v'] = 9,
		['w'] = 13,
		['x'] = 7,
		['y'] = 16,
		['z'] = 6,
		// Special symbols
		['!'] = 18,
		['@'] = 19,
		['$'] = 21,
		['%'] = 23,
		['^'] = 22,
		['*'] = 28,
		['('] = 25,
		['='] = 24,
		['-'] = 27,
		[';'] = 41,
		['\''] = 39,
		['`'] = 50,
		['\\'] = 42,
		['/'] = 44,
	};

	private static readonly Dictionary<ushort, char> UnshiftedByCode = RawKeyCodes
		.Where(kvp => !IsSpecialSymbol(kvp.Key))
		.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);

	private static readonly Dictionary<ushort, char> ShiftedByCode = RawKeyCodes
		.Where(kvp => IsSpecialSymbol(kvp.Key))
		.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);

	internal static readonly IReadOnlyDictionary<char, ushort> KeyCodes = VirtualPianoKeyMap
		.Keys.Keys.Where(RawKeyCodes.ContainsKey)
		.ToDictionary(k => k, k => RawKeyCodes[k]);

	private static bool IsSpecialSymbol(char c) =>
		c is '!' or '@' or '$' or '%' or '^' or '*' or '(';

	internal static char ToChar(ushort keyCode, bool isShifted) =>
		isShifted
			? ShiftedByCode.GetValueOrDefault(keyCode, '\0')
			: UnshiftedByCode.GetValueOrDefault(keyCode, '\0');
}
