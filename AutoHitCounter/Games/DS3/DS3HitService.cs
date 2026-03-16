// 

using System.Collections.Generic;
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
    private readonly List<nint> _hooks = [];

    private const string Kernel32 = "kernel32.dll";
    private const string GetTickCount64 = "GetTickCount64";

    public void InstallHooks()
    {
        WritePlayerDeadCheck();
        WriteGetPlayerSpEffect();

        InstallHitHook();
        InstallAuxHitHooks();
        InstallJailerDrainHook();
        InstallApplyHealthDeltaHook();
        InstallKillBoxHook();
        InstallCheckStaggerIgnore();
        InstallStoreFallHeightHook();
        InstallFallDamageDisabledHook();
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
        if (_hooks.Any(h => memoryService.Read<byte>(h) != 0xE9))
            InstallHooks();
    }

    private void InstallHook(nint code, nint hookAddr, byte[] originalBytes)
    {
        hookManager.InstallHook(code, hookAddr, originalBytes);
        if (!_hooks.Contains(hookAddr))
            _hooks.Add(hookAddr);
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
            (code + 0xBF, staggerCheckFlag, 7, 0xBF + 2),
            (code + 0xC8, hit, 6, 0xC8 + 2),
            (code + 0xDA, Hooks.Hit + 8, 5, 0xDA + 1),
        ]);

        memoryService.WriteBytes(code, bytes);
        InstallHook(code, Hooks.Hit, [0x48, 0x83, 0xEC, 0x50, 0x48, 0x8B, 0x41, 0x08]);
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
        InstallHook(code, Hooks.CheckAuxAttacker, [0x49, 0x89, 0xE3, 0x49, 0x89, 0x4B, 0x08]);
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
        InstallHook(code, Hooks.AuxProc, [0x41, 0x09, 0x42, 0x4C, 0x43, 0x8B, 0x4C, 0x9A, 0x24]);
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
        InstallHook(code, Hooks.HasJailerDrain, [0x76, 0x04, 0xf3, 0x0f, 0x59, 0xf0]);
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
        InstallHook(code, Hooks.ApplyHealthDelta, [0x48, 0x8B, 0x49, 0x08, 0x41, 0x0F, 0xB6, 0xD0]);
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
        InstallHook(code, Hooks.KillBox, [0x48, 0x89, 0xCB, 0x31, 0xD2]);
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
        InstallHook(code, Hooks.CheckStaggerIgnore, [0x45, 0x0F, 0x57, 0xC0, 0x85, 0xC0]);
    }
    
    private void InstallStoreFallHeightHook()
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.DS3StoreFallHeight);
        var storedHeight = Base + StoredFallHeight;
        var fallHitCountedFlag = Base + FallHitCountedFlag;
        var code = Base + StoreFallHeight;
        
        AsmHelper.WriteRelativeOffsets(bytes, [
            (code + 0x1, WorldChrMan.Base, 7, 0x1 + 3),
            (code + 0x1E, storedHeight, 8, 0x1E + 4),
            (code + 0x26, Float20, 7, 0x26 + 3),
            (code + 0x2F, fallHitCountedFlag, 7, 0x2F + 2),
            (code + 0x37, Float100, 8, 0x37 + 4),
            (code + 0x3F, Hooks.FallHeight + 8, 5, 0x3F + 1)
        ]);
        
        var originalBytes = bytes.Skip(0x37).Take(8).ToArray();
        
        memoryService.WriteBytes(code, bytes);
        
        InstallHook(code, Hooks.FallHeight, originalBytes);
    }

    private void InstallFallDamageDisabledHook()
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.DS3IsFallDamageDisabled);
        var storedHeight = Base + StoredFallHeight;
        var fallHitCountedFlag = Base + FallHitCountedFlag;
        var hit = Base + Hit;
        var code = Base + LethalFallCheck;
        AsmHelper.WriteRelativeOffsets(bytes, [
            (code, Functions.IsFallDamageDisabled, 5, 1),
            (code + 0x7, WorldChrMan.Base, 7, 0x7 + 3),
            (code + 0x35, fallHitCountedFlag, 7, 0x35 + 2),
            (code + 0x42, storedHeight, 9, 0x42 + 5),
            (code + 0x4B, Float20, 8,  0x4B + 4),
            (code + 0x5F, Functions.HasSpEffectId, 5, 0x5F + 1),
            (code + 0x70, Functions.HasSpEffectId, 5, 0x70 + 1),
            (code + 0x81, Functions.HasSpEffectId, 5, 0x81 + 1),
            (code + 0x92, Functions.HasSpEffectId, 5, 0x92 + 1),
            (code + 0x9B, fallHitCountedFlag, 7, 0x9B + 2),
            (code + 0xA2, hit, 6, 0xA2 + 2),
            (code + 0xA8, fallHitCountedFlag, 7, 0xA8 + 2),
            (code + 0xB3, storedHeight, 9, 0xB3 + 5),
            (code + 0xBC, Float20, 8,  0xBC + 4),
            (code + 0xC6, fallHitCountedFlag, 7, 0xC6 + 2),
            (code + 0xCF, Hooks.IsFallDmgDisabledHook + 5, 5, 0xCF + 1),
            
        ]);


        var originalBytes = memoryService.ReadBytes(Hooks.IsFallDmgDisabledHook, 5);

        memoryService.WriteBytes(code, bytes);
        InstallHook(code, Hooks.IsFallDmgDisabledHook, originalBytes);
    }
}