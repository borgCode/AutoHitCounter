// 

using System;
using System.Collections.Generic;
using AutoHitCounter.Games.EldenRing;
using AutoHitCounter.Interfaces;
using AutoHitCounter.Memory;
using AutoHitCounter.Models;

namespace AutoHitCounter.Services;

public class GameModuleFactory(IMemoryService memoryService, IStateService stateService, HookManager hookManager)
{
    public List<Game> AvailableGames => new()
    {
        new Game { GameName = "Elden Ring", ProcessName = "eldenring" },
        // new Game { GameName = "Dark Souls III", ProcessName = "DarkSoulsIII" },
    };
    
    public IGameModule CreateModule(Game game)
    {
        return game.ProcessName switch
        {
            "eldenring" => new EldenRingModule(memoryService, stateService, hookManager),
            // "darksoulsiii" => new DarkSouls3Module(_memory, _tick),
            _ => throw new NotSupportedException($"No module for {game.ProcessName}")
        };
    }
}