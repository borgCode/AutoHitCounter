// 

using System.Collections.Generic;
using AutoHitCounter.Enums;
using AutoHitCounter.Interfaces;
using AutoHitCounter.Memory;
using AutoHitCounter.Services;
using AutoHitCounter.Utilities;

namespace AutoHitCounter.Games.ER;

public class EldenRingEventService(
    IMemoryService memoryService,
    HookManager hookManager,
    Dictionary<uint, string> eldenRingEvents)
    : EventServiceBase(memoryService, hookManager, eldenRingEvents)
{
    public override void InstallHook()
    {
        var code = EldenRingCustomCodeOffsets.Base + EldenRingCustomCodeOffsets.EventLogCode;
        var bytes = AsmLoader.GetAsmBytes(AsmScript.EldenRingEventLog);
        var writeIndex = EldenRingCustomCodeOffsets.Base + EldenRingCustomCodeOffsets.EventLogWriteIdx;
        var buffer = EldenRingCustomCodeOffsets.Base + EldenRingCustomCodeOffsets.EventLogBuffer;
        var hookLoc = EldenRingOffsets.Hooks.SetEvent;

        AsmHelper.WriteRelativeOffsets(bytes, [
            (code + 0x8, writeIndex, 6, 0x8 + 2),
            (code + 0x13, buffer, 7, 0x13 + 3),
            (code + 0x2B, writeIndex, 6, 0x2B + 2),
            (code + 0x34, hookLoc + 0x5, 5, 0x34 + 1)
        ]);

        MemoryService.WriteBytes(code, bytes);
        HookManager.InstallHook(code, hookLoc, [0x48, 0x89, 0x5C, 0x24, 0x08]);
    }
}