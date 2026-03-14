// 

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace AutoHitCounter.Utilities;

public class SettingsManager
{
    private static SettingsManager _default;
    public static SettingsManager Default => _default ??= Load();

    private static string SettingsPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "AutoHitCounter",
        "settings.txt");

    [DefaultValue(true)] public bool EnableUpdateChecks { get; set; }

    [DefaultValue("Dark Souls Remastered")]
    public string LastSelectedGame { get; set; }

    public string LastSelectedProfile { get; set; }
    public double MainWindowLeft { get; set; }
    public double MainWindowTop { get; set; }
    public string HotkeyActionIds { get; set; } = "";
    public bool EnableHotkeys { get; set; }
    public bool GlobalHotkeys { get; set; }
    public bool BlockHotkeysFromGame { get; set; }
    public bool AlwaysOnTop { get; set; }
    public bool ShowNotesSection { get; set; }
    public bool PracticeMode { get; set; }
    [DefaultValue(true)] public bool ShowAttempts { get; set; }
    [DefaultValue(true)] public bool ShowProgress { get; set; }
    [DefaultValue(4)] public int PrevSplits { get; set; }
    [DefaultValue(13)] public int NextSplits { get; set; }
    [DefaultValue(true)] public bool ShowDiff { get; set; }
    [DefaultValue(true)] public bool ShowPb { get; set; }
    [DefaultValue(true)] public bool ShowIgt { get; set; }
    [DefaultValue(300)] public int OverlayWidth { get; set; }
    [DefaultValue(420)] public int OverlayHeight { get; set; }
    public bool IsUnlocked { get; set; }
    public bool DS3NoLogo { get; set; }
    public bool DS3StutterFix { get; set; }
    public bool ERNoLogo { get; set; }
    public bool ERStutterFix { get; set; }
    public bool ERDisableAchievements { get; set; }
    public bool SKNoLogo { get; set; }
    public bool SKNoTutorials { get; set; }
    public bool DS2NoBabyJump { get; set; }
    public bool DS2SkipCredits { get; set; }
    public bool DS2DisableDoubleClick { get; set; }

    public string LastImportExportPath { get; set; }

    public double EventLogWindowLeft { get; set; }
    public double EventLogWindowTop { get; set; }
    public bool EventLogWindowAlwaysOnTop { get; set; }

    // Typography
    [DefaultValue("Segoe UI")] public string FontFamily { get; set; }
    [DefaultValue(15)] public int FontSize { get; set; }

    [DefaultValue(false)] public bool FontBold { get; set; }

    [DefaultValue(false)] public bool FontItalic { get; set; }

    [DefaultValue(false)] public bool FontUnderline { get; set; }

    [DefaultValue("#e0e0e0")] public string SplitNameColor { get; set; }

    [DefaultValue("#e0e0e0")] public string SplitNameOnHitColor { get; set; }

    [DefaultValue("#e0e0e0")] public string SplitNameOnHitlessColor { get; set; }

    [DefaultValue("#999999")] public string GroupNameColor { get; set; }

    [DefaultValue("#bbbbbb")] public string PbColor { get; set; }

    [DefaultValue(29)] public int RowHeight { get; set; }

// Background
    [DefaultValue(0)] public double BackgroundOpacity { get; set; }
    [DefaultValue(false)] public bool TableMode { get; set; }

// Hit column

    [DefaultValue("#888888")] public string HitsCurrentColor { get; set; }
    [DefaultValue("#00cc66")] public string HitsClearedColor { get; set; }
    [DefaultValue("#c8843a")] public string HitsActiveColor { get; set; }

// Colours
    [DefaultValue("rgba(255, 76, 76, 0.17)")]
    public string RowHitColor { get; set; }

    [DefaultValue("rgba(0, 204, 102, 0.17)")]
    public string RowClearedColor { get; set; }

    [DefaultValue("rgba(0, 204, 102, 0.06)")]
    public string CurrentSplitColor { get; set; }

    [DefaultValue("#00cc66")] public string CurrentSplitBorderColor { get; set; }

    [DefaultValue("rgba(255, 76, 76, 0.06)")]
    public string CurrentSplitHitColor { get; set; }

    [DefaultValue("#ff4c4c")] public string CurrentSplitHitBorderColor { get; set; }

// Diff
    [DefaultValue("#ff4c4c")] public string DiffPosColor { get; set; }
    [DefaultValue("#00cc66")] public string DiffNegColor { get; set; }
    [DefaultValue("#bbbbbb")] public string DiffZeroColor { get; set; }

// Attempts
    [DefaultValue("#ffffff")] public string AttemptsZeroColor { get; set; }
    [DefaultValue("#9D61A8")] public string AttemptsActiveColor { get; set; }

// Header
    [DefaultValue("#bbbbbb")] public string HeaderTextColor { get; set; }

// Footer
    [DefaultValue("#c47fd4")] public string IgtColor { get; set; }
    [DefaultValue("#00cc66")] public string RunCompleteBannerColor { get; set; }

    [DefaultValue(true)] public bool ShowFooterTotals { get; set; }

    [DefaultValue("Consolas")] public string IgtFontFamily { get; set; }

    [DefaultValue(16)] public int IgtFontSize { get; set; }

    [DefaultValue("rgba(255, 255, 255, 0.05)")]
    public string AlternatingRows { get; set; }

    [DefaultValue("Segoe UI")] public string HeaderFontFamily { get; set; }
    [DefaultValue(11)] public int HeaderFontSize { get; set; }

    // Title
    [DefaultValue(false)] public bool ShowTitle { get; set; }
    [DefaultValue("")] public string TitleText { get; set; }
    [DefaultValue("#e0e0e0")] public string TitleColor { get; set; }
    [DefaultValue("Segoe UI")] public string TitleFontFamily { get; set; }
    [DefaultValue(18)] public int TitleFontSize { get; set; }

    [DefaultValue(false)] public bool ShowRunComplete { get; set; }
    [DefaultValue("✓ Run Complete")] public string RunCompleteText { get; set; }
    [DefaultValue(false)] public bool PbMatchesHit { get; set; }
    [DefaultValue("#c8843a")] public string FooterHitFontColor { get; set; }
    [DefaultValue("#888888")] public string FooterHitsCurrentColor { get; set; }
    [DefaultValue("#bbbbbb")] public string FooterPbFontColor { get; set; }
    [DefaultValue("Segoe UI")] public string FooterFontFamily { get; set; }
    [DefaultValue(15)] public int FooterFontSize { get; set; }


    public void Save()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath)!);
            var lines = new List<string>();

            foreach (var prop in GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var value = prop.GetValue(this);
                var stringValue = value switch
                {
                    double d => d.ToString(CultureInfo.InvariantCulture),
                    float f => f.ToString(CultureInfo.InvariantCulture),
                    _ => value?.ToString() ?? ""
                };
                lines.Add($"{prop.Name}={stringValue}");
            }

            File.WriteAllLines(SettingsPath, lines);
        }
        catch (Exception ex)
        {
            Console.WriteLine($@"Error saving settings: {ex.Message}");
        }
    }

    private static SettingsManager Load()
    {
        var settings = new SettingsManager();

        foreach (var prop in typeof(SettingsManager).GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var defaultAttr = prop.GetCustomAttribute<DefaultValueAttribute>();
            if (defaultAttr != null)
                prop.SetValue(settings, defaultAttr.Value);
        }

        if (!File.Exists(SettingsPath))
            return settings;

        try
        {
            var props = new Dictionary<string, PropertyInfo>();
            foreach (var prop in typeof(SettingsManager).GetProperties(BindingFlags.Public | BindingFlags.Instance))
                props[prop.Name] = prop;

            foreach (var line in File.ReadAllLines(SettingsPath))
            {
                var parts = line.Split(['='], 2);
                if (parts.Length != 2) continue;

                var key = parts[0];
                var value = parts[1];

                if (!props.TryGetValue(key, out var prop)) continue;

                object parsed = prop.PropertyType switch
                {
                    { } t when t == typeof(double) =>
                        double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var d) ? d : 0.0,
                    { } t when t == typeof(float) =>
                        float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var f) ? f : 0f,
                    { } t when t == typeof(int) =>
                        int.TryParse(value, out var i) ? i : (object)null,
                    { } t when t == typeof(bool) =>
                        bool.TryParse(value, out var b) && b,
                    { } t when t == typeof(string) => value,
                    _ => null
                };

                if (parsed != null)
                    prop.SetValue(settings, parsed);
            }
        }
        catch
        {
            // Return default settings on error
        }

        return settings;
    }
}