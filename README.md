# Notewright

[![Build](https://github.com/DoubledDoge/notewright/actions/workflows/ci-cd.yml/badge.svg)](https://github.com/DoubledDoge/notewright/actions/workflows/ci-cd.yml)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![.NET 10](https://img.shields.io/badge/.NET-10-512bd4.svg)](https://dotnet.microsoft.com/)

## Features

- **Automated playback** with play, pause/resume and stop. Tempo runs from 20 to 300 BPM and changes apply immediately, even in the middle of a song.
- **Manual mode.** Each press of a hotkey plays the next note, chord or roll, so you control the timing. Rests and pauses are skipped.
- **Live validation.** Invalid notation is underlined in red in the editor, with a list underneath giving the line, column and reason for each problem. Click an entry to jump to it.
- **Predicted length** that updates as you change the tempo.
- **Global hotkeys** for play, pause, stop and manual advance. Each one is rebindable, and each only acts in the mode it belongs to.
- **Import and export** sheets as plain text files.
- **Persistent settings** that survive restarts.
- **Cross-platform:** Windows, Linux and macOS.

## Quick start

1. Download the build for your platform from the [Releases](https://github.com/DoubledDoge/notewright/releases) page and extract it. Or [build from source](#building-from-source).
2. Complete the [platform setup](#platform-setup) for your operating system (Windows needs nothing extra).
3. Paste a sheet into the editor, or use **Import text file**. Fix anything underlined in red.
4. Set the tempo.
5. Click into the window you want to play into (a game's in-game piano, for example) and press the **Play** hotkey (`-` by default).

The Play button in the app is handy for testing, but keystrokes always go to whichever window has focus. While you are clicking Notewright's own button, that is Notewright, so you will normally start playback with the hotkey.

## Sheet notation

Notewright sends the keys exactly as written. It does not know what your target application maps those keys to, so write sheets for the layout the target expects (the notation is the one used by Virtual Piano).

| Syntax | Meaning | Timing |
|---|---|---|
| `q` | Single note | One beat |
| `Q`, `!` | Shifted note (Shift is held). Uppercase letters, plus `!` `@` `$` `%` `^` `*` `(` | One beat |
| `[qet]` | Chord: all keys pressed together | One beat |
| `{qet}` | Roll: keys pressed in quick succession (about 1 ms apart) | One beat after the last key |
| `q-w-e` | Legato run: notes a quarter beat apart, with no spaces around `-` | A quarter beat per note, no trailing beat |
| `q -` | Short pause: a space before `-` | Half a beat instead of a full beat |
| `q,` | Medium pause | Three-quarters of a beat |
| `\|` or `.` | Rest | One silent beat each (`\|\|` is two) |
| space, newline | Separator | None |

Allowed note characters are `a`–`z`, `A`–`Z`, `0`–`9` and `! @ $ % ^ * (`.

Everything is measured in beats, so at 120 BPM one beat is 500 ms. A note, chord or roll takes one beat unless it carries a pause marker. Spaces and newlines only separate things and add no time.

### Examples

Twinkle, Twinkle, Little Star (Virtual Piano layout):

```text
t t o o p p o |
i i u u y y t |
```

Chords, a roll, a legato run, a short pause and a shifted note:

```text
[tuo] {tuo} t-y-u | t - Y
```

## Playback modes

### Automated mode

Press **Play** (button or hotkey) to start, **Pause** to hold at the current beat, and **Play** again to resume from where you paused. **Stop** returns to the beginning. Changing the tempo mid-song takes effect from the next note.

### Manual mode

Use **Switch to manual** (next to the mode badge). Each press of the **Advance** hotkey plays the next note, chord, roll or legato run. Rests and pauses add no delay, so the beat is yours. When the sheet ends, Notewright returns to automated mode. **Stop** restarts the sheet from the first beat.

Switching modes while a run is in progress ends the run and resets progress.

## Hotkeys

| Action | Default | Automated mode | Manual mode |
|---|---|---|---|
| Play / resume | `-` | yes | no |
| Pause | `\` | yes | no |
| Stop | `/` | yes | yes (restarts at beat 1) |
| Advance (play next) | `=` | no | yes |

- Hotkeys are **only live while Notewright is not the focused window**. That keeps every key free for typing in the editor, and it matches how you will use them: focus the target window, then press a key.
- A hotkey is only registered in the modes shown above.
- To rebind, open **Settings → Keyboard & hotkeys**, choose **Change hotkey**, type the new key, and press **Save changes**. **Reset** restores the default. Keys already used by another action are rejected.
- Hotkeys must be one of: `-` `=` `\` `/` `;` `'` `` ` ``.

## Settings

Your hotkeys, tempo and transposition settings are saved to `settings.json` shortly after each change and again on exit:

| OS | Location |
|---|---|
| Windows | `%APPDATA%\Notewright\settings.json` |
| Linux | `~/.config/Notewright/settings.json` (or `$XDG_CONFIG_HOME/Notewright/settings.json`) |
| macOS | `~/.config/Notewright/settings.json` |

```json
{
  "Version": 1,
  "Hotkeys": {
    "Advance": "=",
    "Play": "-",
    "Pause": "\\",
    "Stop": "/"
  },
  "Bpm": 120,
  "Transposition": 0
}
```

A missing or unreadable file falls back to defaults. Hotkeys that are unsupported or duplicated are intended to be ignored, and tempo is clamped to its valid range.

## Platform setup

### Windows

Windows 10 or later. No extra setup.

If the target application runs **as administrator**, Windows will block input from a non-elevated Notewright, so run Notewright as administrator too.

### Linux

Notewright types keys through [dotool](https://git.sr.ht/~geb/dotool) and listens for hotkeys through libinput.

- Install **dotool** (see its documentation for your distribution) so that `dotool` is on your `PATH`.
- Make sure **libinput** and **libudev** are installed.
- Your user needs access to input devices and to `/dev/uinput`. The usual setup is:

```bash
sudo usermod -aG input "$USER"
echo 'KERNEL=="uinput", GROUP="input", MODE="0660", OPTIONS+="static_node=uinput"' | sudo tee /etc/udev/rules.d/80-uinput.rules
sudo udevadm control --reload && sudo udevadm trigger
```

Then log out and back in. dotool's own documentation describes this in more detail.

### macOS

Notewright needs the **Accessibility** permission to send keys and listen for hotkeys. Grant it under **System Settings → Privacy & Security → Accessibility**, then restart Notewright. If you run from source, grant the permission to the terminal you launch it from.

## Building from source

Requires the [.NET 10 SDK](https://dotnet.microsoft.com/download) (see `global.json`).

```bash
git clone https://github.com/DoubledDoge/notewright.git
cd notewright
dotnet build
dotnet run --project src/Notewright.UI
```

## Known limitations

- **Transposition is not applied yet.** The Transpose control is saved but does not change what is played.
- **Windows consumes hotkeys.** While a hotkey is registered, Windows delivers that key only to Notewright, so it cannot be typed in the target application. If you need `/` or `\` in a game's chat, rebind the hotkey.
- **Linux observes hotkeys.** Notewright listens for them without intercepting, so the key also reaches the focused application.
- **US keyboard layout assumed** for the Linux and macOS key maps.
- Only single-character hotkeys from the supported set are available.

## Troubleshooting

- **Hotkeys do nothing.** Notewright must *not* be the focused window, and the hotkey must belong to the current mode (see the table above).
- **Nothing is typed in the game.** Make sure the intended window has focus when playback starts. On Windows, check whether the application runs elevated.
- **Linux: "dotool was not found".** Install dotool and make sure it is on your `PATH`, then check your `uinput` permissions.
- **Linux: "Failed to create libinput context".** Your user needs to be in the `input` group (log out and back in after adding yourself).
- **macOS: "Failed to create CGEventTap".** Grant the Accessibility permission and restart Notewright.

## Responsible use

Notewright is meant to send simulated keystrokes. Automated input may be against the rules of some online games and services, so check the terms of whatever you are playing on. Please use it at your own risk :|

## Contributing

Issues and pull requests are welcome. Please use the bug report template for problems.

The build treats warnings as errors and runs the .NET and Sonar analyzers, so a clean `dotnet build` is required. Formatting is enforced with [CSharpier](https://csharpier.com/):

```bash
dotnet tool restore
dotnet csharpier format .
```

## License

Released under the [MIT License](LICENSE).

## Acknowledgments

Special thanks for the following websites for guiding me around the keyboard piano syntax:

- [Virtual Piano](https://virtualpiano.net/) - First and closest source of general piano playing. My most definitive pick.
- [PlayPianoSheets](https://playpianosheets.com/) - Meant for Roblox (funnily enough) and has minor syntax differences but is an excellent source for songs.

