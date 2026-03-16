// 

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using AutoHitCounter.Interfaces;
using AutoHitCounter.Models;

namespace AutoHitCounter.Services;

public class ProfileService : IProfileService
{
    private readonly Dictionary<string, List<Profile>> _profiles;

    private static readonly string UserProfilesPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "AutoHitCounter",
        "Profiles.json");

    private static readonly string DefaultProfilesPath = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory,
        "DefaultProfiles.json");

    public ProfileService()
    {
        _profiles = LoadMergedProfiles();
    }

    public List<Profile> GetProfiles(string gameName)
    {
        return _profiles.TryGetValue(gameName, out var profiles)
            ? profiles
            : [];
    }

    public void SaveProfile(Profile profile)
    {
        if (profile?.GameName == null) return;
        if (!_profiles.ContainsKey(profile.GameName))
            _profiles[profile.GameName] = [];

        var list = _profiles[profile.GameName];
        var existing = list.FindIndex(p => p.Name == profile.Name);

        if (existing >= 0)
            list[existing] = profile;
        else
            list.Add(profile);

        WriteUserProfiles();
    }

    public void DeleteProfile(string gameName, string profileName)
    {
        if (!_profiles.TryGetValue(gameName, out var list)) return;
        list.RemoveAll(p => p.Name == profileName);
        WriteUserProfiles();
    }

    private Dictionary<string, List<Profile>> LoadMergedProfiles()
    {
        var defaults = LoadFromDisk(DefaultProfilesPath);
        var user = LoadFromDisk(UserProfilesPath);
        
        var merged = new Dictionary<string, List<Profile>>();

        foreach (var kvp in defaults)
            merged[kvp.Key] = new List<Profile>(kvp.Value);

        foreach (var kvp in user)
        {
            if (!merged.TryGetValue(kvp.Key, out var mergedList))
            {
                merged[kvp.Key] = new List<Profile>(kvp.Value);
                continue;
            }

            foreach (var userProfile in kvp.Value)
            {
                var existingIndex = mergedList.FindIndex(p => p.Name == userProfile.Name);

                if (existingIndex >= 0)
                    mergedList[existingIndex] = userProfile;
                else
                    mergedList.Add(userProfile);
            }
        }

        return merged;
    }

    private static Dictionary<string, List<Profile>> LoadFromDisk(string path)
    {
        try
        {
            if (!File.Exists(path))
                return new Dictionary<string, List<Profile>>();

            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<Dictionary<string, List<Profile>>>(json)
                   ?? new Dictionary<string, List<Profile>>();
        }
        catch
        {
            return new Dictionary<string, List<Profile>>();
        }
    }

    private void WriteUserProfiles()
    {
        var dir = Path.GetDirectoryName(UserProfilesPath)!;
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        var json = JsonSerializer.Serialize(_profiles, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(UserProfilesPath, json);
    }
}