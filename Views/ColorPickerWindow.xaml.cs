using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace KeyShow;

public partial class ColorPickerWindow : Window
{
    private double _hue;       // 0-360
    private double _sat = 1;   // 0-1
    private double _val = 1;   // 0-1
    private double _alpha = 1; // 0-1
    private bool _svDragging, _hueDragging, _alphaDragging;

    private static readonly (string Hex, string Label)[] _palette = new[]
    {
        ("#FF000000", "纯黑"), ("#FF333333", "深灰"), ("#FF666666", "中灰"),
        ("#FF999999", "浅灰"), ("#FFCCCCCC", "亮灰"), ("#FFFFFFFF", "纯白"),
        ("#FFFF0000", "红"), ("#FFFF6B00", "橙"), ("#FFFFD700", "金"),
        ("#FF00AA00", "绿"), ("#FF00CED1", "青"), ("#FF0066FF", "蓝"),
        ("#FF8A2BE2", "紫"), ("#FFFF1493", "粉"), ("#FF8B4513", "棕"),
        ("#FF2D2D30", "卡片"), ("#FF1E1E1E", "暗黑"), ("#FF16213E", "藏蓝"),
    };

    /// <summary>最终选中的颜色 (含 Alpha)</summary>
    public Color SelectedColor { get; private set; }

    /// <summary>是否为有效确认（点确定）</summary>
    public bool Confirmed { get; private set; }

    public ColorPickerWindow(Color initialColor)
    {
        InitializeComponent();
        Icon = App.GetWindowIcon();
        ApplyLocale();
        SelectedColor = initialColor;

        // 从初始颜色反推 HSV
        RgbToHsv(initialColor, out _hue, out _sat, out _val);
        _alpha = initialColor.A / 255.0;

        UpdateHueFill();
        UpdateAllIndicators();
        BuildPalette();

        Loaded += (_, _) =>
        {
            UpdateHueFill();
            UpdateAllIndicators();
            UpdatePreview();
        };
    }

    private void ApplyLocale()
    {
        Title = L.ColorPickerTitle;
        CurrentLabel.Text = L.CurrentColor;
        CommonLabel.Text = L.CommonColors;
        OkBtn.Content = L.Ok;
        CancelBtn.Content = L.Cancel;
    }

    // ==================== HSV ↔ RGB ====================

    private static void RgbToHsv(Color c, out double h, out double s, out double v)
    {
        double r = c.R / 255.0, g = c.G / 255.0, b = c.B / 255.0;
        double max = Math.Max(r, Math.Max(g, b));
        double min = Math.Min(r, Math.Min(g, b));
        double delta = max - min;

        h = 0;
        if (delta > 0.0001)
        {
            if (Math.Abs(max - r) < 0.0001)
                h = 60 * (((g - b) / delta) % 6);
            else if (Math.Abs(max - g) < 0.0001)
                h = 60 * ((b - r) / delta + 2);
            else
                h = 60 * ((r - g) / delta + 4);
        }
        if (h < 0) h += 360;

        s = max < 0.0001 ? 0 : delta / max;
        v = max;
    }

    private static Color HsvToRgb(double h, double s, double v, byte alpha = 255)
    {
        double c = v * s;
        double x = c * (1 - Math.Abs((h / 60) % 2 - 1));
        double m = v - c;

        double r, g, b;
        if (h < 60) { r = c; g = x; b = 0; }
        else if (h < 120) { r = x; g = c; b = 0; }
        else if (h < 240) { r = 0; g = c; b = x; }
        else if (h < 240) { r = 0; g = x; b = c; }
        else if (h < 300) { r = x; g = 0; b = c; }
        else { r = c; g = 0; b = x; }

        return Color.FromArgb(alpha,
            (byte)Math.Clamp((r + m) * 255, 0, 255),
            (byte)Math.Clamp((g + m) * 255, 0, 255),
            (byte)Math.Clamp((b + m) * 255, 0, 255));
    }

    // ==================== 预览更新 ====================

    private void UpdateHueFill()
    {
        HueFill.Fill = new SolidColorBrush(HsvToRgb(_hue, 1, 1));
    }

    private void UpdatePreview()
    {
        var c = HsvToRgb(_hue, _sat, _val, (byte)(_alpha * 255));
        SelectedColor = c;
        ColorPreview.Background = new SolidColorBrush(c);
        HexLabel.Text = c.ToString();
    }

    // ==================== 指示器位置 ====================

    private void UpdateSvIndicator()
    {
        double x = _sat * 280 - 6;
        double y = (1 - _val) * 180 - 6;
        Canvas.SetLeft(SvCrosshair, x);
        Canvas.SetTop(SvCrosshair, y);
    }

    private void UpdateHueIndicator()
    {
        Canvas.SetTop(HueIndicator, _hue / 360 * 180 - 2);
    }

    private void UpdateAlphaIndicator()
    {
        Canvas.SetTop(AlphaIndicator, (1 - _alpha) * 240 - 2);
    }

    private void UpdateAllIndicators()
    {
        UpdateSvIndicator();
        UpdateHueIndicator();
        UpdateAlphaIndicator();
    }

    // ==================== SV 平面交互 ====================

    private void SvCanvas_MouseDown(object sender, MouseButtonEventArgs e)
    {
        _svDragging = true;
        ((UIElement)sender).CaptureMouse();
        UpdateSvFromMouse(e.GetPosition((UIElement)sender));
    }

    private void SvCanvas_MouseMove(object sender, MouseEventArgs e)
    {
        if (!_svDragging) return;
        UpdateSvFromMouse(e.GetPosition((UIElement)sender));
    }

    private void SvCanvas_MouseUp(object sender, MouseButtonEventArgs e)
    {
        _svDragging = false;
        ((UIElement)sender).ReleaseMouseCapture();
    }

    private void UpdateSvFromMouse(Point pos)
    {
        _sat = Math.Clamp(pos.X / 280, 0, 1);
        _val = Math.Clamp(1 - pos.Y / 180, 0, 1);
        UpdateSvIndicator();
        UpdatePreview();
    }

    // ==================== 色相条交互 ====================

    private void HueBar_MouseDown(object sender, MouseButtonEventArgs e)
    {
        _hueDragging = true;
        ((UIElement)sender).CaptureMouse();
        UpdateHueFromMouse(e.GetPosition((UIElement)sender));
    }

    private void HueBar_MouseMove(object sender, MouseEventArgs e)
    {
        if (!_hueDragging) return;
        UpdateHueFromMouse(e.GetPosition((UIElement)sender));
    }

    private void HueBar_MouseUp(object sender, MouseButtonEventArgs e)
    {
        _hueDragging = false;
        ((UIElement)sender).ReleaseMouseCapture();
    }

    private void UpdateHueFromMouse(Point pos)
    {
        _hue = Math.Clamp(pos.Y / 180 * 360, 0, 360);
        UpdateHueFill();
        UpdateHueIndicator();
        UpdatePreview();
    }

    // ==================== 透明度条交互 ====================

    private void AlphaBar_MouseDown(object sender, MouseButtonEventArgs e)
    {
        _alphaDragging = true;
        ((UIElement)sender).CaptureMouse();
        UpdateAlphaFromMouse(e.GetPosition((UIElement)sender));
    }

    private void AlphaBar_MouseMove(object sender, MouseEventArgs e)
    {
        if (!_alphaDragging) return;
        UpdateAlphaFromMouse(e.GetPosition((UIElement)sender));
    }

    private void AlphaBar_MouseUp(object sender, MouseButtonEventArgs e)
    {
        _alphaDragging = false;
        ((UIElement)sender).ReleaseMouseCapture();
    }

    private void UpdateAlphaFromMouse(Point pos)
    {
        _alpha = Math.Clamp(1 - pos.Y / 180, 0, 1);
        UpdateAlphaIndicator();
        UpdatePreview();
    }

    // ==================== 常用色板 ====================

    private void BuildPalette()
    {
        PalettePanel.Children.Clear();
        foreach (var (hex, label) in _palette)
        {
            var b = new Border
            {
                Width = 28, Height = 28, CornerRadius = new CornerRadius(3),
                Background = ParseBrush(hex), Margin = new Thickness(2),
                Cursor = Cursors.Hand, ToolTip = label
            };
            b.MouseLeftButtonDown += (_, _) =>
            {
                var c = (Color)ColorConverter.ConvertFromString(hex);
                RgbToHsv(c, out _hue, out _sat, out _val);
                _alpha = c.A / 255.0;
                UpdateHueFill();
                UpdateAllIndicators();
                UpdatePreview();
            };
            PalettePanel.Children.Add(b);
        }
    }

    private static SolidColorBrush ParseBrush(string hex)
    {
        try { return new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex)); }
        catch { return Brushes.Gray; }
    }

    // ==================== 按钮 ====================

    private void Ok_Click(object sender, RoutedEventArgs e)
    {
        Confirmed = true;
        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        Confirmed = false;
        DialogResult = false;
        Close();
    }

    /// <summary>静态方法：打开色盘并返回颜色</summary>
    public static Color? Pick(Window owner, Color initial)
    {
        var w = new ColorPickerWindow(initial) { Owner = owner };
        return w.ShowDialog() == true ? w.SelectedColor : null;
    }
}
