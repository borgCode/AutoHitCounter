// 

using System;
using AutoHitCounter.Interfaces;
using AutoHitCounter.Memory;

namespace AutoHitCounter.Games.DS2S;

public class DS2ScholarModule : IGameModule
{
    public DS2ScholarModule(IMemoryService memoryService, IStateService stateService, HookManager hookManager, ITickService tickService)
    {
   
    }

    public event Action<int> OnHit;
    public event Action OnEventSet;
}