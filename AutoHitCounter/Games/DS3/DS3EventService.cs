// 

using System.Collections.Generic;
using AutoHitCounter.Interfaces;
using AutoHitCounter.Memory;
using AutoHitCounter.Services;

namespace AutoHitCounter.Games.DS3;

public class DS3EventService(IMemoryService memoryService, HookManager hookManager, Dictionary<uint, string> events)
    : EventServiceBase(memoryService, hookManager, events)
{
    public override void InstallHook()
    {
        throw new System.NotImplementedException();
    }
}