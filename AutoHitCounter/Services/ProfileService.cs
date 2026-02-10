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

    private static readonly string ProfilesPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "AutoHitCounter",
        "Profiles.json");

    public ProfileService()
    {
        _profiles = LoadProfiles();
    }

    public List<Profile> GetProfiles(string gameName)
    {
        return _profiles.TryGetValue(gameName, out var profiles)
            ? profiles
            : [];
    }

    public void SaveProfile(Profile profile)
    {
        if (!_profiles.ContainsKey(profile.GameName))
            _profiles[profile.GameName] = [];

        var list = _profiles[profile.GameName];
        var existing = list.FindIndex(p => p.Name == profile.Name);

        if (existing >= 0)
            list[existing] = profile;
        else
            list.Add(profile);

        WriteToDisk();
    }

    public void DeleteProfile(string gameName, string profileName)
    {
        if (!_profiles.TryGetValue(gameName, out var list)) return;
        list.RemoveAll(p => p.Name == profileName);
        WriteToDisk();
    }

    private Dictionary<string, List<Profile>> LoadProfiles()
    {
        try
        {
            if (!File.Exists(ProfilesPath))
                return new Dictionary<string, List<Profile>>();

            var json = File.ReadAllText(ProfilesPath);
            return JsonSerializer.Deserialize<Dictionary<string, List<Profile>>>(json)
                   ?? new Dictionary<string, List<Profile>>();
        }
        catch
        {
            return new Dictionary<string, List<Profile>>();
        }
    }

    private void WriteToDisk()
    {
        var dir = Path.GetDirectoryName(ProfilesPath)!;
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        var json = JsonSerializer.Serialize(_profiles, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(ProfilesPath, json);
    }
}