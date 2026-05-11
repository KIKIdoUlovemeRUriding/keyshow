using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;

namespace KeyShow;

public partial class SettingsWindow : Window
{
    private readonly AppSettings _settings = AppSettings.Instance;
    private bool _loading;

    public SettingsWindow()
    {
        InitializeComponent();
        Icon = App.GetWindowIcon();
        ApplyLocale();
        PopulateFontList();
        _loading = true;
        LoadSettings();
        _loading = false;
    }

    private void ApplyLocale()
    {
        Title = L.SettingsTitle;
        // GroupBox headers
        MaxKeysHeader.Header = L.MaxKeys;
        CardHeightHeader.Header = L.CardHeight;
        CardMinWidthHeader.Header = L.CardMinWidth;
        VisibleHeader.Header = L.VisibleDuration;
        FadeHeader.Header = L.FadeDuration;
        BgColorHeader.Header = L.BgColor;
        FontColorHeader.Header = L.FontColor;
        FontHeader.Header = L.Font;
        PositionHeader.Header = L.DisplayPosition;
        PosBottomRight.Content = L.PosBottomRight;
        PosBottomLeft.Content = L.PosBottomLeft;
        PosBottomCenter.Content = L.PosBottomCenter;
        PosDragTip.Text = L.PosDragTip;
        AutoStartCheck.Content = L.AutoStart;
        BgColorBtn.Content = L.PickColor;
        FontColorBtn.Content = L.PickColor;
        ImportFontBtn.Content = L.ImportFont;
        FontBoldCheck.Content = L.FontBold;
        FontSizeLabel.Text = L.FontSize;
        SaveBtn.Content = L.Save;
        CancelBtn.Content = L.Cancel;
        LanguageHeader.Header = L.Language;
        LangZhRadio.Content = "中文";
        LangEnRadio.Content = "English";
    }

    // ==================== 字体列表 ====================

    private void PopulateFontList()
    {
        var fonts = new List<string>();
        foreach (var f in Fonts.SystemFontFamilies.OrderBy(f => f.Source))
            fonts.Add(f.Source);
        FontCombo.ItemsSource = fonts;
    }

    // ==================== 加载设置 ====================

    private void LoadSettings()
    {
        MaxKeysSlider.Value = _settings.MaxKeys;
        MaxKeysLabel.Text = _settings.MaxKeys.ToString();

        CardPaddingSlider.Value = _settings.CardPadding;
        CardPaddingLabel.Text = _settings.CardPadding.ToString("F0");

        CardMinWidthSlider.Value = _settings.CardMinWidth;
        CardMinWidthLabel.Text = _settings.CardMinWidth.ToString("F0");

        VisibleSlider.Value = _settings.VisibleSeconds;
        VisibleLabel.Text = _settings.VisibleSeconds.ToString("0.0");

        FadeSlider.Value = _settings.FadeSeconds;
        FadeLabel.Text = _settings.FadeSeconds.ToString("0.0");

        UpdateBgPreview(_settings.BackgroundColor);
        UpdateFontPreview(_settings.FontColor);

        FontCombo.SelectedItem = _settings.FontFamilyName;
        if (!string.IsNullOrEmpty(_settings.CustomFontPath))
            CustomFontLabel.Text = $"{L.FontImported}{Path.GetFileName(_settings.CustomFontPath)}";

        switch (_settings.Position)
        {
            case "bottom-left": PosBottomLeft.IsChecked = true; break;
            case "bottom-center": PosBottomCenter.IsChecked = true; break;
            default: PosBottomRight.IsChecked = true; break;
        }

        AutoStartCheck.IsChecked = _settings.AutoStart;
        FontBoldCheck.IsChecked = _settings.FontBold;
        FontSizeSlider.Value = _settings.FontSize;
        FontSizeValue.Text = _settings.FontSize.ToString("F0");

        // 语言
        if (_settings.Language == "en") LangEnRadio.IsChecked = true;
        else LangZhRadio.IsChecked = true;
    }

    // ==================== 语言 ====================

    private bool _languageChanged;
    private void Language_Changed(object sender, RoutedEventArgs e)
    {
        if (_loading) return;
        if (LangZhRadio.IsChecked == true)
            _settings.Language = "zh";
        else if (LangEnRadio.IsChecked == true)
            _settings.Language = "en";
        _languageChanged = true;
    }

    // ==================== 颜色选择 ====================

    private void BgColorBtn_Click(object sender, RoutedEventArgs e)
    {
        var initial = ParseColor(_settings.BackgroundColor);
        var result = ColorPickerWindow.Pick(this, initial);
        if (result.HasValue)
        {
            _settings.BackgroundColor = result.Value.ToString();
            UpdateBgPreview(_settings.BackgroundColor);
        }
    }

    private void FontColorBtn_Click(object sender, RoutedEventArgs e)
    {
        var initial = ParseColor(_settings.FontColor);
        var result = ColorPickerWindow.Pick(this, initial);
        if (result.HasValue)
        {
            _settings.FontColor = result.Value.ToString();
            UpdateFontPreview(_settings.FontColor);
        }
    }

    private static Color ParseColor(string hex)
    {
        try { return (Color)ColorConverter.ConvertFromString(hex); }
        catch { return Colors.Gray; }
    }

    private void UpdateBgPreview(string hex) =>
        BgColorPreview.Background = new SolidColorBrush(ParseColor(hex));

    private void UpdateFontPreview(string hex) =>
        FontColorPreview.Background = new SolidColorBrush(ParseColor(hex));

    // ==================== 滑块事件 ====================

    private void MaxKeysSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (MaxKeysLabel == null) return;
        var val = (int)MaxKeysSlider.Value;
        MaxKeysLabel.Text = val.ToString();
        _settings.MaxKeys = val;
    }

    private void CardPaddingSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (CardPaddingLabel == null) return;
        var val = (int)CardPaddingSlider.Value;
        CardPaddingLabel.Text = val.ToString();
        _settings.CardPadding = val;
    }

    private void CardMinWidthSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (CardMinWidthLabel == null) return;
        var val = (int)CardMinWidthSlider.Value;
        CardMinWidthLabel.Text = val.ToString();
        _settings.CardMinWidth = val;
    }

    private void VisibleSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (VisibleLabel == null) return;
        var val = Math.Round(VisibleSlider.Value, 1);
        VisibleLabel.Text = val.ToString("0.0");
        _settings.VisibleSeconds = val;
    }

    private void FadeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (FadeLabel == null) return;
        var val = Math.Round(FadeSlider.Value, 1);
        FadeLabel.Text = val.ToString("0.0");
        _settings.FadeSeconds = val;
    }

    // ==================== 字体事件 ====================

    private void FontCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (FontCombo.SelectedItem is string name)
            _settings.FontFamilyName = name;
    }

    private void ImportFont_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new OpenFileDialog
        {
            Title = L.FontDialogTitle,
            Filter = L.FontFilter
        };
        if (dlg.ShowDialog() == true)
        {
            try
            {
                var families = Fonts.GetFontFamilies(dlg.FileName);
                foreach (var f in families)
                {
                    _settings.CustomFontPath = dlg.FileName;
                    _settings.FontFamilyName = f.Source;
                    FontCombo.SelectedItem = f.Source;
                    CustomFontLabel.Text = $"{L.FontImported}{Path.GetFileName(dlg.FileName)} ({f.Source})";
                    break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{L.FontLoadError}{ex.Message}", L.SettingsTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void FontSizeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (FontSizeValue == null) return;
        var val = (int)FontSizeSlider.Value;
        FontSizeValue.Text = val.ToString();
        _settings.FontSize = val;
    }

    private void FontBold_Changed(object sender, RoutedEventArgs e)
    {
        _settings.FontBold = FontBoldCheck.IsChecked == true;
    }

    // ==================== 位置事件 ====================

    private void Position_Changed(object sender, RoutedEventArgs e)
    {
        if (PosBottomRight.IsChecked == true)
            _settings.Position = "bottom-right";
        else if (PosBottomLeft.IsChecked == true)
            _settings.Position = "bottom-left";
        else if (PosBottomCenter.IsChecked == true)
            _settings.Position = "bottom-center";
    }

    // ==================== 保存 / 取消 ====================

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        _settings.AutoStart = AutoStartCheck.IsChecked == true;
        ApplyAutoStart();
        _settings.Save();
        DialogResult = true;
        Close();

        if (_languageChanged)
            MessageBox.Show(L.RestartRequired, L.SettingsTitle, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void ApplyAutoStart()
    {
        var startupDir = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
        var shortcut = Path.Combine(startupDir, "KeyShow.lnk");

        try
        {
            if (_settings.AutoStart)
            {
                var exe = Environment.ProcessPath ?? "";
                if (!string.IsNullOrEmpty(exe) && !File.Exists(shortcut))
                    CreateShortcut(exe, shortcut);
            }
            else
            {
                if (File.Exists(shortcut))
                    File.Delete(shortcut);
            }
        }
        catch { }
    }

    private static void CreateShortcut(string targetPath, string shortcutPath)
    {
        var script = $@"
            $WshShell = New-Object -ComObject WScript.Shell
            $Shortcut = $WshShell.CreateShortcut('{shortcutPath}')
            $Shortcut.TargetPath = '{targetPath}'
            $Shortcut.Save()
        ";
        var psi = new System.Diagnostics.ProcessStartInfo("powershell.exe",
            $"-NoProfile -NonInteractive -Command \"{script.Replace("\n", " ").Replace("\r", "")}\"")
        {
            CreateNoWindow = true,
            UseShellExecute = false
        };
        var p = System.Diagnostics.Process.Start(psi);
        p?.WaitForExit(3000);
    }
}
