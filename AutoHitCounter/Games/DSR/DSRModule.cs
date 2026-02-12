// 

using System;
using AutoHitCounter.Interfaces;
using AutoHitCounter.Memory;

namespace AutoHitCounter.Games.DSR;

public class DSRModule : IGameModule
{
    public DSRModule(IMemoryService memoryService, IStateService stateService, HookManager hookManager, ITickService tickService)
    {
      Console.WriteLine("DSRModule ctor");
    }

    public event Action<int> OnHit;
    public event Action OnEventSet;
    public event Action<long> OnIgtChanged;
}