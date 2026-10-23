namespace Notewright.Input;

public interface IGlobalHotkeyService : IDisposable
{
	void Register(char key, Action onPressed);
	void Unregister(char key);
}
