//

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using AutoHitCounter.Models;

namespace AutoHitCounter.Utilities;

public class OverlayProfileManager
{
    private static readonly string ProfilesDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "AutoHitCounter",
        "overlay-profiles");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    private static readonly JsonSerializerOptions ReadOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    private string _activeProfileName;

    public string ActiveProfileName => _activeProfileName;

    public OverlayProfileManager()
    {
        Directory.CreateDirectory(ProfilesDir);
        _activeProfileName = SettingsManager.Default.SelectedOverlayProfile;

        if (string.IsNullOrEmpty(_activeProfileName))
            _activeProfileName = "Default";
    }

    public OverlayConfig LoadActiveProfile()
    {
        var path = GetProfilePath(_activeProfileName);
        if (!File.Exists(path))
        {
            var config = CreateDefaultConfig();
            SaveActiveProfile(config);
            return config;
        }

        try
        {
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<OverlayConfig>(json, ReadOptions) ?? CreateDefaultConfig();
        }
        catch
        {
            return CreateDefaultConfig();
        }
    }

    public void SaveActiveProfile(OverlayConfig config)
    {
        try
        {
            Directory.CreateDirectory(ProfilesDir);
            var json = JsonSerializer.Serialize(config, JsonOptions);
            File.WriteAllText(GetProfilePath(_activeProfileName), json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($@"Error saving overlay profile: {ex.Message}");
        }
    }

    public void SetActiveProfile(string name)
    {
        _activeProfileName = name;
        SettingsManager.Default.SelectedOverlayProfile = name;
        SettingsManager.Default.Save();
    }

    public List<string> GetProfileNames()
    {
        if (!Directory.Exists(ProfilesDir))
            return new List<string> { "Default" };

        var names = Directory.GetFiles(ProfilesDir, "*.json")
            .Select(Path.GetFileNameWithoutExtension)
            .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (names.Count == 0)
            names.Add("Default");

        return names;
    }

    public void CreateProfile(string name, OverlayConfig config)
    {
        var json = JsonSerializer.Serialize(config, JsonOptions);
        File.WriteAllText(GetProfilePath(name), json);
    }

    public void RenameProfile(string oldName, string newName)
    {
        var oldPath = GetProfilePath(oldName);
        var newPath = GetProfilePath(newName);

        if (!File.Exists(oldPath)) return;
        File.Move(oldPath, newPath);

        if (string.Equals(_activeProfileName, oldName, StringComparison.OrdinalIgnoreCase))
            SetActiveProfile(newName);
    }

    public void DeleteProfile(string name)
    {
        var path = GetProfilePath(name);
        if (File.Exists(path))
            File.Delete(path);

        if (string.Equals(_activeProfileName, name, StringComparison.OrdinalIgnoreCase))
        {
            var remaining = GetProfileNames();
            SetActiveProfile(remaining.FirstOrDefault() ?? "Default");
        }
    }

    public void ExportProfile(string name, string destPath)
    {
        var sourcePath = GetProfilePath(name);
        if (File.Exists(sourcePath))
            File.Copy(sourcePath, destPath, overwrite: true);
    }

    public string ImportProfile(string sourcePath)
    {
        var baseName = Path.GetFileNameWithoutExtension(sourcePath);
        var name = baseName;
        var destPath = GetProfilePath(name);

        int counter = 1;
        while (File.Exists(destPath))
        {
            name = $"{baseName} ({counter++})";
            destPath = GetProfilePath(name);
        }

        File.Copy(sourcePath, destPath);
        return name;
    }

    public static OverlayConfig CreateDefaultConfig()
    {
        return new OverlayConfig
        {
            ShowAttempts = true,
            PrevSplits = 4,
            NextSplits = 10,
            ShowDiff = true,
            ShowPb = true,
            ShowIgt = true,
            OverlayWidth = 380,
            OverlayHeight = 500,
            ShowProgress = true,
            ShowFooterTotals = true,
            TableMode = false,
            BackgroundOpacity = 0,

            ShowTitle = false,
            TitleText = "",
            TitleColor = "#e0e0e0",
            TitleFontFamily = "Segoe UI",
            TitleFontSize = 20,

            HeaderTextColor = "#bbbbbb",
            HeaderFontFamily = "Segoe UI",
            HeaderFontSize = 13,

            AttemptsZeroColor = "#ffffff",
            AttemptsActiveColor = "#9D61A8",

            FontFamily = "Segoe UI",
            FontSize = 16,
            RowHeight = 29,
            FontBold = false,
            FontItalic = false,
            FontUnderline = false,

            GroupNameColor = "#999999",
            SplitNameColor = "#e0e0e0",
            SplitNameOnHitColor = "#e0e0e0",
            SplitNameOnHitlessColor = "#e0e0e0",

            CurrentSplitColor = "rgba(0, 204, 102, 0.06)",
            CurrentSplitHitColor = "rgba(255, 76, 76, 0.06)",
            CurrentSplitBorderColor = "#00cc66",
            CurrentSplitHitBorderColor = "#ff4c4c",
            
            ShowDpbHighlight = true,
            DpbHighlightColor = "#ffe766",

            RowClearedColor = "rgba(0, 204, 102, 0.17)",
            RowHitColor = "rgba(255, 76, 76, 0.17)",
            AlternatingRows = "rgba(255, 255, 255, 0.05)",

            HitsCurrentColor = "#888888",
            HitsActiveColor = "#c8843a",
            HitsClearedColor = "#00cc66",

            DiffPosColor = "#ff4c4c",
            DiffNegColor = "#00cc66",
            DiffZeroColor = "#bbbbbb",

            PbColor = "#bbbbbb",
            PbMatchesHit = false,

            FooterHitFontColor = "#c8843a",
            FooterHitsCurrentColor = "#888888",
            FooterPbFontColor = "#bbbbbb",
            FooterFontFamily = "Segoe UI",
            FooterFontSize = 16,
            IgtColor = "#c47fd4",
            IgtFontFamily = "Consolas",
            IgtFontSize = 17,

            ShowRunComplete = true,
            RunCompleteText = "✓ Run Complete",
            RunCompleteBannerColor = "#00cc66",

            CustomCss = "",
        };
    }

    public static void MigrateFromSettingsIfNeeded()
    {
        Directory.CreateDirectory(ProfilesDir);
        var defaultPath = Path.Combine(ProfilesDir, "Default.json");
        if (File.Exists(defaultPath)) return;

        var settingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "AutoHitCounter",
            "settings.txt");

        if (!File.Exists(settingsPath))
        {
            var config = CreateDefaultConfig();
            var json = JsonSerializer.Serialize(config, JsonOptions);
            File.WriteAllText(defaultPath, json);
            return;
        }

        try
        {
            var settingsDict = new Dictionary<string, string>();
            foreach (var line in File.ReadAllLines(settingsPath))
            {
                var parts = line.Split(new[] { '=' }, 2);
                if (parts.Length == 2)
                    settingsDict[parts[0]] = parts[1];
            }

            var overlayConfig = CreateDefaultConfig();
            var configProps = typeof(OverlayConfig).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in configProps)
            {
                if (!settingsDict.TryGetValue(prop.Name, out var value)) continue;

                object parsed = prop.PropertyType switch
                {
                    { } t when t == typeof(double) =>
                        double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var d) ? d : (object)null,
                    { } t when t == typeof(int) =>
                        int.TryParse(value, out var i) ? i : (object)null,
                    { } t when t == typeof(bool) =>
                        bool.TryParse(value, out var b) ? (object)b : null,
                    { } t when t == typeof(string) => value,
                    _ => null
                };

                if (parsed != null)
                    prop.SetValue(overlayConfig, parsed);
            }

            var json2 = JsonSerializer.Serialize(overlayConfig, JsonOptions);
            File.WriteAllText(defaultPath, json2);
        }
        catch
        {
            var config = CreateDefaultConfig();
            var json = JsonSerializer.Serialize(config, JsonOptions);
            File.WriteAllText(defaultPath, json);
        }
    }

    private static string GetProfilePath(string name)
    {
        return Path.Combine(ProfilesDir, name + ".json");
    }
}
