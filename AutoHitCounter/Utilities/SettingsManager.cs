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
    
    [DefaultValue("Dark Souls Remastered")]
    public string LastSelectedGame { get; set; }
    public double MainWindowLeft { get; set; }
    public double MainWindowTop { get; set; }
    public string HotkeyActionIds { get; set; } = "";
    public bool EnableHotkeys { get; set; }
    public bool GlobalHotkeys { get; set; }
    public bool BlockHotkeysFromGame { get; set; }
    public bool AlwaysOnTop { get; set; }
    public bool ShowNotesSection { get; set; }
    
    
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