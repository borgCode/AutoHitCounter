// 

using System.Collections.Generic;
using AutoHitCounter.Enums;
using AutoHitCounter.Games.ER;
using AutoHitCounter.Interfaces;
using AutoHitCounter.Memory;
using AutoHitCounter.Services;
using AutoHitCounter.Utilities;
using static AutoHitCounter.Games.DS2S.DS2ScholarCustomCodeOffsets;

namespace AutoHitCounter.Games.DS2S;

public class DS2ScholarEventService(
    IMemoryService memoryService,
    HookManager hookManager,
    Dictionary<uint,string> events)
    : EventServiceBase(memoryService, hookManager, events, Base + EventLogWriteIdx, Base + EventLogBuffer)
{
    public override void InstallHook()
    {
        var code = Base + EventLogCode;
        var bytes = AsmLoader.GetAsmBytes(AsmScript.ScholarEventLog);
        var writeIndex = Base + EventLogWriteIdx;
        var buffer = Base + EventLogBuffer;
        var hookLoc = DS2ScholarOffsets.Hooks.SetEvent;
        
        AsmHelper.WriteRelativeOffsets(bytes, [
            (code + 0x3, writeIndex, 6, 0x3 + 2),
            (code + 0xE, buffer, 7, 0xE + 3),
            (code + 0x26, writeIndex, 6, 0x26 + 2),
            (code + 0x34, hookLoc + 0x5, 5, 0x34 + 1)
        ]);

        MemoryService.WriteBytes(code, bytes);
        HookManager.InstallHook(code, hookLoc, [0xB8, 0x59, 0x17, 0xB7, 0xD]);
    }
}