// 

using System.Collections.Generic;
using AutoHitCounter.Interfaces;
using AutoHitCounter.Memory;
using AutoHitCounter.Services;
using static AutoHitCounter.Games.SK.SKCustomCodeOffsets;

namespace AutoHitCounter.Games.SK;

public class SKEventService(IMemoryService memoryService, HookManager hookManager, Dictionary<uint, string> events) 
    : EventServiceBase(memoryService, hookManager, events, Base + EventLogWriteIdx, Base + EventLogBuffer)
{
    
    public override void InstallHook()
    {
        throw new System.NotImplementedException();
    }
    
}