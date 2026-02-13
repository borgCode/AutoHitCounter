// 

using System;
using AutoHitCounter.Interfaces;
using AutoHitCounter.Memory;

namespace AutoHitCounter.Games.DS3;

public class DS3Module : IGameModule
{
    public DS3Module(IMemoryService memoryService, IStateService stateService, HookManager hookManager, ITickService tickService)
    {
        Console.WriteLine("DS3Module Ctor");
    }

    public event Action<int> OnHit;
    public event Action OnEventSet;
    public event Action<long> OnIgtChanged;
}