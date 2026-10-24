using System.Globalization;
using Notewright.Core.Models;
using Notewright.Core.Models.Tokens;

namespace Notewright.Core.Parsing;

public sealed class Lexer(string input)
{
	private readonly List<Diagnostic> _errors = [];
	private int _position;

	private char Current => _position < input.Length ? input[_position] : default;

	private bool IsAtEnd => _position >= input.Length;

	private char Peek(int offset = 1) =>
		_position + offset < input.Length ? input[_position + offset] : default;

	private void Report(int start, int length, string message) =>
		_errors.Add(new Diagnostic(start, length, message));

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
		string message = Current switch
		{
			']' => "']' has no matching '['.",
			'}' => "'}' has no matching '{'.",
			'-' => "'-' must sit between two notes (q-w) or follow a note after a space (q -).",
			',' => "',' must follow a note.",
			_ => $"'{Current}' isn't a valid note, rest or symbol.",
		};
		Report(_position, 1, message);
		_position++;
		return null;
	}

	private static bool IsNoteChar(char c) => char.IsAsciiLetterOrDigit(c);

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
		(string raw, List<NoteToken> notes) = ReadGroup(']', "chord");

		return new ChordToken(raw, notes.AsReadOnly());
	}

	private RollToken ReadRoll()
	{
		(string raw, List<NoteToken> notes) = ReadGroup('}', "roll");
		return new RollToken(raw, notes.AsReadOnly());
	}

	private (string Raw, List<NoteToken> Notes) ReadGroup(char close, string name)
	{
		int start = _position;
		_position++; // opening bracket
		List<NoteToken> notes = [];

		// One typo shouldn't swallow the rest of the sheet
		while (!IsAtEnd && Current != close && Current != '\n')
		{
			if (IsNoteChar(Current))
			{
				notes.Add(ReadSingleNote());
			}
			else if (char.IsWhiteSpace(Current))
			{
				_position++;
			}
			else
			{
				Report(
					_position,
					1,
					$"'{Current}' isn't allowed inside a {name}. Only note keys (a-z, 0-9) can go between the brackets."
				);
				_position++;
			}
		}

		bool closed = Current == close;
		if (closed)
		{
			_position++;
			if (notes.Count == 0)
			{
				Report(start, _position - start, $"Empty {name}. Put at least one note inside it.");
			}
		}
		else
		{
			Report(start, 1, $"This {name} is never closed. Add '{close}' at the end of it.");
		}

		return (input[start.._position], notes);
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
