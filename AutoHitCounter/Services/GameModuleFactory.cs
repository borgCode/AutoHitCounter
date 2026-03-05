// 

using System;
using System.Collections.Generic;
using AutoHitCounter.Enums;
using AutoHitCounter.Games.DS2;
using AutoHitCounter.Games.DS3;
using AutoHitCounter.Games.DSR;
using AutoHitCounter.Games.ER;
using AutoHitCounter.Games.SK;
using AutoHitCounter.Interfaces;
using AutoHitCounter.Memory;
using AutoHitCounter.Models;

namespace AutoHitCounter.Services;

public class GameModuleFactory(
    IMemoryService memoryService,
    IStateService stateService,
    HookManager hookManager,
    ITickService tickService)
{
    
    public IGameModule CreateModule(Game game, Dictionary<uint, string> events, IGameSettingsProvider settings)
    {
        return game.Title switch
        {
            GameTitle.DarkSoulsRemastered => new DSRModule(memoryService, stateService, hookManager, tickService, events),
            GameTitle.DarkSouls2          => new DS2Module(memoryService, stateService, hookManager, tickService, events, settings),
            GameTitle.DarkSouls3          => new DS3Module(memoryService, stateService, hookManager, tickService, events),
            GameTitle.Sekiro              => new SKModule(memoryService, stateService, hookManager, tickService, events, settings),
            GameTitle.EldenRing           => new EldenRingModule(memoryService, stateService, hookManager, tickService, events),
            _ => throw new NotSupportedException($"No module for {game.ProcessName}")
        };
    }
}