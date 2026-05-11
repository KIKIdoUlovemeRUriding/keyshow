namespace KeyShow;

/// <summary>
/// 多语言支持：根据 AppSettings.Language 返回中文或英文字符串
/// </summary>
public static class L
{
    public static bool IsEn => AppSettings.Instance.Language == "en";

    // ===== 托盘 =====
    public static string TrayText => IsEn ? "KeyShow - Key Display" : "KeyShow - 按键显示";
    public static string TrayStats => IsEn ? "Key Statistics" : "热键统计";
    public static string TraySettings => IsEn ? "Settings" : "设置";
    public static string TrayExit => IsEn ? "Exit" : "退出";

    // ===== 设置窗口 =====
    public static string SettingsTitle => IsEn ? "KeyShow Settings" : "KeyShow 设置";
    public static string MaxKeys => IsEn ? "Max Keys Displayed" : "最大同时显示按键数";
    public static string CardHeight => IsEn ? "Card Height" : "卡片高度";
    public static string CardMinWidth => IsEn ? "Card Min Width" : "卡片最小宽度";
    public static string VisibleDuration => IsEn ? "Visible Duration (s)" : "完全显示时长（秒）";
    public static string FadeDuration => IsEn ? "Fade Duration (s)" : "淡出动画时长（秒）";
    public static string BgColor => IsEn ? "Card Background Color" : "卡片背景颜色";
    public static string FontColor => IsEn ? "Font Color" : "字体颜色";
    public static string Font => IsEn ? "Font" : "字体";
    public static string FontSize => IsEn ? "Font Size" : "字号";
    public static string FontBold => IsEn ? "Bold" : "字体加粗";
    public static string DisplayPosition => IsEn ? "Display Position" : "显示位置";
    public static string PosBottomRight => IsEn ? "Bottom Right" : "屏幕右下角";
    public static string PosBottomLeft => IsEn ? "Bottom Left" : "屏幕左下角";
    public static string PosBottomCenter => IsEn ? "Bottom Center" : "屏幕底部居中";
    public static string PosDragTip => IsEn
        ? "Tip: You can also drag the key cards to any position"
        : "提示：也可以直接拖动按键卡片到任意位置";
    public static string AutoStart => IsEn ? "Start with Windows" : "开机自动启动";
    public static string PickColor => IsEn ? "Pick Color..." : "选择颜色...";
    public static string ImportFont => IsEn ? "Import Font (.ttf/.otf)" : "导入字体文件 (.ttf / .otf)";
    public static string Save => IsEn ? "Save" : "保存";
    public static string Cancel => IsEn ? "Cancel" : "取消";
    public static string Language => IsEn ? "Language" : "语言";
    public static string LangZh => IsEn ? "Chinese" : "中文";
    public static string LangEn => IsEn ? "English" : "英文";
    public static string RestartRequired => IsEn
        ? "Please restart the application for language change to take effect."
        : "语言变更需要重启应用生效。";

    // ===== 统计窗口 =====
    public static string StatsTitle => IsEn ? "Key Statistics - Last 2 Hours" : "热键统计 - 最近 2 小时";
    public static string TotalKeys => IsEn ? "Total Keystrokes" : "总计按键";
    public static string UniqueKeys => IsEn ? "Unique Keys" : "不同按键";
    public static string HeatLegend => IsEn ? "Frequency:" : "热度：";
    public static string Rank => IsEn ? "Rank" : "排名";
    public static string KeyHeader => IsEn ? "Key" : "按键";
    public static string CountHeader => IsEn ? "Count" : "次数";
    public static string HeatHeader => IsEn ? "Heat" : "热度";
    public static string GlowToggle => IsEn ? "Enable Glow Effect" : "启用按键流光特效";
    public static string Refresh => IsEn ? "Refresh" : "刷新";
    public static string Close => IsEn ? "Close" : "关闭";

    // ===== 色盘窗口 =====
    public static string ColorPickerTitle => IsEn ? "Pick Color" : "选择颜色";
    public static string CurrentColor => IsEn ? "Current" : "当前";
    public static string CommonColors => IsEn ? "Common Colors" : "常用颜色";
    public static string Ok => IsEn ? "OK" : "确定";

    // ===== 字体导入 =====
    public static string FontImported => IsEn ? "Imported: " : "已导入: ";
    public static string FontLoadError => IsEn ? "Cannot load font: " : "无法加载字体: ";
    public static string FontDialogTitle => IsEn ? "Select Font File" : "选择字体文件";
    public static string FontFilter => IsEn ? "Font Files|*.ttf;*.otf|All Files|*.*" : "字体文件|*.ttf;*.otf|所有文件|*.*";
}
