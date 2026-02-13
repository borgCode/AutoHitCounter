// 

using System;
using AutoHitCounter.Interfaces;
using AutoHitCounter.Memory;

namespace AutoHitCounter.Games.DS2;

public class DS2VanillaModule : IGameModule
{
    public DS2VanillaModule(IMemoryService memoryService, IStateService stateService, HookManager hookManager, ITickService tickService)
    {
        Console.WriteLine("DS2VanillaModule Ctor");
    }

    public event Action<int> OnHit;
    public event Action OnEventSet;
    public event Action<long> OnIgtChanged;
}