using System.Collections.Generic;
using AutoHitCounter.Enums;

namespace AutoHitCounter.Services;

public static class GameFlagRegistry
{
    private static readonly Dictionary<GameTitle, IReadOnlyList<(string Key, string DisplayName)>> _flags = new()
    {
        [GameTitle.DarkSoulsRemastered] = [],
        [GameTitle.DarkSouls2] =
        [
            ("ignore_shulva_spikes", "Ignore Shulva Spikes"),
        ],
        [GameTitle.DarkSouls3] = [],
        [GameTitle.Sekiro] =
        [
            ("should_count_roberto", "Count Roberto stagger")
        ],
        [GameTitle.EldenRing] = [],
        [GameTitle.Nightreign] = [],
    };

    public static IReadOnlyList<(string Key, string DisplayName)> GetFlags(GameTitle title)
        => _flags.TryGetValue(title, out var flags) ? flags : [];
}