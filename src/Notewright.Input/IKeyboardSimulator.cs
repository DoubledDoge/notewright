namespace Notewright.Input;

using Core.Models.Tokens;

public interface IKeyboardSimulator : IDisposable
{
	void KeyPress(NoteToken note);
}
