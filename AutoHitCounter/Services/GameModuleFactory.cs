//

using System;
using System.Collections.Generic;
using System.Linq;
using AutoHitCounter.Enums;
using AutoHitCounter.Games.DS2;
using AutoHitCounter.Games.DS3;
using AutoHitCounter.Games.DSR;
using AutoHitCounter.Games.ER;
using AutoHitCounter.Games.Manual;
using AutoHitCounter.Games.SK;
using AutoHitCounter.Interfaces;
using AutoHitCounter.Memory;
using AutoHitCounter.Models;
using AutoHitCounter.Utilities;

namespace AutoHitCounter.Services;

public class GameModuleFactory(
    IMemoryService memoryService,
    IStateService stateService,
    HookManager hookManager,
    ITickService tickService)
{
    private class GameRegistration(string processName, string eventResource, bool isEventLogSupported)
    {
        public string ProcessName { get; } = processName;
        public string EventResource { get; } = eventResource;
        public bool IsEventLogSupported { get; } = isEventLogSupported;
    }

    private static readonly Dictionary<GameTitle, GameRegistration> Registrations = new()
    {
        [GameTitle.DarkSoulsRemastered] = new("darksoulsremastered", "DSREvents", true),
        [GameTitle.DarkSouls2]          = new("darksoulsii", "DS2Events", true),
        [GameTitle.DarkSouls3]          = new("darksoulsiii", "DS3Events", true),
        [GameTitle.Sekiro]              = new("sekiro", "SKEvents", true),
        [GameTitle.EldenRing]           = new("eldenring", "EldenRingEvents", true),
    };

    public List<Game> GetRegisteredGames() =>
        Registrations.Select(r => new Game
        {
            Title = r.Key,
            GameName = r.Key.GetDescription(),
            ProcessName = r.Value.ProcessName,
            IsEventLogSupported = r.Value.IsEventLogSupported
        }).ToList();

    public Dictionary<uint, string> GetEventsForGame(GameTitle title) =>
        title == GameTitle.Manual ? new()
        : Registrations.TryGetValue(title, out var reg) && reg.EventResource != null
            ? EventLoader.GetEvents(reg.EventResource)
            : new();

    public IGameModule CreateModule(Game game, Dictionary<uint, (string Name, int Required, int Hit)> events, IHitRulesProvider rules)
    {
        if (game.IsManual)
            return new ManualGameModule();

        return game.Title switch
        {
            GameTitle.DarkSoulsRemastered => new DSRModule(memoryService, stateService, hookManager, tickService, events),
            GameTitle.DarkSouls2          => new DS2Module(memoryService, stateService, hookManager, tickService, events, rules),
            GameTitle.DarkSouls3          => new DS3Module(memoryService, stateService, hookManager, tickService, events),
            GameTitle.Sekiro              => new SKModule(memoryService, stateService, hookManager, tickService, events, rules),
            GameTitle.EldenRing           => new EldenRingModule(memoryService, stateService, hookManager, tickService, events),
            _ => throw new NotSupportedException($"No module for {game.ProcessName}")
        };
    }
}
