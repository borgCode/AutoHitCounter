// 

using System.Collections.Generic;
using System.Linq;
using AutoHitCounter.Enums;
using AutoHitCounter.Interfaces;
using AutoHitCounter.Memory;
using AutoHitCounter.Utilities;
using static AutoHitCounter.Games.NR.NRCustomCodeOffsets;
using static AutoHitCounter.Games.NR.NROffsets;

namespace AutoHitCounter.Games.NR;

public class NRHitService(IMemoryService memoryService, HookManager hookManager) : IHitService
{
    private int _lastHitCount;

    private readonly List<nint> _hooks = [];

    public void InstallHooks()
    {
        WritePlayerDeadCheck();

        InstallProcessHitHook();
        InstallSetHealthHook();
        InstallApplySpEffectAndProcessStatusHook();
        InstallClearThrowStateHook();
        InstallCalcSpEffectMultipliersHook();
    }

    public bool HasHit()
    {
        var current = memoryService.Read<int>(Base + Hit);
        var newHits = current - _lastHitCount;
        _lastHitCount = current;
        return newHits > 0;
    }

    public void EnsureHooksInstalled()
    {
        if (_hooks.Any(c => !hookManager.IsHookInstalled(c)))
            InstallHooks();
    }

    public void ResetFlags()
    {
        memoryService.Write(Base + InThrowFlag, false);
    }

    private void InstallHook(nint code, nint hookAddr, byte[] originalBytes)
    {
        hookManager.InstallHook(code, hookAddr, originalBytes);
        TrackHook(code);
    }

    private void InstallCallHook(nint code, nint hookAddr, byte[] originalBytes)
    {
        hookManager.InstallCallHook(code, hookAddr, originalBytes);
        TrackHook(code);
    }

    private void TrackHook(nint code)
    {
        if (!_hooks.Contains(code))
            _hooks.Add(code);
    }

    private void WritePlayerDeadCheck()
    {
        var code = Base + IsPlayerDead;
        var bytes = AsmLoader.GetAsmBytes(AsmScript.NRIsPlayerDead);
        AsmHelper.WriteRelativeOffset(bytes, code, WorldChrMan.Base, 7, 3);
        AsmHelper.WriteImmediateDwords(bytes, [
                (WorldChrMan.PlayerIns, 0xC + 3),
                (WorldChrMan.ChrIns.Modules, 0x18 + 3),
                (WorldChrMan.ChrIns.Health, 0x2C + 2)
            ]
        );

        memoryService.WriteBytes(code, bytes);
    }

    private void InstallProcessHitHook()
    {
        var code = Base + ProcessHit;
        var inThrow = Base + InThrowFlag;
        var spEffectDamageNegatedFlag = Base + SpEffectDamageNegatedFlag;
        var hit = Base + Hit;
        var isPlayerDead = Base + IsPlayerDead;

        var bytes = AsmLoader.GetAsmBytes(AsmScript.NRProcessHit);

        AsmHelper.WriteRelativeOffsets(bytes, [
            (code, spEffectDamageNegatedFlag, 7, 2),
            (code + 0x7, inThrow, 7, 0x7 + 2),
            (code + 0x14, isPlayerDead, 5, 0x14 + 1),
            (code + 0x1F, WorldChrMan.Base, 7, 0x1F + 3),
            (code + 0xAD, Functions.ProcessHitOriginal, 5, 0xAD + 1),
            (code + 0xCB, inThrow, 7, 0xCB + 2),
            (code + 0xF8, spEffectDamageNegatedFlag, 7, 0xF8 + 2),
            (code + 0x107, hit, 6, 0x107 + 2),
            (code + 0x112, Functions.ProcessHitOriginal, 5, 0x112 + 1),
        ]);

        AsmHelper.WriteImmediateDwords(bytes, [
                (WorldChrMan.PlayerIns, 0x26 + 3),
            ]
        );

        memoryService.WriteBytes(code, bytes);

        byte[] originalBytes = [0xE8, 0x00, 0x00, 0x00, 0x00];
        AsmHelper.WriteRelativeOffset(originalBytes, Hooks.ProcessHit, Functions.ProcessHitOriginal, 5, 1);
        InstallCallHook(code, Hooks.ProcessHit, originalBytes);
    }

    private void InstallSetHealthHook()
    {
        var code = Base + SetHealth;
        var hit = Base + Hit;
        var isPlayerDead = Base + IsPlayerDead;

        var bytes = AsmLoader.GetAsmBytes(AsmScript.NRSetHealth);

        AsmHelper.WriteRelativeOffsets(bytes, [
            (code, isPlayerDead, 5, 1),
            (code + 0x14, WorldChrMan.Base, 7, 0x14 + 3),
            (code + 0x4D, hit, 6, 0x4D + 2),
            (code + 0x58, Hooks.SetHealth + 5, 5, 0x58 + 1),
        ]);

        AsmHelper.WriteImmediateDwords(bytes, [
            (WorldChrMan.PlayerIns, 0x20 + 3),
            (WorldChrMan.ChrIns.Modules, 0x2C + 3)
        ]);

        memoryService.WriteBytes(code, bytes);
        InstallHook(code, Hooks.SetHealth, [0x48, 0x89, 0x5C, 0x24, 0x08]);
    }

    private void InstallApplySpEffectAndProcessStatusHook()
    {
        var code = Base + ApplySpEffectAndProcessStatus;
        var hit = Base + Hit;
        var isPlayerDead = Base + IsPlayerDead;

        var bytes = AsmLoader.GetAsmBytes(AsmScript.NRApplySpEffectAndProcessStatus);

        AsmHelper.WriteRelativeOffsets(bytes, [
            (code + 0x9, isPlayerDead, 5, 0x9 + 1),
            (code + 0x14, WorldChrMan.Base, 7, 0x14 + 3),
            (code + 0xF2, Functions.ApplySpEffectAndProcessStatusOriginal, 5, 0xF2 + 1),
            (code + 0x118, hit, 6, 0x118 + 2),
            (code + 0x128, hit, 6, 0x128 + 2),
            (code + 0x12E, Functions.ApplySpEffectAndProcessStatusOriginal, 5, 0x12E + 1),
        ]);

        AsmHelper.WriteImmediateDwords(bytes, [
            (WorldChrMan.PlayerIns, 0x24 + 3),
            (WorldChrMan.ChrIns.Modules, 0x101 + 3)
        ]);
        
        memoryService.WriteBytes(code, bytes);

        byte[] originalBytes = [0xE8, 0x00, 0x00, 0x00, 0x00];
        AsmHelper.WriteRelativeOffset(originalBytes, Hooks.ApplySpEffectAndProcessStatus, Functions.ApplySpEffectAndProcessStatusOriginal, 5, 1);
        InstallCallHook(code, Hooks.ApplySpEffectAndProcessStatus, originalBytes);
    }

    private void InstallClearThrowStateHook()
    {
        var code = Base + ClearThrowState;
        var inThrow = Base + InThrowFlag;
        
        var bytes = AsmLoader.GetAsmBytes(AsmScript.NRClearThrowState);
        
        AsmHelper.WriteRelativeOffsets(bytes, [
            (code, WorldChrMan.Base, 7, 3),
            (code + 0x19, inThrow, 7, 0x19 + 2),
            (code + 0x25, Hooks.ClearThrowState + 6, 5, 0x25 + 1),
        ]);
        
        AsmHelper.WriteImmediateDword(bytes, WorldChrMan.PlayerIns, 0xC + 3);
        
        memoryService.WriteBytes(code, bytes);
        InstallHook(code, Hooks.ClearThrowState, [0x40, 0x53, 0x48, 0x83, 0xEC, 0x30]);
    }

    private void InstallCalcSpEffectMultipliersHook()
    {
        var code = Base + CalcSpEffectMultipliers;
        var spEffectDamageNegatedFlag = Base + SpEffectDamageNegatedFlag;
        
        var bytes = AsmLoader.GetAsmBytes(AsmScript.NRCalcSpEffectMultipliers);
        
        AsmHelper.WriteRelativeOffsets(bytes, [
            (code + 0xE, Functions.CalcSpEffectMultipliersOriginal, 5, 0xE + 1),
            (code + 0x17, WorldChrMan.Base, 7, 0x17 + 3),
            (code + 0xD2, spEffectDamageNegatedFlag, 7, 0xD2 + 2)
        ]);
        
        AsmHelper.WriteImmediateDword(bytes, WorldChrMan.PlayerIns, 0x27 + 3);
        
        memoryService.WriteBytes(code, bytes);

        byte[] originalBytes = [0xE8, 0x00, 0x00, 0x00, 0x00];
        AsmHelper.WriteRelativeOffset(originalBytes, Hooks.CalcSpEffectMultipliers, Functions.CalcSpEffectMultipliersOriginal, 5, 1);
        InstallCallHook(code, Hooks.CalcSpEffectMultipliers, originalBytes);
    }
}