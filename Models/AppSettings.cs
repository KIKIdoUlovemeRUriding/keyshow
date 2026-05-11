using System;
using System.IO;
using System.Text.Json;
using System.Windows;

namespace KeyShow;

/// <summary>
/// 应用设置，JSON 持久化到 %LocalAppData%/KeyShow/settings.json
/// </summary>
public class AppSettings
{
    private static readonly string _dir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "KeyShow");
    private static readonly string _path = Path.Combine(_dir, "settings.json");
    private static AppSettings? _instance;

    public static AppSettings Instance => _instance ??= Load();

    // ===== 基础设置 =====

    public int MaxKeys { get; set; } = 5;
    public double VisibleSeconds { get; set; } = 1.5;
    public double FadeSeconds { get; set; } = 1.0;
    public string Position { get; set; } = "bottom-right";
    public bool AutoStart { get; set; } = false;

    /// <summary>语言："zh"(中文) / "en"(英文)</summary>
    public string Language { get; set; } = "zh";

    // ===== 外观设置 =====

    /// <summary>卡片垂直内边距（影响高度）</summary>
    public double CardPadding { get; set; } = 6;

    /// <summary>卡片最小宽度（px）</summary>
    public double CardMinWidth { get; set; } = 36;

    /// <summary>背景颜色，十六进制格式 #AARRGGBB</summary>
    public string BackgroundColor { get; set; } = "#BB2D2D30";

    /// <summary>字体颜色，十六进制格式 #AARRGGBB</summary>
    public string FontColor { get; set; } = "#F0F0F0";

    /// <summary>字体名称（系统字体名或导入字体的 FamilyName）</summary>
    public string FontFamilyName { get; set; } = "Consolas";

    /// <summary>字号（px）</summary>
    public double FontSize { get; set; } = 18;

    /// <summary>字体加粗</summary>
    public bool FontBold { get; set; } = true;

    /// <summary>自定义导入字体文件路径，空表示使用系统字体</summary>
    public string CustomFontPath { get; set; } = "";

    // ===== 持久化 =====

    public static AppSettings Load()
    {
        try
        {
            if (File.Exists(_path))
            {
                var json = File.ReadAllText(_path);
                return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
            }
        }
        catch { }
        return new AppSettings();
    }

    public void Save()
    {
        try
        {
            Directory.CreateDirectory(_dir);
            var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_path, json);
        }
        catch { }
    }

    public (double Left, double Top) GetWindowPosition(double width, double height)
    {
        var screen = SystemParameters.WorkArea;
        return Position switch
        {
            "bottom-left" => (screen.Left + 20, screen.Bottom - height - 30),
            "bottom-center" => (screen.Left + (screen.Width - width) / 2, screen.Bottom - height - 30),
            _ => (screen.Right - width - 20, screen.Bottom - height - 30),
        };
    }
}
