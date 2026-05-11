# KeyShow

屏幕浮层按键可视化工具 — 录制、直播、演示时实时展示键盘输入。

[English](./README_EN.md)

## 功能

- **全局按键捕获** — 监听所有按键输入，无需窗口聚焦
- **透明浮层显示** — 半透明置顶窗口，鼠标穿透不干扰操作
- **连击合并** — 长按/快速连击自动合并为 `Key × N` 显示
- **热键统计** — 统计最近 2 小时按键频次，热度排名
- **流光特效** — 高频按键触发金色/蓝色环绕流光边框
- **丰富自定义** — 卡片颜色、字号、字体、大小、位置、语言均可配置
- **多语言** — 内置中文 / English

## 截图

![demo](./Images/demo.gif)

## 下载

| 版本 | 大小 | 说明 |
|------|------|------|
| `KeyShow-self-contained.exe` | ~71 MB | 免安装 .NET，即开即用 |
| `framework-dependent/KeyShow.exe` | ~290 KB | 需 [.NET 9.0 Runtime](https://dotnet.microsoft.com/download/dotnet/9.0) |

从 [Releases](https://github.com/KIKIdoUlovemeRUriding/keyshow/releases) 下载。

## 构建

```bash
git clone git@github.com:KIKIdoUlovemeRUriding/keyshow.git
cd keyshow
dotnet run --project KeyShow.csproj

# 发布单文件
dotnet publish KeyShow.csproj -c Release -r win-x64 \
  -p:PublishSingleFile=true --self-contained true -o publish
```

## 技术栈

- .NET 9.0 / WPF + WinForms
- `SetWindowsHookEx(WH_KEYBOARD_LL)` 全局键盘钩子
- `StrokeDashOffset` 动画实现边框流光
- HSV 色盘取色器
- JSON 配置持久化 (`%LocalAppData%/KeyShow/`)

## 许可

MIT
