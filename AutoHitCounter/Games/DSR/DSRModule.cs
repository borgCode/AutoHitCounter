// 

using System;
using AutoHitCounter.Interfaces;
using AutoHitCounter.Memory;

namespace AutoHitCounter.Games.DSR;

public class DSRModule : IGameModule
{
    public DSRModule(IMemoryService memoryService, IStateService stateService, HookManager hookManager, ITickService tickService)
    {
      
    }

    public event Action<int> OnHit;
    public event Action OnEventSet;
}