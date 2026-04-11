using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using AutoHitCounter.Models;

namespace AutoHitCounter.Utilities;

public static class ProfileMigrator
{
    private const int CurrentMigrationVersion = 3;

    private static readonly string UserProfilesPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "AutoHitCounter",
        "Profiles.json");

    public static void RunIfNeeded()
    {
        var settings = SettingsManager.Default;
        if (settings.MigrationVersion >= CurrentMigrationVersion)
            return;

        if (!File.Exists(UserProfilesPath))
        {
            settings.MigrationVersion = CurrentMigrationVersion;
            settings.Save();
            return;
        }

        try
        {
            var json = File.ReadAllText(UserProfilesPath);
            var profiles = JsonSerializer.Deserialize<Dictionary<string, List<Profile>>>(json);
            if (profiles == null)
            {
                settings.MigrationVersion = CurrentMigrationVersion;
                settings.Save();
                return;
            }

            bool changed = false;

            if (settings.MigrationVersion < CurrentMigrationVersion)
                changed |= MigrateEventIds(profiles);

            if (changed)
            {
                var dir = Path.GetDirectoryName(UserProfilesPath)!;
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                var output = JsonSerializer.Serialize(profiles, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(UserProfilesPath, output);
            }

            settings.MigrationVersion = CurrentMigrationVersion;
            settings.Save();
        }
        catch
        {
            // Don't block app startup if migration fails
        }
    }

    private static bool MigrateEventIds(Dictionary<string, List<Profile>> profiles)
    {
        // Old event ID -> correct event ID, organized by game name
        var migrations = new Dictionary<string, Dictionary<uint, uint>>
        {
            ["Dark Souls Remastered"] = new()
            {
                { 11210001, 11215015 },  // Artorias
                { 11010902, 11015375 },  // Capra Demon
                { 11215065, 11210004 },  // Kalameet  
                { 11210002, 11215025 },  // Manus
                { 11210000, 11215005 },  // Sanctuary Guardian
                { 11010901, 11015385 },  // Taurus Demon
                { 11410901, 11415384 },  // Centipede Demon
                { 11410410, 11415344 },  // Demon Firesage
            },
            ["Dark Souls 3"] = new()
            {
                { 13700800, 50002130 },  // Aldrich
                { 13200800, 50002070 },  // Ancient Wyvern
                { 15100850, 50002350 },  // Darkeater Midir
                { 15000800, 50002330 },  // Demon Prince
                { 14500860, 6323 },      // Gravetender
                { 13800800, 50002140 },  // High Lord Wolnir (original wrong ID)
                { 63800561, 50002140 },  // High Lord Wolnir (first fix, also wrong)
                { 13800830, 50002150 },  // Old Demon King
                { 14500800, 6322 },      // Sister Friede
                { 14100800, 50002200 },  // Soul of Cinder
            },
            ["Sekiro"] = new()
            {
                { 20005340, 11110410 },  // General Kuranosuke Matsumoto
                { 9302, 11020800 },          // Lady Butterfly
                { 11005637, 11020800 },          // Lady Butterfly
                { 9314, 11700850 },          // Headless Ape
                { 9307, 11700850 },          // Headless Ape
                { 9308, 1359 },          // Shinobi Owl
                { 11115850, 1359 },          // Shinobi Owl
                { 9309, 12500950 },          // True Monk
                {9300, 1027},             //Geni
                {9303, 11110800},             //Geni Castle
                {11500200, 9380},            // Mist noble
                {9313, 11110800},             //Demon of Hatred
                {11125830, 1296},             // Tomoe Geni
                {9312, 1299},             // Isshin
                {11125877, 1299}             // Isshin
                
            },
            ["Elden Ring"] = new()
            {
                { 35000800, 510250 },    // Mohg, the Omen
                { 1035500800, 510810 },  // Royal Knight Loretta
            },
        };

        bool changed = false;

        foreach (var kvp in migrations)
        {
            var gameName = kvp.Key;
            var idMap = kvp.Value;

            if (!profiles.TryGetValue(gameName, out var profileList))
                continue;

            foreach (var profile in profileList)
            {
                if (profile.Splits == null) continue;

                foreach (var split in profile.Splits)
                {
                    if (split.EventId.HasValue && idMap.TryGetValue(split.EventId.Value, out var newId))
                    {
                        split.EventId = newId;
                        changed = true;
                    }
                }
            }
        }

        return changed;
    }
}
