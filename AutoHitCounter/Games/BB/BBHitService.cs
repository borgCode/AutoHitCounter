//

using AutoHitCounter.Enums;
using AutoHitCounter.Interfaces;
using AutoHitCounter.Memory;
using AutoHitCounter.Utilities;
using static AutoHitCounter.Games.BB.BBCustomCodeOffsets;
using static AutoHitCounter.Games.BB.BBOffsets;

namespace AutoHitCounter.Games.BB;

public class BBHitService(IMemoryService memoryService, HookManager hookManager) : IHitService
{
    private int _lastHitCount;

    public void InstallHooks()
    {
        WritePlayerDeadCheck();
        WriteReadThrowState();
        WriteHasItem();

        InstallProcessHitHook();
        InstallApplySpEffectAndProcessStatusHook();
        InstallKillBoxHook();
        InstallLandingHpCheckHook();
        InstallSpEffectHpCheckHook();
    }

    public bool HasHit()
    {
        var current = memoryService.Read<int>(Base + Hit);
        var newHits = current - _lastHitCount;
        _lastHitCount = current;
        return newHits > 0;
    }

    private void InstallHook(nint code, nint hookAddr, byte[] originalBytes)
    {
        hookManager.InstallAbsoluteHook(code, hookAddr, originalBytes);
    }

    private void WritePlayerDeadCheck()
    {
        var code = Base + IsPlayerDead;
        var bytes = AsmLoader.GetAsmBytes(AsmScript.BBIsPlayerDead);
        AsmHelper.WriteAbsoluteAddress(bytes, WorldChrMan.Base, 2);

        memoryService.WriteBytes(code, bytes);
    }

    private void WriteReadThrowState()
    {
        var code = Base + ReadThrowState;
        var bytes = AsmLoader.GetAsmBytes(AsmScript.BBReadThrowState);

        memoryService.WriteBytes(code, bytes);
    }

    private void WriteHasItem()
    {
        var code = Base + HasItem;
        var bytes = AsmLoader.GetAsmBytes(AsmScript.BBHasItem);
        AsmHelper.WriteAbsoluteAddresses(bytes, [
            (GameDataMan.Base, 0xA),
            (Functions.FindInventoryItem, 0x2E)
        ]);

        memoryService.WriteBytes(code, bytes);
    }

    private void InstallProcessHitHook()
    {
        var code = Base + ProcessHit;
        var hit = Base + Hit;
        var isPlayerDead = Base + IsPlayerDead;
        var readThrowState = Base + ReadThrowState;
        var hasItem = Base + HasItem;
        byte[] originalBytes = [0x4C, 0x89, 0xF7, 0x4C, 0x89, 0xFE, 0x48, 0x89, 0xDA, 0xE8, 0xB6, 0x00, 0x00, 0x00];

        var bytes = AsmLoader.GetAsmBytes(AsmScript.BBProcessHit);

        AsmHelper.WriteAbsoluteAddresses(bytes, [
            (WorldChrMan.Base, 0xC + 2),
            (Functions.ProcessHit, 0x9C + 2),
            (Hooks.ProcessHit + originalBytes.Length, 0x11E)
        ]);

        AsmHelper.WriteRelativeOffsets(bytes, [
            (code + 0x2F, isPlayerDead, 5, 0x2F + 1),
            (code + 0x6A, readThrowState, 5, 0x6A + 1),
            (code + 0xB2, readThrowState, 5, 0xB2 + 1),
            (code + 0xDC, hasItem, 5, 0xDC + 1),
            (code + 0x10E, hit, 6, 0x10E + 2)
        ]);

        memoryService.WriteBytes(code, bytes);
        InstallHook(code, Hooks.ProcessHit, originalBytes);
    }

    private void InstallApplySpEffectAndProcessStatusHook()
    {
        var code = Base + ApplySpEffectAndProcessStatus;
        var hit = Base + Hit;
        var isPlayerDead = Base + IsPlayerDead;
        var readThrowState = Base + ReadThrowState;
        byte[] originalBytes =
            [0x55, 0x48, 0x89, 0xE5, 0x41, 0x57, 0x41, 0x56, 0x41, 0x55, 0x41, 0x54, 0x53, 0x48, 0x83, 0xEC, 0x48];

        var bytes = AsmLoader.GetAsmBytes(AsmScript.BBApplySpEffectAndProcessStatus);

        AsmHelper.WriteAbsoluteAddresses(bytes, [
            (WorldChrMan.Base, 0x2),
            (Hooks.ApplySpEffectAndProcessStatus + originalBytes.Length, 0xDF)
        ]);

        AsmHelper.WriteRelativeOffsets(bytes, [
            (code + 0x32, isPlayerDead, 5, 0x32 + 1),
            (code + 0x40, readThrowState, 5, 0x40 + 1),
            (code + 0x66, hit, 6, 0x66 + 2),
            (code + 0x8F, readThrowState, 5, 0x8F + 1),
            (code + 0xB8, hit, 6, 0xB8 + 2)
        ]);

        memoryService.WriteBytes(code, bytes);
        InstallHook(code, Hooks.ApplySpEffectAndProcessStatus, originalBytes);
    }

    private void InstallKillBoxHook()
    {
        var code = Base + KillBox;
        var hit = Base + Hit;
        var isPlayerDead = Base + IsPlayerDead;
        byte[] originalBytes = [0x55, 0x48, 0x89, 0xE5, 0x41, 0x57, 0x41, 0x56, 0x53, 0x50, 0x48, 0x89, 0xFB, 0x48, 0x85, 0xF6];

        var bytes = AsmLoader.GetAsmBytes(AsmScript.BBKillBox);

        AsmHelper.WriteAbsoluteAddresses(bytes, [
            (WorldChrMan.Base, 0x2),
            (Hooks.KillBox + originalBytes.Length, 0x7A)
        ]);

        AsmHelper.WriteRelativeOffsets(bytes, [
            (code + 0x18, isPlayerDead, 5, 0x18 + 1),
            (code + 0x5E, hit, 6, 0x5E + 2)
        ]);

        memoryService.WriteBytes(code, bytes);
        InstallHook(code, Hooks.KillBox, originalBytes);
    }

    private void InstallLandingHpCheckHook()
    {
        var code = Base + LandingHpCheck;
        var hit = Base + Hit;
        byte[] originalBytes = [0xC5, 0xFA, 0x11, 0x55, 0xB4, 0x89, 0x4D, 0xA4, 0x29, 0xD3, 0x85, 0xDB, 0x7F, 0x27];

        var bytes = AsmLoader.GetAsmBytes(AsmScript.BBLandingHpCheck);

        AsmHelper.WriteAbsoluteAddresses(bytes, [
            (WorldChrMan.Base, 0xE + 2),
            (Hooks.LandingHpCheck + originalBytes.Length, 0x37),
            (Hooks.LandingHpCheck + 0x35, 0x45)
        ]);

        AsmHelper.WriteRelativeOffset(bytes, code + 0x2B, hit, 6, 0x2B + 2);

        memoryService.WriteBytes(code, bytes);
        InstallHook(code, Hooks.LandingHpCheck, originalBytes);
    }

    private void InstallSpEffectHpCheckHook()
    {
        var code = Base + SpEffectHpCheck;
        var hit = Base + Hit;
        byte[] originalBytes =
            [0x44, 0x8B, 0xAB, 0xF8, 0x00, 0x00, 0x00, 0x45, 0x01, 0xF5, 0x45, 0x85, 0xED, 0x7E, 0x07];

        var bytes = AsmLoader.GetAsmBytes(AsmScript.BBSpEffectHpCheck);

        AsmHelper.WriteAbsoluteAddresses(bytes, [
            (WorldChrMan.Base, 0x1C + 2),
            (Hooks.SpEffectHpCheck + 0x16, 0x46),
            (Hooks.SpEffectHpCheck + originalBytes.Length, 0x54)
        ]);

        AsmHelper.WriteRelativeOffset(bytes, code + 0x3A, hit, 6, 0x3A + 2);

        memoryService.WriteBytes(code, bytes);
        InstallHook(code, Hooks.SpEffectHpCheck, originalBytes);
    }
}
