//

using System.Collections.Generic;
using AutoHitCounter.Enums;
using AutoHitCounter.Interfaces;
using AutoHitCounter.Memory;
using AutoHitCounter.Services;
using AutoHitCounter.Utilities;
using static AutoHitCounter.Games.BB.BBCustomCodeOffsets;

namespace AutoHitCounter.Games.BB;

public class BBEmevdEventService(
    IMemoryService memoryService,
    HookManager hookManager,
    Dictionary<uint, (string Name, int Required, int Hit)> events)
    : EventServiceBase(memoryService, hookManager, events, Base + EmevdEventLogWriteIdx, Base + EmevdEventLogBuffer)
{
    public override void InstallHook()
    {
        var code = Base + EmevdEventLogCode;
        var bytes = AsmLoader.GetAsmBytes(AsmScript.BBEmevdEventLog);
        var writeIndex = Base + EmevdEventLogWriteIdx;
        var buffer = Base + EmevdEventLogBuffer;
        var hookLoc = BBOffsets.Hooks.EmevdSetEvent;
        byte[] originalBytes =
            [0x44, 0x89, 0xF0, 0xC1, 0xE8, 0x03, 0x41, 0xF7, 0xD6, 0x41, 0x80, 0xE6, 0x07, 0xBE, 0x01, 0x00, 0x00, 0x00];

        AsmHelper.WriteRelativeOffsets(bytes, [
            (code + 0x4, writeIndex, 6, 0x4 + 2),
            (code + 0xE, buffer, 7, 0xE + 3),
            (code + 0x27, writeIndex, 6, 0x27 + 2)
        ]);

        AsmHelper.WriteAbsoluteAddress(bytes, hookLoc + originalBytes.Length, 0x45);

        MemoryService.WriteBytes(code, bytes);
        HookManager.InstallAbsoluteHook(code, hookLoc, originalBytes);
    }
}
