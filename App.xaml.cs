using System;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace KeyShow;

public partial class App : Application
{
    /// <summary>
    /// 从嵌入资源加载 icon.ico，用于系统托盘（WinForms）
    /// </summary>
    public static Icon GetTrayIcon()
    {
        var uri = new Uri("pack://application:,,,/icon.ico");
        var info = GetResourceStream(uri);
        using var ms = new MemoryStream();
        info.Stream.CopyTo(ms);
        ms.Position = 0;
        return new Icon(ms);
    }

    /// <summary>
    /// 从嵌入资源加载 icon.ico，用于 WPF 窗口
    /// </summary>
    public static BitmapFrame GetWindowIcon()
    {
        var uri = new Uri("pack://application:,,,/icon.ico");
        var info = GetResourceStream(uri);
        return BitmapFrame.Create(info.Stream);
    }
}
