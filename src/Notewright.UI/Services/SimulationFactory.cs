using System.Runtime.InteropServices;

namespace Notewright.UI.Services;

using Input;
using Input.Linux;
using Input.MacOS;
using Input.Windows;

internal static class SimulatorFactory
{
	internal static IKeyboardSimulator CreateKeyboardSimulator() =>
		RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? new WindowsKeyboardSimulator()
		: RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? new LinuxKeyboardSimulator()
		: RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? new MacKeyboardSimulator()
		: throw new PlatformNotSupportedException(
			"Notewright does not support this operating system."
		);

	internal static IGlobalHotkeyService CreateHotkeyService() =>
		RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? new WindowsHotkeyService()
		: RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? new LinuxHotkeyService()
		: RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? new MacHotkeyService()
		: throw new PlatformNotSupportedException(
			"Notewright does not support this operating system."
		);
}
