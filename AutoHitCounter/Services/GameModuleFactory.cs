// 

using System;
using System.Collections.Generic;
using AutoHitCounter.Games.DS2;
using AutoHitCounter.Games.DS2S;
using AutoHitCounter.Games.DS3;
using AutoHitCounter.Games.DSR;
using AutoHitCounter.Games.ER;
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
    
    private readonly Dictionary<uint, string> _eldenRingEvents = EventLoader.GetEvents("EldenRingEvents");
    
    public IGameModule CreateModule(Game game)
    {
        return game.GameName switch
        {
            "Dark Souls Remastered" => new DSRModule(memoryService, stateService, hookManager, tickService),
            "Dark Souls 2 Vanilla" => new DS2VanillaModule(memoryService, stateService, hookManager, tickService),
            "Dark Souls 2 Scholar" => new DS2ScholarModule(memoryService, stateService, hookManager, tickService),
            "Dark Souls 3" => new DS3Module(memoryService, stateService, hookManager, tickService),
            "Sekiro" => new SKModule(memoryService, stateService, hookManager, tickService),
            "Elden Ring" => new EldenRingModule(memoryService, stateService, hookManager, tickService, _eldenRingEvents),
            _ => throw new NotSupportedException($"No module for {game.ProcessName}")
        };
    }
}