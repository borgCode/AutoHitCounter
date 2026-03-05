// 

using System.Collections.Generic;
using AutoHitCounter.Enums;
using AutoHitCounter.Interfaces;
using AutoHitCounter.Memory;
using AutoHitCounter.Services;
using AutoHitCounter.Utilities;
using static AutoHitCounter.Games.DS2.DS2CustomCodeOffsets;
using static AutoHitCounter.Games.DS2.DS2Offsets;

namespace AutoHitCounter.Games.DS2;

public class DS2EventService(
    IMemoryService memoryService,
    HookManager hookManager,
    Dictionary<uint, string> events)
    : EventServiceBase(memoryService, hookManager, events, Base + EventLogWriteIdx, Base + EventLogBuffer)
{
    public override void InstallHook()
    {
        if (IsScholar) InstallScholarHook();
        else InstallVanillaHook();
    }

    private void InstallScholarHook()
    {
        var code = Base + EventLogCode;
        var bytes = AsmLoader.GetAsmBytes(AsmScript.ScholarEventLog);
        var writeIndex = Base + EventLogWriteIdx;
        var buffer = Base + EventLogBuffer;
        var hookLoc = Hooks.SetEvent;

        AsmHelper.WriteRelativeOffsets(bytes, [
            (code + 0x3, writeIndex, 6, 0x3 + 2),
            (code + 0xE, buffer, 7, 0xE + 3),
            (code + 0x26, writeIndex, 6, 0x26 + 2),
            (code + 0x34, hookLoc + 0x5, 5, 0x34 + 1)
        ]);

        MemoryService.WriteBytes(code, bytes);
        HookManager.InstallHook(code, hookLoc, [0xB8, 0x59, 0x17, 0xB7, 0xD]);
    }

    private void InstallVanillaHook()
    {
        var code = Base + EventLogCode;
        var bytes = AsmLoader.GetAsmBytes(AsmScript.VanillaEventLog);
        var writeIndex = Base + EventLogWriteIdx;
        var buffer = Base + EventLogBuffer;
        var hookLoc = Hooks.SetEvent;

        AsmHelper.WriteImmediateDwords(bytes, [
            ((int)writeIndex, 0x3 + 2),
            ((int)buffer, 0xE + 2),
            ((int)writeIndex, 0x25 + 2)
        ]);

        AsmHelper.WriteRelativeOffset(bytes, code + 0x33, Hooks.SetEvent + 5, 5, +0x33 + 1);
        MemoryService.WriteBytes(code, bytes);
        HookManager.InstallHook(code, hookLoc, [0xB8, 0x59, 0x17, 0xB7, 0xD1]);
    }
}