// 

using System;
using System.Collections.Generic;
using System.Linq;
using AutoHitCounter.Enums;
using AutoHitCounter.Interfaces;
using AutoHitCounter.Memory;
using AutoHitCounter.Utilities;
using static AutoHitCounter.Games.DSR.DSRCustomCodeOffsets;
using static AutoHitCounter.Games.DSR.DSROffsets;

namespace AutoHitCounter.Games.DSR;

public class DSRHitService(IMemoryService memoryService, HookManager hookManager) : IHitService
{
    private int _lastHitCount;
    private readonly List<nint> _hooks = [];

    public void InstallHooks()
    {
        InstallHitHook();
        InstallApplyHealthDeltaHook();
        InstallKillChrHook();
        InstallCheckAuxAttacker();
        InstallAuxProcHook();
        InstallClearThrowStateHook();
        InstallSetThrowStateHook();
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
        if (_hooks.Any(h => memoryService.Read<byte>(h) != 0xE9))
            InstallHooks();
    }
    
    public void ResetFlags()
    {
        memoryService.Write(Base + InThrowFlag, false);
    }

    private void InstallHook(nint code, nint hookAddr, byte[] originalBytes)
    {
        hookManager.InstallHook(code, hookAddr, originalBytes);
        _hooks.Add(hookAddr);
    }

    private void InstallHitHook()
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.DSRHit);
        var hit = Base + Hit;
        var envDeathFlag = Base + CheckEnvDeathFlag;
        var throwFlag = Base + InThrowFlag;
        var code = Base + HitCode;
        var originalBytes = DSROriginalBytes.Hit.GetOriginal();

        Array.Copy(originalBytes, 0, bytes, 0xAB, originalBytes.Length);
        
        AsmHelper.WriteRelativeOffsets(bytes, [
            (code, envDeathFlag, 7, 2),
            (code + 0x7, throwFlag, 7, 0x7 + 2),
            (code + 0x15, WorldChrMan.Base, 7, 0x15 + 3),
            (code + 0x9B, hit, 6, 0x9B + 2),
            (code + 0xA3, envDeathFlag, 7, 0xA3 + 2),
            (code + 0xB0, Hooks.Hit + 5, 5, 0xB0 + 1),
        ]);

        memoryService.WriteBytes(code, bytes);
        InstallHook(code, Hooks.Hit, [0x48, 0x89, 0x6C, 0x24, 0x10]);
    }

    private void InstallApplyHealthDeltaHook()
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.DSRApplyHealthDelta);
        var hit = Base + Hit;
        var envDeathFlag = Base + CheckEnvDeathFlag;
        var code = Base + ApplyHealthDelta;


        AsmHelper.WriteRelativeOffsets(bytes, [
            (code + 0x17, envDeathFlag, 7, 0x17 + 2),
            (code + 0x42, WorldChrMan.Base, 7, 0x42 + 3),
            (code + 0x54, hit, 6, 0x54 + 2),
            (code + 0x5B, Hooks.ApplyHealthDelta + 5, 5, 0x5B + 1),
        ]);

        AsmHelper.WriteAbsoluteAddresses(bytes, [
            (FallDmgRetAddr, 0x6 + 2),
            (EnvDeathRetAddr, 0x20 + 2),
            (AuxDeathRetAddr, 0x31 + 2)
        ]);

        memoryService.WriteBytes(code, bytes);
        InstallHook(code, Hooks.ApplyHealthDelta, [0x48, 0x89, 0x7C, 0x24, 0x40]);
    }

    private void InstallKillChrHook()
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.DSRKillChr);
        var hit = Base + Hit;
        var code = Base + KillChr;
        var originalBytes = DSROriginalBytes.KillChr.GetOriginal();
        
        Array.Copy(originalBytes, 0, bytes, 0, originalBytes.Length);

        AsmHelper.WriteRelativeOffsets(bytes, [
            (code + 0x6, WorldChrMan.Base, 7, 0x6 + 3),
            (code + 0x18, hit, 6, 0x18 + 2),
            (code + 0x1F, Hooks.KillChr + 5, 5, 0x1F + 1)
        ]);
        
        memoryService.WriteBytes(code, bytes);
        InstallHook(code, Hooks.KillChr, originalBytes);
    }

    private void InstallCheckAuxAttacker()
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.DSRCheckAuxAttacker);
        var code = Base + CheckAuxAttacker;
        var checkAuxProcFlag = Base + CheckAuxProcFlag;
        
        AsmHelper.WriteRelativeOffsets(bytes, [
            (code, checkAuxProcFlag, 7, 2),
            (code + 0x17, WorldChrMan.Base, 7, 0x17 + 3),
            (code + 0x4B, checkAuxProcFlag, 7, 0x4B + 2),
            (code + 0x53, Hooks.CheckAuxAttacker + 7, 5, 0x53 + 1)
        ]);
        
        memoryService.WriteBytes(code, bytes);
        InstallHook(code, Hooks.CheckAuxAttacker, [0x0F, 0xB6, 0x80, 0x56, 0x01, 0x00, 0x00]);
    }

    private void InstallAuxProcHook()
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.DSRAuxProc);
        var code = Base + CheckAuxProc;
        var hit = Base + Hit;
        var checkAuxProcFlag = Base + CheckAuxProcFlag;
        
        AsmHelper.WriteRelativeOffsets(bytes, [
            (code + 0x7, checkAuxProcFlag, 7, 0x7 + 2),
            (code + 0x11, WorldChrMan.Base, 7, 0x11 + 3),
            (code + 0x34, hit, 6, 0x34 + 2),
            (code + 0x3B, Hooks.CheckAuxProc + 7, 5, 0x3B + 1)
        ]);
        
        memoryService.WriteBytes(code, bytes);
        InstallHook(code, Hooks.CheckAuxProc, [0x44, 0x8B, 0x83, 0x34, 0x04, 0x00, 0x00]);
    }

    private void InstallClearThrowStateHook()
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.DSRClearThrowState);
        var code = Base + ClearThrowState;
        var throwFlag = Base + InThrowFlag;
        
        AsmHelper.WriteRelativeOffsets(bytes, [
            (code + 0xD, WorldChrMan.Base, 7, 0xD + 3),
            (code + 0x25, throwFlag, 7, 0x25 + 2),
            (code + 0x2D, Hooks.ClearThrowState + 7, 5, 0x2D + 1)
        ]);
        
        memoryService.WriteBytes(code, bytes);
        InstallHook(code, Hooks.ClearThrowState, [ 0x48, 0x8B, 0x8B, 0x48, 0x04, 0x00, 0x00]);
    }

    private void InstallSetThrowStateHook()
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.DSRSetThrowState);
        var code = Base + SetThrowState;
        var throwFlag = Base + InThrowFlag;
        
        AsmHelper.WriteRelativeOffsets(bytes, [
            (code + 0x2D, WorldChrMan.Base, 7, 0x2D + 3),
            (code + 0x47, throwFlag, 7, 0x47 + 2),
            (code + 0x50, Hooks.SetThrowState + 10, 5, 0x50 + 1),
        ]);
        
        memoryService.WriteBytes(code, bytes);
        InstallHook(code, Hooks.SetThrowState, [ 0x81, 0x8B, 0xDC, 0x01, 0x00, 0x00, 0x00, 0x00, 0x20, 0x00]);
    }
}