//

using System.Collections.Generic;
using System.Linq;
using AutoHitCounter.Enums;
using AutoHitCounter.Interfaces;
using AutoHitCounter.Models;

namespace AutoHitCounter.Services;

public class CustomGameService(ICustomGamesStore store, IProfileService profileService, RunStateService runStateService)
{
    public IReadOnlyList<Game> Load()
    {
        var games = new List<Game>();
        var raw = store.Raw;
        if (string.IsNullOrWhiteSpace(raw)) return games;

        foreach (var name in raw.Split(','))
        {
            var trimmed = name.Trim();
            if (string.IsNullOrEmpty(trimmed)) continue;
            games.Add(CreateGame(trimmed));
        }

        return games;
    }

    public Game Add(string name)
    {
        var existing = store.Raw;
        store.Raw = string.IsNullOrEmpty(existing) ? name : $"{existing},{name}";
        store.Save();
        return CreateGame(name);
    }

    public void Delete(string name)
    {
        foreach (var profile in profileService.GetProfiles(name).ToList())
            profileService.DeleteProfile(name, profile.Name);

        var remaining = (store.Raw ?? "")
            .Split(',')
            .Select(n => n.Trim())
            .Where(n => n != name);
        store.Raw = string.Join(",", remaining);
        store.Save();

        runStateService.InvalidateStale(name, []);
    }

    public void Rename(string oldName, string newName)
    {
        var names = (store.Raw ?? "")
            .Split(',')
            .Select(n => n.Trim())
            .Select(n => n == oldName ? newName : n);
        store.Raw = string.Join(",", names);
        store.Save();

        profileService.RenameGame(oldName, newName);
        runStateService.RenameGame(oldName, newName);
    }

    public static bool IsValidName(string name) =>
        !string.IsNullOrWhiteSpace(name) && !name.Contains(',');

    private static Game CreateGame(string name) => new()
    {
        Title = GameTitle.Manual,
        GameName = name,
        ProcessName = null,
        IsManual = true
    };
}
