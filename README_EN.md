# KeyShow

A floating on-screen keypress visualizer for Windows — see every keystroke in real time.

[中文](./README.md)

## Features

- **Global key capture** — Captures all keystrokes system-wide via `WH_KEYBOARD_LL` hook, no window focus required
- **Floating overlay** — Semi-transparent, always-on-top window with click-through
- **Combo detection** — Rapid repeated presses auto-merge into `Key × N` display
- **Key statistics** — Tracks press frequency over the last 2 hours with a ranked stats panel
- **Glow effects** — High-frequency keys trigger animated gold/blue border glow
- **Extensive customization** — Card colors, font family/size/weight, card dimensions, position, language
- **i18n** — Built-in Chinese / English support

## Screenshot

![demo](./Images/demo.gif)

## Download

| Version | Size | Notes |
|---------|------|-------|
| `KeyShow-self-contained.exe` | ~71 MB | Standalone — no .NET install needed |
| `framework-dependent/KeyShow.exe` | ~290 KB | Requires [.NET 9.0 Runtime](https://dotnet.microsoft.com/download/dotnet/9.0) |

Download from [Releases](https://github.com/KIKIdoUlovemeRUriding/keyshow/releases).

## Build

```bash
git clone git@github.com:KIKIdoUlovemeRUriding/keyshow.git
cd keyshow
dotnet run --project KeyShow.csproj

# Publish single-file
dotnet publish KeyShow.csproj -c Release -r win-x64 \
  -p:PublishSingleFile=true --self-contained true -o publish
```

## Tech Stack

- .NET 9.0 / WPF + WinForms
- `SetWindowsHookEx(WH_KEYBOARD_LL)` for global keyboard hooking
- `StrokeDashOffset` animation for border glow effects
- HSV color picker with gradient canvas
- JSON-based settings persistence (`%LocalAppData%/KeyShow/`)

## License

MIT
