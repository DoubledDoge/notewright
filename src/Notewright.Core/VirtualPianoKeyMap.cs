namespace Notewright.Core;

public static class VirtualPianoKeyMap
{
	public static readonly IReadOnlyDictionary<char, char> Keys = new Dictionary<char, char>
	{
		// Numbers
		['1'] = '1',
		['2'] = '2',
		['3'] = '3',
		['4'] = '4',
		['5'] = '5',
		['6'] = '6',
		['7'] = '7',
		['8'] = '8',
		['9'] = '9',
		['0'] = '0',
		// Letters
		['a'] = 'a',
		['b'] = 'b',
		['c'] = 'c',
		['d'] = 'd',
		['e'] = 'e',
		['f'] = 'f',
		['g'] = 'g',
		['h'] = 'h',
		['i'] = 'i',
		['j'] = 'j',
		['k'] = 'k',
		['l'] = 'l',
		['m'] = 'm',
		['n'] = 'n',
		['o'] = 'o',
		['p'] = 'p',
		['q'] = 'q',
		['r'] = 'r',
		['s'] = 's',
		['t'] = 't',
		['u'] = 'u',
		['v'] = 'v',
		['w'] = 'w',
		['x'] = 'x',
		['y'] = 'y',
		['z'] = 'z',
		// Special Symbols
		['!'] = '!',
		['@'] = '@',
		['$'] = '$',
		['%'] = '%',
		['^'] = '^',
		['*'] = '*',
		['('] = '(',
	};

	public static readonly IReadOnlyDictionary<char, char> ShiftedSymbols = new Dictionary<
		char,
		char
	>
	{
		['!'] = '1',
		['@'] = '2',
		['$'] = '4',
		['%'] = '5',
		['^'] = '6',
		['*'] = '8',
		['('] = '9',
	};
}
