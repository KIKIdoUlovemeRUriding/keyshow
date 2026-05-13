using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace KeyShow;

public partial class StatsWindow : Window
{
    public StatsWindow()
    {
        InitializeComponent();
        Icon = App.GetWindowIcon();
        ApplyLocale();
        GlowToggle.IsChecked = KeyStatsTracker.GlowEnabled;
        RefreshStats();
    }

    private void ApplyLocale()
    {
        Title = L.StatsTitle;
        HeatLabel.Text = L.HeatLegend;
        GoldLabel.Text = L.IsEn ? "≥500" : "≥500次";
        BlueLabel.Text = L.IsEn ? "≥200" : "≥200次";
        NoneLabel.Text = L.IsEn ? "<200" : "<200次";
        GlowToggle.Content = L.GlowToggle;
        RefreshBtn.Content = L.Refresh;
        CloseBtn.Content = L.Close;
        ColRank.Header = L.Rank;
        ColKey.Header = L.KeyHeader;
        ColCount.Header = L.CountHeader;
        ColHeat.Header = L.HeatHeader;
    }

    private void GlowToggle_Changed(object sender, RoutedEventArgs e)
    {
        KeyStatsTracker.GlowEnabled = GlowToggle.IsChecked == true;
        if (Application.Current.MainWindow is MainWindow mw)
            mw.RefreshAllGlowEffects();
    }

    private void RefreshStats()
    {
        var stats = KeyStatsTracker.GetAllStats();
        var total = KeyStatsTracker.TotalCount;
        var max = stats.Count > 0 ? stats[0].Count : 0;

        TotalLabel.Text = $"{L.TotalKeys}: {total:N0}";
        UniqueLabel.Text = $"{L.UniqueKeys}: {stats.Count}";

        var items = new List<StatItem>();
        int rank = 0;
        foreach (var s in stats)
        {
            rank++;
            double ratio = max > 0 ? (double)s.Count / max : 0;
            items.Add(new StatItem
            {
                Rank = rank,
                Key = s.Key,
                Count = s.Count,
                HeatBrush = ratio > 0.6 ? new SolidColorBrush(Color.FromRgb(0xFF, 0xD7, 0x00))
                          : ratio > 0.3 ? new SolidColorBrush(Color.FromRgb(0x4F, 0xC3, 0xF7))
                          : new SolidColorBrush(Color.FromRgb(0x55, 0x55, 0x55)),
                HeatWidth = Math.Max(6, ratio * 100)
            });
        }

        StatsList.ItemsSource = items;
    }

    private void Refresh_Click(object sender, RoutedEventArgs e) => RefreshStats();
    private void Close_Click(object sender, RoutedEventArgs e) => Close();
}

public class StatItem
{
    public int Rank { get; set; }
    public string Key { get; set; } = "";
    public int Count { get; set; }
    public Brush HeatBrush { get; set; } = Brushes.Gray;
    public double HeatWidth { get; set; }
}
