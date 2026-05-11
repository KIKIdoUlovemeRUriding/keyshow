using System;
using System.Collections.Generic;
using System.Linq;

namespace KeyShow;

/// <summary>
/// 按键频率统计（最近 2 小时）
/// </summary>
public static class KeyStatsTracker
{
    /// <summary>是否启用流光特效</summary>
    public static bool GlowEnabled { get; set; } = true;

    private static readonly List<KeyRecord> _records = new();
    private static readonly TimeSpan _window = TimeSpan.FromHours(2);

    private struct KeyRecord
    {
        public string Key;
        public DateTime Time;
    }

    /// <summary>记录一次按键</summary>
    public static void Record(string keyText)
    {
        var now = DateTime.Now;
        _records.Add(new KeyRecord { Key = keyText, Time = now });
        // 清理过期记录
        var cutoff = now - _window;
        _records.RemoveAll(r => r.Time < cutoff);
    }

    /// <summary>获取某个按键在窗口内的次数</summary>
    public static int GetCount(string keyText)
    {
        return _records.Count(r => r.Key == keyText);
    }

    /// <summary>最大频次（用于归一化）</summary>
    public static int MaxCount
    {
        get
        {
            Cleanup();
            if (_records.Count == 0) return 0;
            return _records.GroupBy(r => r.Key).Max(g => g.Count());
        }
    }

    /// <summary>获取所有按键统计（降序）</summary>
    public static List<KeyStat> GetAllStats()
    {
        Cleanup();
        return _records
            .GroupBy(r => r.Key)
            .Select(g => new KeyStat { Key = g.Key, Count = g.Count() })
            .OrderByDescending(s => s.Count)
            .ToList();
    }

    /// <summary>总按键次数</summary>
    public static int TotalCount
    {
        get { Cleanup(); return _records.Count; }
    }

    private static void Cleanup()
    {
        var cutoff = DateTime.Now - _window;
        _records.RemoveAll(r => r.Time < cutoff);
    }
}

public class KeyStat
{
    public string Key { get; set; } = "";
    public int Count { get; set; }
}
