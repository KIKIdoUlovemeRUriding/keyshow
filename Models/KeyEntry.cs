using System;
using System.ComponentModel;

namespace KeyShow;

/// <summary>
/// 按键条目：绑定到 UI 的数据模型
/// </summary>
public class KeyEntry : INotifyPropertyChanged
{
    public string BaseText { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public bool IsFading { get; set; }

    private string _text = "";
    public string Text
    {
        get => _text;
        set
        {
            if (_text != value)
            {
                _text = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Text)));
            }
        }
    }

    public int RepeatCount { get; set; } = 1;

    /// <summary>累加连击，≥5 次才显示 ×N</summary>
    public void BumpRepeat()
    {
        RepeatCount++;
        Text = RepeatCount >= 5 ? $"{BaseText} × {RepeatCount}" : BaseText;
        IsFading = false;
    }

    private double _opacity = 1.0;
    public double Opacity
    {
        get => _opacity;
        set
        {
            if (Math.Abs(_opacity - value) > 0.001)
            {
                _opacity = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Opacity)));
            }
        }
    }

    private double _scale = 1.0;
    public double Scale
    {
        get => _scale;
        set
        {
            if (Math.Abs(_scale - value) > 0.001)
            {
                _scale = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Scale)));
            }
        }
    }

    private string _glowColor = "Transparent";
    public string GlowColor
    {
        get => _glowColor;
        set
        {
            if (_glowColor != value)
            {
                _glowColor = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(GlowColor)));
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}
