# KeyShow

A floating on-screen keypress visualizer for Windows. Displays every keystroke in real time — perfect for streaming, screen recording, presentations, or just showing off your keyboard skills.

基于 .NET 9 / WPF 的 Windows 屏幕浮层按键可视化工具，录制、直播、演示时实时展示键盘输入。

## Features · 功能

- **Global key capture** · 全局按键捕获 — Captures all keystrokes system-wide via `WH_KEYBOARD_LL` hook, no window focus required
- **Floating overlay** · 透明浮层 — Semi-transparent, always-on-top window with click-through, never gets in your way
- **Combo detection** · 连击合并 — Rapid repeated presses of the same key auto-merge into `Key × N` display
- **Key statistics** · 热键统计 — Tracks press frequency over the last 2 hours with a ranked stats panel
- **Glow effects** · 流光特效 — High-frequency keys trigger animated gold/blue border glow that sweeps around the card
- **Extensive customization** · 丰富自定义 — Card colors, font family/size/weight, card dimensions, screen position, language
- **i18n** · 多语言 — Built-in Chinese / English support

## Screenshot · 截图

Keys appear as floating cards at the bottom-right corner of the screen:

```
┌─────────────────────────────────────────────┐
│  [Ctrl] [C] [Enter] [A × 12] [H] [E] [L]  │
└─────────────────────────────────────────────┘
```

Hot keys are highlighted with animated gold or blue glow effects.

## Download · 下载

| Version | Size | Notes |
|---------|------|-------|
| `KeyShow-self-contained.exe` | ~71 MB | Standalone — no .NET install needed |
| `framework-dependent/KeyShow.exe` | ~290 KB | Requires [.NET 9.0 Runtime](https://dotnet.microsoft.com/download/dotnet/9.0) |

Download from [Releases](https://github.com/KIKIdoUlovemeRUriding/keyshow/releases).

## Build · 构建

```bash
git clone git@github.com:KIKIdoUlovemeRUriding/keyshow.git
cd keyshow
dotnet run --project KeyShow.csproj

# Publish single-file
dotnet publish KeyShow.csproj -c Release -r win-x64 \
  -p:PublishSingleFile=true --self-contained true -o publish
```

## Tech Stack · 技术栈

- .NET 9.0 / WPF + WinForms
- `SetWindowsHookEx(WH_KEYBOARD_LL)` for global keyboard hooking
- `StrokeDashOffset` animation for border glow effects
- HSV color picker with gradient canvas
- JSON-based settings persistence (`%LocalAppData%/KeyShow/`)

## License · 许可

MIT
