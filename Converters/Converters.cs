using System;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace KeyShow;

/// <summary>
/// 十六进制颜色字符串 → Brush 转换器
/// </summary>
public class StringToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var str = value as string ?? "#FFF";
        try { return new SolidColorBrush((Color)ColorConverter.ConvertFromString(str)); }
        catch { return new SolidColorBrush(Colors.White); }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// 字体名称/路径 → FontFamily（支持系统字体和导入字体）
/// </summary>
public class FontFamilyConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var settings = AppSettings.Instance;
        try
        {
            // 自定义导入字体优先
            if (!string.IsNullOrEmpty(settings.CustomFontPath) && File.Exists(settings.CustomFontPath))
            {
                var families = Fonts.GetFontFamilies(settings.CustomFontPath);
                foreach (var f in families)
                    return f; // 返回第一个 family
            }
            // 系统字体
            return new System.Windows.Media.FontFamily(settings.FontFamilyName);
        }
        catch { return new System.Windows.Media.FontFamily("Consolas"); }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

/// <summary>
/// double → Thickness(12, value, 12, value) 用于卡片内边距
/// <summary>
/// bool → FontWeight (true=Bold, false=Normal)
/// </summary>
public class BoolToFontWeightConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (value is true) ? FontWeights.Bold : FontWeights.Normal;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

/// <summary>
/// double → Thickness(12, value, 12, value) 用于卡片内边距
/// </summary>
public class CardPaddingConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var v = value is double d ? d : 6.0;
        return new Thickness(12, v, 12, v);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
