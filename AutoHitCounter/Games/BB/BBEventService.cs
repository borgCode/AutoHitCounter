//

using System.Collections.Generic;
using AutoHitCounter.Enums;
using AutoHitCounter.Interfaces;
using AutoHitCounter.Memory;
using AutoHitCounter.Services;
using AutoHitCounter.Utilities;
using static AutoHitCounter.Games.BB.BBCustomCodeOffsets;

namespace AutoHitCounter.Games.BB;

public class BBEventService(
    IMemoryService memoryService,
    HookManager hookManager,
    Dictionary<uint, (string Name, int Required, int Hit)> events)
    : EventServiceBase(memoryService, hookManager, events, Base + EventLogWriteIdx, Base + EventLogBuffer)
{
    public override void InstallHook()
    {
        var code = Base + EventLogCode;
        var bytes = AsmLoader.GetAsmBytes(AsmScript.BBEventLog);
        var writeIndex = Base + EventLogWriteIdx;
        var buffer = Base + EventLogBuffer;
        var hookLoc = BBOffsets.Hooks.SetEvent;
        byte[] originalBytes = [0x41, 0x89, 0xD0, 0x8B, 0x4F, 0x1C, 0x31, 0xD2, 0x89, 0xF0, 0xF7, 0xF1, 0x41, 0x89, 0xC1];

        AsmHelper.WriteRelativeOffsets(bytes, [
            (code, writeIndex, 6, 2),
            (code + 0xC, buffer, 7, 0xC + 3),
            (code + 0x2A, writeIndex, 7, 0x2A + 3)
        ]);

        AsmHelper.WriteAbsoluteAddress(bytes, hookLoc + originalBytes.Length, 0x46);

        MemoryService.WriteBytes(code, bytes);
        HookManager.InstallAbsoluteHook(code, hookLoc, originalBytes);
    }
}
