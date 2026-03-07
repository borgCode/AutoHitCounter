// 

using System.Linq;
using AutoHitCounter.Enums;
using AutoHitCounter.Interfaces;
using AutoHitCounter.Memory;
using AutoHitCounter.Utilities;
using static AutoHitCounter.Games.DS3.DS3CustomCodeOffsets;
using static AutoHitCounter.Games.DS3.DS3Offsets;

namespace AutoHitCounter.Games.DS3;

public class DS3HitService(IMemoryService memoryService, HookManager hookManager) : IHitService
{
    private int _lastHitCount;

    private const string Kernel32 = "kernel32.dll";
    private const string GetTickCount64 = "GetTickCount64";

    public void InstallHooks()
    {
        WritePlayerDeadCheck();
        WriteGetPlayerSpEffect();

        InstallHitHook();
        InstallLethalFallHook();
        InstallAuxHitHooks();
        InstallJailerDrainHook();
        InstallApplyHealthDeltaHook();
        InstallKillBoxHook();
        InstallCheckStaggerIgnore();
    }

    public bool HasHit()
    {
        var current = memoryService.Read<int>(Base + Hit);
        var newHits = current - _lastHitCount;
        _lastHitCount = current;
        return newHits > 0;
    }

    // Needed because arxan can restore any hook site

    public void EnsureHooksInstalled()
    {
        nint[] hooks = [Hooks.Hit, Hooks.LethalFall, Hooks.CheckAuxAttacker,
            Hooks.AuxProc, Hooks.HasJailerDrain, Hooks.ApplyHealthDelta,
            Hooks.KillBox, Hooks.CheckStaggerIgnore];
        if (hooks.Any(h => memoryService.Read<byte>(h) != 0xE9))
            InstallHooks();
    }

    private void WritePlayerDeadCheck()
    {
        var code = Base + CheckPlayerDead;

        var bytes = AsmLoader.GetAsmBytes(AsmScript.DS3CheckPlayerDead);
        AsmHelper.WriteRelativeOffset(bytes, code, WorldChrMan.Base, 7, 3);
        memoryService.WriteBytes(code, bytes);
    }

    private void WriteGetPlayerSpEffect()
    {
        var code = Base + GetSpEffect;

        var bytes = AsmLoader.GetAsmBytes(AsmScript.DS3GetSpEffect);
        AsmHelper.WriteRelativeOffset(bytes, code, WorldChrMan.Base, 7, 3);
        memoryService.WriteBytes(code, bytes);
    }

    private void InstallHitHook()
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.DS3Hit);
        var hit = Base + Hit;
        var staggerCheckFlag = Base + CheckStaggerFlag;
        var checkPlayerDeadFunc = Base + CheckPlayerDead;
        var code = Base + HitCode;
        
        AsmHelper.WriteRelativeOffsets(bytes, [
            (code + 0x1, checkPlayerDeadFunc, 5, 0x1 + 1),
            (code + 0x14, WorldChrMan.Base, 7, 0x14 + 3),
            (code + 0xB1, staggerCheckFlag, 7, 0xB1 + 2),
            (code + 0xBA, hit, 6, 0xBA + 2),
            (code + 0xCC, Hooks.Hit + 8, 5, 0xCC + 1),
        ]);
        
        memoryService.WriteBytes(code, bytes);
        hookManager.InstallHook(code, Hooks.Hit, [0x48, 0x83, 0xEC, 0x50, 0x48, 0x8B, 0x41, 0x08]);
    }

    private void InstallLethalFallHook()
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.DS3LethalFall);
        var hit = Base + Hit;
        var checkPlayerDeadFunc = Base + CheckPlayerDead;
        var code = Base + LethalFall;

        AsmHelper.WriteRelativeOffsets(bytes, [
            (code + 0x1, checkPlayerDeadFunc, 5, 0x1 + 1),
            (code + 0x8, WorldChrMan.Base, 7, 0x8 + 3),
            (code + 0x26, Functions.HasSpEffectId, 5, 0x26 + 1),
            (code + 0x31, hit, 6, 0x31 + 2),
            (code + 0x38, FallDamageKillFloor, 8, 0x38 + 4),
            (code + 0x40, Hooks.LethalFall + 8, 5, 0x40 + 1)
        ]);
        
        var originalBytes = bytes.Skip(0x1F).Take(8).ToArray();
        
        memoryService.WriteBytes(code, bytes);
        
        hookManager.InstallHook(code, Hooks.LethalFall, originalBytes);
        
    }

    private void InstallAuxHitHooks()
    {
        var auxCheckFlag = Base + CheckAuxProcFlag;
        InstallCheckAuxAttackerHook(auxCheckFlag);
        InstallAuxProcHook(auxCheckFlag);
    }

    private void InstallCheckAuxAttackerHook(nint auxCheckFlag)
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.DS3CheckAuxAttacker);
        var checkPlayerDeadFunc = Base + CheckPlayerDead;
        var code = Base + CheckAuxAttacker;
        
        AsmHelper.WriteRelativeOffsets(bytes, [
            (code + 0x1, checkPlayerDeadFunc, 5, 0x1 + 1),
            (code + 0x8, WorldChrMan.Base, 7, 0x8 + 3),
            (code + 0x2F, auxCheckFlag, 7, 0x2F + 2),
            (code + 0x38, auxCheckFlag, 7, 0x38 + 2),
            (code + 0x47, Hooks.CheckAuxAttacker + 7, 5, 0x47 + 1)
        ]);
        
        memoryService.WriteBytes(code, bytes);
        hookManager.InstallHook(code, Hooks.CheckAuxAttacker, [0x49, 0x89, 0xE3, 0x49, 0x89, 0x4B, 0x08]);
    }

    private void InstallAuxProcHook(nint auxCheckFlag)
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.DS3AuxProc);
        var hit = Base + Hit;
        var code = Base + AuxProc;
        
        AsmHelper.WriteRelativeOffsets(bytes, [
            (code + 0x9, auxCheckFlag, 7, 0x9 + 2),
            (code + 0x12, hit, 6, 0x12 + 2),
            (code + 0x18, Hooks.AuxProc + 9, 5, 0x18 + 1)
        ]);
        
        memoryService.WriteBytes(code, bytes);
        hookManager.InstallHook(code, Hooks.AuxProc, [0x41, 0x09, 0x42, 0x4C, 0x43, 0x8B, 0x4C, 0x9A, 0x24]);
        
    }

    private void InstallJailerDrainHook()
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.DS3JailerDrain);
        var getPlayerSpEffect = Base + GetSpEffect;
        var getTickCount = memoryService.GetProcAddress(Kernel32, GetTickCount64);
        var lastHitCountTime = Base + LastJailerCountTime;
        var hit = Base + Hit;
        var code = Base + JailerDrain;
        
        AsmHelper.WriteRelativeOffsets(bytes, [
            (code, Hooks.HasJailerDrain + 6, 6, 2),
            (code + 0x16, getPlayerSpEffect, 5, 0x16 + 1),
            (code + 0x2E, lastHitCountTime, 7, 0x2E + 3),
            (code + 0x43, lastHitCountTime, 7, 0x43 + 3),
            (code + 0x4A, hit, 6, 0x4A + 2),
            (code + 0x53, Hooks.HasJailerDrain + 6, 5, 0x53 + 1)
        ]);
        
        AsmHelper.WriteAbsoluteAddress(bytes, getTickCount, 0x22 + 2);
        
        memoryService.WriteBytes(code, bytes);
        hookManager.InstallHook(code, Hooks.HasJailerDrain, [0x76, 0x04, 0xf3, 0x0f, 0x59, 0xf0]);
    }

    private void InstallApplyHealthDeltaHook()
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.DS3ApplyHealthDelta);
        var hit = Base + Hit;
        var code = Base + ApplyHealthDelta;
        
        AsmHelper.WriteRelativeOffsets(bytes, [
            (code + 0x1, WorldChrMan.Base, 7, 0x1 + 3),
            (code + 0x42, WorldChrMan.Base, 7, 0x42 + 3),
            (code + 0x50, Functions.HasSpEffectId, 5, 0x50 + 1),
            (code + 0x5B, hit, 6, 0x5B + 2),
            (code + 0x6A, Hooks.ApplyHealthDelta + 8, 5, 0x6A + 1)
        ]);
        
        memoryService.WriteBytes(code, bytes);
        hookManager.InstallHook(code, Hooks.ApplyHealthDelta, [0x48, 0x8B, 0x49, 0x08, 0x41, 0x0F, 0xB6, 0xD0]);
    }

    private void InstallKillBoxHook()
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.DS3KillBox);
        var checkPlayerDeadFunc = Base + CheckPlayerDead;
        var hit = Base + Hit;
        var code = Base + KillBox;
        
        AsmHelper.WriteRelativeOffsets(bytes, [
            (code + 0x6, checkPlayerDeadFunc, 5, 0x6 + 1),
            (code + 0xF, WorldChrMan.Base, 7, 0xF + 3),
            (code + 0x20, hit, 6, 0x20 + 2),
            (code + 0x26, Hooks.KillBox + 5, 5, 0x26 + 1)
        ]);
        
        memoryService.WriteBytes(code, bytes);
        hookManager.InstallHook(code, Hooks.KillBox, [0x48, 0x89, 0xCB, 0x31, 0xD2]);
    }

    private void InstallCheckStaggerIgnore()
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.DS3CheckStaggerIgnore);
        var checkStaggerFlag = Base + CheckStaggerFlag;
        var hit = Base + Hit;
        var code = Base + CheckStagger;
        
        AsmHelper.WriteRelativeOffsets(bytes, [
            (code, checkStaggerFlag, 7, 2),
            (code + 0xD, hit, 6, 0xD + 2),
            (code + 0x13, checkStaggerFlag, 7, 0x13 + 2),
            (code + 0x20, Hooks.CheckStaggerIgnore + 6, 5, 0x20 + 1)
        ]);
        
        memoryService.WriteBytes(code, bytes);
        hookManager.InstallHook(code, Hooks.CheckStaggerIgnore, [0x45, 0x0F, 0x57, 0xC0, 0x85, 0xC0]);
    }
}