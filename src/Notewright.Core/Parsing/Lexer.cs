using System.Globalization;

namespace Notewright.Core.Parsing;

using Models;
using Models.Tokens;

public sealed class Lexer(string input)
{
	private int _position;
	private readonly List<string> _errors = [];

	private char Current => _position < input.Length ? input[_position] : default;

	private char Peek(int offset = 1) =>
		_position + offset < input.Length ? input[_position + offset] : default;

	private bool IsAtEnd => _position >= input.Length;

	public ParseResult<IReadOnlyList<TokenBase>> Tokenize()
	{
		List<TokenBase> tokens = [];

		while (!IsAtEnd)
		{
			SkipWhitespace();

			if (IsAtEnd)
			{
				break;
			}

			TokenBase? token = Current switch
			{
				'[' => ReadChord(),
				'{' => ReadRoll(),
				'|' or '.' => ReadRest(),
				_ when IsNoteChar(Current) => ReadNoteOrLegatoRun(),
				_ => SkipUnknown(),
			};

			if (token is not null)
			{
				tokens.Add(token);
			}
		}

		return new ParseResult<IReadOnlyList<TokenBase>>(
			Value: tokens.AsReadOnly(),
			IsSuccess: _errors.Count == 0,
			Errors: _errors.AsReadOnly()
		);
	}

	private void SkipWhitespace()
	{
		while (!IsAtEnd && char.IsWhiteSpace(Current))
		{
			_position++;
		}
	}

	private TokenBase? SkipUnknown()
	{
		_errors.Add($"Unrecognised character '{Current}' at position {_position}.");
		_position++;
		return null;
	}

	private static bool IsNoteChar(char c) => char.IsLetter(c) || char.IsDigit(c);

	private NoteToken ReadSingleNote(DelayAfter delay = DelayAfter.None)
	{
		char key = Current;
		_position++;

		return new NoteToken(
			Raw: key.ToString(),
			Key: char.ToLower(key, CultureInfo.CurrentCulture),
			IsShifted: char.IsUpper(key),
			DelayAfter: delay
		);
	}

	private TokenBase ReadNoteOrLegatoRun()
	{
		int lookahead = 1;
		while (
			_position + lookahead < input.Length
			&& input[_position + lookahead] == '-'
			&& _position + lookahead + 1 < input.Length
			&& IsNoteChar(input[_position + lookahead + 1])
		)
		{
			lookahead += 2;
		}

		if (lookahead > 1)
		{
			return ReadLegatoRun();
		}

		int start = _position;
		NoteToken note = ReadSingleNote();

		int savedPosition = _position;
		SkipWhitespace();

		if (Current == '-' && char.IsWhiteSpace(input[_position - 1]))
		{
			_position++;
			return note with { DelayAfter = DelayAfter.Short, Raw = input[start.._position] };
		}

		if (Current == ',')
		{
			_position++;
			return note with { DelayAfter = DelayAfter.Medium, Raw = input[start.._position] };
		}

		_position = savedPosition;
		return note;
	}

	private LegatoRunToken ReadLegatoRun()
	{
		int start = _position;
		List<NoteToken> notes = [ReadSingleNote()];

		while (!IsAtEnd && Current == '-' && IsNoteChar(Peek()))
		{
			_position++;
			notes.Add(ReadSingleNote());
		}

		return new LegatoRunToken(Raw: input[start.._position], Notes: notes.AsReadOnly());
	}

	private ChordToken ReadChord()
	{
		int start = _position;
		_position++;
		List<NoteToken> notes = [];

		while (!IsAtEnd && Current != ']')
		{
			if (IsNoteChar(Current))
			{
				notes.Add(ReadSingleNote());
			}
			else
			{
				_position++;
			}
		}

		if (Current == ']')
		{
			_position++;
		}
		else
		{
			_errors.Add($"Unclosed chord '[' at position {start}.");
		}

		return new ChordToken(Raw: input[start.._position], Notes: notes.AsReadOnly());
	}

	private RollToken ReadRoll()
	{
		int start = _position;
		_position++;
		List<NoteToken> notes = [];

		while (!IsAtEnd && Current != '}')
		{
			if (IsNoteChar(Current))
			{
				notes.Add(ReadSingleNote());
			}
			else
			{
				_position++;
			}
		}

		if (Current == '}')
		{
			_position++;
		}
		else
		{
			_errors.Add($"Unclosed roll '{{' at position {start}.");
		}

		return new RollToken(Raw: input[start.._position], Notes: notes.AsReadOnly());
	}

	private RestToken ReadRest()
	{
		int start = _position;
		int units = 0;

		while (!IsAtEnd && Current is '|' or '.')
		{
			units++;
			_position++;
		}

		return new RestToken(Raw: input[start.._position], Units: units);
	}
}
