// 

using System;
using AutoHitCounter.Interfaces;
using AutoHitCounter.Memory;

namespace AutoHitCounter.Games.SK;

public class SKModule : IGameModule
{
    public SKModule(IMemoryService memoryService, IStateService stateService, HookManager hookManager, ITickService tickService)
    {
   
    }

    public event Action<int> OnHit;
    public event Action OnEventSet;
}