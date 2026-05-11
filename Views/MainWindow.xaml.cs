using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace KeyShow;

public partial class MainWindow : Window
{
    // 窗口扩展样式：不显示在 Alt+Tab
    private const int WS_EX_TOOLWINDOW = 0x00000080;
    private const int GWL_EXSTYLE = -20;

    private readonly GlobalKeyboardHook _hook;
    private readonly ObservableCollection<KeyEntry> _keys = new();
    private readonly DispatcherTimer _cleanupTimer;
    private readonly System.Windows.Forms.NotifyIcon _trayIcon;
    private bool _mouseInArea;

    public MainWindow()
    {
        InitializeComponent();
        Icon = App.GetWindowIcon();

        KeyList.ItemsSource = _keys;

        _hook = new GlobalKeyboardHook();
        _hook.KeyPressed += OnKeyPressed;

        // 每 100ms 检测是否需要开始淡出 / 移除过期条目
        _cleanupTimer = new DispatcherTimer(
            TimeSpan.FromMilliseconds(100),
            DispatcherPriority.Normal,
            OnCleanup,
            Dispatcher);
        _cleanupTimer.Start();

        // 系统托盘图标：右键菜单
        _trayIcon = new System.Windows.Forms.NotifyIcon
        {
            Text = L.TrayText,
            Icon = App.GetTrayIcon(),
            Visible = true,
            ContextMenuStrip = new System.Windows.Forms.ContextMenuStrip()
        };
        _trayIcon.ContextMenuStrip.Items.Add(L.TrayStats, null, (_, _) => OpenStats());
        _trayIcon.ContextMenuStrip.Items.Add(L.TraySettings, null, (_, _) => OpenSettings());
        _trayIcon.ContextMenuStrip.Items.Add("-");
        _trayIcon.ContextMenuStrip.Items.Add(L.TrayExit, null, (_, _) =>
        {
            _trayIcon.Visible = false;
            Close();
        });

        // 双击托盘图标也打开设置
        _trayIcon.DoubleClick += (_, _) => OpenSettings();

        // 窗口初始化后设置样式
        SourceInitialized += OnSourceInitialized;
        Loaded += (_, _) => PositionWindow();
    }

    private void OnSourceInitialized(object? sender, EventArgs e)
    {
        var helper = new System.Windows.Interop.WindowInteropHelper(this);
        int exStyle = GetWindowLong(helper.Handle, GWL_EXSTYLE);
        _ = SetWindowLong(helper.Handle, GWL_EXSTYLE,
            exStyle | WS_EX_TOOLWINDOW);
    }

    private void PositionWindow()
    {
        var (left, top) = AppSettings.Instance.GetWindowPosition(Width, Height);
        Left = left;
        Top = top;
    }

    private void OpenStats()
    {
        var win = new StatsWindow { Owner = this };
        win.ShowDialog();
    }

    private void OpenSettings()
    {
        var win = new SettingsWindow();
        win.Owner = this;
        if (win.ShowDialog() == true)
            ApplySettings();
    }

    private void ApplySettings()
    {
        PositionWindow();
        // 更新已有按键容器的样式
        foreach (var entry in _keys)
        {
            var border = GetItemBorder(entry);
            if (border != null) ApplySettingsToBorder(border);
        }
        RefreshAllGlowEffects();
    }

    /// <summary>
    /// 将当前设置应用到指定的 Border 元素
    /// </summary>
    private static void ApplySettingsToBorder(Border border)
    {
        var s = AppSettings.Instance;
        border.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(s.BackgroundColor));
        border.Padding = new Thickness(12, s.CardPadding, 12, s.CardPadding);
        border.MinWidth = s.CardMinWidth;

        if (border.Child is TextBlock tb)
        {
            tb.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(s.FontColor));
            tb.FontWeight = s.FontBold ? FontWeights.Bold : FontWeights.Normal;
            tb.FontSize = s.FontSize;
            try
            {
                if (!string.IsNullOrEmpty(s.CustomFontPath) && File.Exists(s.CustomFontPath))
                {
                    foreach (var f in Fonts.GetFontFamilies(s.CustomFontPath))
                    {
                        tb.FontFamily = f;
                        return;
                    }
                }
                tb.FontFamily = new System.Windows.Media.FontFamily(s.FontFamilyName);
            }
            catch { }
        }
    }

    // ==================== 拖拽 ====================

    private void KeyList_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        DragMove();
    }

    // ==================== 鼠标移入/移出：冻结/恢复动画 ====================

    private void KeyList_MouseEnter(object sender, MouseEventArgs e)
    {
        _mouseInArea = true;
        foreach (var entry in _keys)
        {
            entry.IsFading = false;
            var border = GetItemBorder(entry);
            if (border == null) continue;

            // 停止所有淡出动画，恢复满值
            StopFadeAnimation(border);
        }
    }

    private void KeyList_MouseLeave(object sender, MouseEventArgs e)
    {
        _mouseInArea = false;
        var now = DateTime.Now;
        foreach (var entry in _keys)
            entry.CreatedAt = now;
    }

    // ==================== 按键事件 ====================

    private void OnKeyPressed(Key key)
    {
        Dispatcher.Invoke(() =>
        {
            var text = KeyToDisplayString(key);
            var now = DateTime.Now;

            // 记录统计
            KeyStatsTracker.Record(text);
            var statCount = KeyStatsTracker.GetCount(text);

            string glow;
            if (statCount >= 10000) glow = "#CCFFD700";
            else if (statCount >= 2000) glow = "#CC4FC3F7";
            else glow = "Transparent";

            // 场景 A：上一个是已合并条目（RepeatCount≥5），且在 800ms 内 → 连击累加
            if (_keys.Count > 0)
            {
                var last = _keys[^1];
                if (last.RepeatCount >= 5 && last.BaseText == text
                    && (now - last.CreatedAt).TotalMilliseconds <= 800)
                {
                    last.BumpRepeat();
                    last.CreatedAt = now;
                    last.GlowColor = glow;
                    return;
                }
            }

            // 场景 B：先创建独立条目
            var entry = new KeyEntry
            {
                BaseText = text,
                Text = text,
                RepeatCount = 1,
                CreatedAt = now,
                Opacity = 1.0,
                Scale = 1.0,
                GlowColor = glow
            };
            _keys.Add(entry);
            ApplyNewEntryStyle(entry);

            // 场景 C：检查末尾连续 5 个同键独立条目是否在 800ms 内 → 合并
            if (_keys.Count >= 5)
            {
                int same = 0;
                for (int i = _keys.Count - 1; i >= 0; i--)
                {
                    if (_keys[i].BaseText == text && _keys[i].RepeatCount == 1)
                        same++;
                    else
                        break;
                }

                if (same >= 5)
                {
                    var fifth = _keys[^5];
                    if ((now - fifth.CreatedAt).TotalMilliseconds <= 800)
                    {
                        // 移除 5 个独立条目
                        for (int i = 0; i < 5; i++)
                            _keys.RemoveAt(_keys.Count - 1);

                        // 替换为 1 个合并条目
                        var merged = new KeyEntry
                        {
                            BaseText = text,
                            Text = $"{text} × 5",
                            RepeatCount = 5,
                            CreatedAt = now,
                            Opacity = 1.0,
                            Scale = 1.0,
                            GlowColor = glow
                        };
                        _keys.Add(merged);
                        ApplyNewEntryStyle(merged);
                    }
                }
            }

            // 限制最大条目数
            var maxKeys = AppSettings.Instance.MaxKeys;
            while (_keys.Count > maxKeys)
                _keys.RemoveAt(0);
        });
    }

    private void ApplyNewEntryStyle(KeyEntry entry)
    {
        var captured = entry;
        Dispatcher.BeginInvoke(new Action(() =>
        {
            var glowRect = GetGlowRect(captured, KeyList);
            if (glowRect != null)
                ApplyGlowEffect(glowRect, captured.GlowColor);

            var border = GetItemBorder(captured);
            if (border != null) ApplySettingsToBorder(border);
        }), DispatcherPriority.Loaded);
    }

    // ==================== 计时清理：触发 WPF 平滑动画 ====================

    private void OnCleanup(object? sender, EventArgs e)
    {
        if (_mouseInArea)
            return;

        var settings = AppSettings.Instance;
        var now = DateTime.Now;
        var totalLife = settings.VisibleSeconds + settings.FadeSeconds;

        for (int i = _keys.Count - 1; i >= 0; i--)
        {
            var entry = _keys[i];
            var elapsed = (now - entry.CreatedAt).TotalSeconds;

            if (elapsed > totalLife)
            {
                _keys.RemoveAt(i);
            }
            else if (elapsed > settings.VisibleSeconds && !entry.IsFading)
            {
                // 启动 WPF 平滑淡出 + 缩小动画
                var border = GetItemBorder(entry);
                if (border != null)
                {
                    StartFadeAnimation(border, settings.FadeSeconds);
                    entry.IsFading = true;
                }
            }
        }
    }

    /// <summary>
    /// 启动 WPF DoubleAnimation：透明度 1→0，缩放 1→0.4
    /// </summary>
    private static void StartFadeAnimation(Border border, double durationSeconds)
    {
        var duration = TimeSpan.FromSeconds(durationSeconds);
        var easing = new CubicEase { EasingMode = EasingMode.EaseIn };

        // 透明度
        border.BeginAnimation(UIElement.OpacityProperty,
            new DoubleAnimation(0.0, duration) { EasingFunction = easing });

        // 缩放
        var scale = border.RenderTransform as ScaleTransform;
        if (scale != null)
        {
            scale.BeginAnimation(ScaleTransform.ScaleXProperty,
                new DoubleAnimation(0.4, duration) { EasingFunction = easing });
            scale.BeginAnimation(ScaleTransform.ScaleYProperty,
                new DoubleAnimation(0.4, duration) { EasingFunction = easing });
        }
    }

    /// <summary>
    /// 停止动画并恢复 Opacity / Scale 到默认值
    /// </summary>
    private static void StopFadeAnimation(Border border)
    {
        border.BeginAnimation(UIElement.OpacityProperty, null);
        border.Opacity = 1.0;

        var scale = border.RenderTransform as ScaleTransform;
        if (scale != null)
        {
            scale.BeginAnimation(ScaleTransform.ScaleXProperty, null);
            scale.BeginAnimation(ScaleTransform.ScaleYProperty, null);
            scale.ScaleX = 1.0;
            scale.ScaleY = 1.0;
        }
    }

    /// <summary>
    /// 从 ItemsControl 容器中找到指定 KeyEntry 对应的内层按键 Border
    /// </summary>
    private Border? GetItemBorder(KeyEntry entry)
    {
        var container = KeyList.ItemContainerGenerator.ContainerFromItem(entry) as ContentPresenter;
        if (container == null) return null;
        // 结构: ContentPresenter → Grid → [GlowRect, KeyBorder, ...]
        var grid = VisualTreeHelper.GetChild(container, 0) as Grid;
        if (grid == null || grid.Children.Count < 2) return null;
        return grid.Children[1] as Border;
    }

    /// <summary>
    /// 获取流光 Rectangle（Grid 的第一个子元素）
    /// </summary>
    private static Rectangle? GetGlowRect(KeyEntry entry, ItemsControl list)
    {
        var container = list.ItemContainerGenerator.ContainerFromItem(entry) as ContentPresenter;
        if (container == null) return null;
        var grid = VisualTreeHelper.GetChild(container, 0) as Grid;
        if (grid == null || grid.Children.Count < 1) return null;
        return grid.Children[0] as Rectangle;
    }

    /// <summary>
    /// 为流光边框设置环绕渐变动画
    /// </summary>
    /// <summary>
    /// 设置 Rectangle 的描边流光：StrokeDashOffset 动画 → 光段沿边框一笔画过
    /// </summary>
    private static void ApplyGlowEffect(Rectangle glowRect, string glowColor)
    {
        if (!KeyStatsTracker.GlowEnabled || glowColor == "Transparent")
        {
            glowRect.Stroke = Brushes.Transparent;
            return;
        }

        var c = (Color)ColorConverter.ConvertFromString(glowColor);
        glowRect.Stroke = new SolidColorBrush(c);

        // Dash 动画：光段沿矩形周长行进，周长 ≈ 2*(W+H)，用足够大的 gap
        var anim = new DoubleAnimation(0, -530, TimeSpan.FromSeconds(2.0))
        {
            RepeatBehavior = RepeatBehavior.Forever
        };
        glowRect.BeginAnimation(System.Windows.Shapes.Shape.StrokeDashOffsetProperty, anim);
    }

    /// <summary>
    /// 刷新所有按键的流光特效（开/关切换时调用）
    /// </summary>
    public void RefreshAllGlowEffects()
    {
        foreach (var entry in _keys)
        {
            var glowRect = GetGlowRect(entry, KeyList);
            if (glowRect != null)
                ApplyGlowEffect(glowRect, entry.GlowColor);
        }
    }

    // ==================== 按键名称映射 ====================

    private static string KeyToDisplayString(Key key)
    {
        return key switch
        {
            // 字母
            >= Key.A and <= Key.Z => key.ToString(),

            // 数字
            >= Key.D0 and <= Key.D9 => ((char)('0' + (key - Key.D0))).ToString(),

            // 小键盘数字
            >= Key.NumPad0 and <= Key.NumPad9 => $"Num{(char)('0' + (key - Key.NumPad0))}",

            // 功能键
            >= Key.F1 and <= Key.F24 => $"F{(int)(key - Key.F1 + 1)}",

            // 修饰键
            Key.LeftShift or Key.RightShift => "Shift",
            Key.LeftCtrl or Key.RightCtrl => "Ctrl",
            Key.LeftAlt or Key.RightAlt => "Alt",
            Key.LWin or Key.RWin => "Win",

            // 特殊键
            Key.Space => "Space",
            Key.Enter or Key.Return => "Enter",
            Key.Back => "Back",
            Key.Tab => "Tab",
            Key.Escape => "Esc",
            Key.CapsLock => "Caps",
            Key.Delete => "Del",
            Key.Insert => "Ins",
            Key.Home => "Home",
            Key.End => "End",
            Key.PageUp => "PgUp",
            Key.PageDown => "PgDn",
            Key.PrintScreen => "PrtSc",
            Key.Scroll => "ScrlLk",
            Key.Pause => "Pause",
            Key.Apps => "Menu",

            // 方向键
            Key.Up => "↑",
            Key.Down => "↓",
            Key.Left => "←",
            Key.Right => "→",

            // 标点符号 (US 布局)
            Key.OemPlus => "=",
            Key.OemMinus => "-",
            Key.OemComma => ",",
            Key.OemPeriod => ".",
            Key.OemQuestion => "/",
            Key.OemSemicolon => ";",
            Key.OemQuotes => "'",
            Key.OemOpenBrackets => "[",
            Key.OemCloseBrackets => "]",
            Key.OemPipe => "\\",
            Key.OemTilde => "`",

            // 小键盘运算符
            Key.Add => "Num+",
            Key.Subtract => "Num-",
            Key.Multiply => "Num*",
            Key.Divide => "Num/",
            Key.Decimal => "Num.",

            // 多媒体键
            Key.VolumeUp => "Vol+",
            Key.VolumeDown => "Vol-",
            Key.VolumeMute => "Mute",
            Key.MediaNextTrack => "⏭",
            Key.MediaPreviousTrack => "⏮",
            Key.MediaPlayPause => "▶❙❙",
            Key.MediaStop => "⏹",

            _ => key.ToString()
        };
    }

    // ==================== Win32 ====================

    [DllImport("user32.dll")]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    protected override void OnClosed(EventArgs e)
    {
        _trayIcon.Dispose();
        _hook.Dispose();
        base.OnClosed(e);
    }
}
