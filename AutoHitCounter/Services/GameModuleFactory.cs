// 

using System.Collections.Generic;
using AutoHitCounter.Games.EldenRing;
using AutoHitCounter.Interfaces;
using AutoHitCounter.Memory;

namespace AutoHitCounter.Services;

public class GameModuleFactory(IMemoryService memoryService, IStateService stateService, HookManager hookManager)
{
    private readonly List<IGameModule> _modules = new()
    {
        new EldenRingModule(memoryService, stateService, hookManager),
    };
}