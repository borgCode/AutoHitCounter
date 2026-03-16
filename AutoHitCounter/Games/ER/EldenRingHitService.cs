// 

using System.Collections.Generic;
using System.Linq;
using AutoHitCounter.Enums;
using AutoHitCounter.Interfaces;
using AutoHitCounter.Memory;
using AutoHitCounter.Utilities;
using static AutoHitCounter.Games.ER.EldenRingCustomCodeOffsets;
using static AutoHitCounter.Games.ER.EldenRingOffsets;

namespace AutoHitCounter.Games.ER;

public class EldenRingHitService(IMemoryService memoryService, HookManager hookManager) : IHitService
{
    private int _lastHitCount;
    private readonly List<nint> _hooks = [];

    public void InstallHooks()
    {
        WritePlayerDeadCheck();

        InstallHitHook();
        InstallFallDamageHook();
        InstallKillBoxHook();
        InstallAuxHooks();
        InstallSpEffectTickDamageHook();
        InstallStaggerEndureHook();
        InstallEnvKillingHook();
        InstallCheckStateInfoHook();
        InstallDeflectTearHook();
        InstallKillChrHook();
        InstallSetThrowStateHook();
        InstallClearThrowStateHook();
    }

    public void ResetFlags()
    {
        memoryService.Write(Base + InThrowFlag, false);
    }

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

    public bool HasHit()
    {
        var current = memoryService.Read<int>(Base + Hit);
        var newHits = current - _lastHitCount;
        _lastHitCount = current;
        return newHits > 0;
    }

    private void WritePlayerDeadCheck()
    {
        var code = Base + CheckPlayerDead;

        var bytes = AsmLoader.GetAsmBytes(AsmScript.EldenRingCheckPlayerDead);
        AsmHelper.WriteRelativeOffset(bytes, code, WorldChrMan.Base, 7, 3);
        memoryService.WriteBytes(code, bytes);
    }

    private void InstallHitHook()
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.EldenRingHit);
        var hit = Base + Hit;
        var staggerCheckFlag = Base + StaggerCheckFlag;
        var stateInfoCheckFlag = Base + StateInfoCheckFlag;
        var throwStateFlag = Base + InThrowFlag;
        var deflectTearCheckFlag = Base + DeflectTearCheckFlag;
        var checkPlayerDeadFunc = Base + CheckPlayerDead;
        var code = Base + HitCode;

        AsmHelper.WriteRelativeOffsets(bytes, [
            (code, stateInfoCheckFlag, 7, 2),
            (code + 0x7, deflectTearCheckFlag, 7, 0x7 + 2),
            (code + 0x13, throwStateFlag, 7, 0x13 + 2),
            (code + 0x21, checkPlayerDeadFunc, 5, 0x21 + 1),
            (code + 0x8F, WorldChrMan.Base, 7, 0x8F + 3),
            (code + 0xDB, Functions.ChrInsByHandle, 5, 0xDB + 1),
            (code + 0x155, deflectTearCheckFlag, 7, 0x155 + 2),
            (code + 0x17B, staggerCheckFlag, 7, 0x17B + 2),
            (code + 0x18C, stateInfoCheckFlag, 7, 0x18C + 2),
            (code + 0x1A6, GameDataMan.Base, 7, 0x1A6 + 3),
            (code + 0x1C5, hit, 6, 0x1C5 + 2),
            (code + 0x1CF, Hooks.Hit + 5, 5, 0x1CF + 1),
        ]);

        memoryService.WriteBytes(code, bytes);
        InstallHook(code, Hooks.Hit, [0x48, 0x89, 0x5C, 0x24, 0x08]);
    }

    private void InstallFallDamageHook()
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.EldenRingFallDamage);
        var hit = Base + Hit;
        var code = Base + FallDamage;
        AsmHelper.WriteRelativeOffsets(bytes, [
            (code + 0x7, WorldChrMan.Base, 7, 0x7 + 3),
            (code + 0x2F, hit, 6, 0x2F + 2),
            (code + 0x37, Hooks.FallDamage + 5, 5, 0x37 + 1),
        ]);

        memoryService.WriteBytes(code, bytes);
        InstallHook(code, Hooks.FallDamage, [0xC6, 0x44, 0x24, 0x30, 0x01]);
    }

    private void InstallKillBoxHook()
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.EldenRingKillBox);
        var hit = Base + Hit;
        var code = Base + KillBox;
        AsmHelper.WriteRelativeOffsets(bytes, [
            (code + 0x7, WorldChrMan.Base, 7, 0x7 + 3),
            (code + 0x33, Functions.HasSpEffectId, 5, 0x33 + 1),
            (code + 0x41, hit, 6, 0x41 + 2),
            (code + 0x49, Hooks.KillBox + 5, 5, 0x49 + 1),
        ]);
        memoryService.WriteBytes(code, bytes);
        InstallHook(code, Hooks.KillBox, [0xC6, 0x44, 0x24, 0x28, 0x01]);
    }

    private void InstallAuxHooks()
    {
        var checkAuxFlag = Base + CheckAuxProcFlag;
        InstallAuxDamageAttackerHook(checkAuxFlag);
        InstallAuxProcHook(checkAuxFlag);
    }

    private void InstallAuxDamageAttackerHook(nint checkAuxFlag)
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.EldenRingAuxDamageAttacker);
        var code = Base + CheckAuxAttacker;
        AsmHelper.WriteRelativeOffsets(bytes, [
            (code + 0x1, WorldChrMan.Base, 7, 0x1 + 3),
            (code + 0x28, checkAuxFlag, 7, 0x28 + 2),
            (code + 0x31, checkAuxFlag, 7, 0x31 + 2),
            (code + 0x40, Hooks.AuxDamageAttacker + 7, 5, 0x40 + 1)
        ]);

        memoryService.WriteBytes(code, bytes);
        InstallHook(code, Hooks.AuxDamageAttacker, [0x48, 0x8B, 0x8B, 0x90, 0x01, 0x00, 0x00]);
    }

    private void InstallAuxProcHook(nint checkAuxFlag)
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.EldenRingAuxProc);
        var hit = Base + Hit;
        var code = Base + AuxProc;

        AsmHelper.WriteRelativeOffsets(bytes, [
            (code + 0x6, checkAuxFlag, 7, 0x6 + 2),
            (code + 0xF, hit, 6, 0xF + 2),
            (code + 0x15, Hooks.AuxProc + 6, 5, 0x15 + 1)
        ]);

        memoryService.WriteBytes(code, bytes);
        InstallHook(code, Hooks.AuxProc, [0x09, 0x83, 0xB8, 0x00, 0x00, 0x00]);
    }

    private void InstallSpEffectTickDamageHook()
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.EldenRingSpEffectTickDamage);
        var hit = Base + Hit;
        var checkPlayerDeadFunc = Base + CheckPlayerDead;

        var code = Base + SpEffectTickDamage;
        AsmHelper.WriteRelativeOffsets(bytes, [
            (code + 0x4, checkPlayerDeadFunc, 5, 0x4 + 1),
            (code + 0xF, WorldChrMan.Base, 7, 0xF + 3),
            (code + 0xE1, hit, 6, 0xE1 + 2),
            (code + 0xF1, Hooks.SpEffectTickDamage + 6, 5, 0xF1 + 1),
        ]);

        memoryService.WriteBytes(code, bytes);
        InstallHook(code, Hooks.SpEffectTickDamage, [0xF3, 0x0F, 0x11, 0x44, 0x24, 0x20]);
    }

    private void InstallStaggerEndureHook()
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.EldenRingStaggerEndure);
        var staggerCheckFlag = Base + StaggerCheckFlag;
        var hit = Base + Hit;
        var code = Base + StaggerEndure;

        AsmHelper.WriteRelativeOffsets(bytes, [
            (code, staggerCheckFlag, 7, 2),
            (code + 0xD, hit, 6, 0xD + 2),
            (code + 0x13, staggerCheckFlag, 7, 0x13 + 2),
            (code + 0x20, Hooks.EndureStagger + 6, 5, 0x20 + 1),
        ]);

        memoryService.WriteBytes(code, bytes);
        InstallHook(code, Hooks.EndureStagger, [0x45, 0x0F, 0x57, 0xC9, 0x84, 0xC0]);
    }

    private void InstallEnvKillingHook()
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.EldenRingEnvKilling);
        var hit = Base + Hit;
        var checkPlayerDeadFunc = Base + CheckPlayerDead;

        var code = Base + EnvKilling;

        AsmHelper.WriteRelativeOffsets(bytes, [
            (code + 0x7, checkPlayerDeadFunc, 5, 0x7 + 1),
            (code + 0xD, Hooks.EnvKilling + 6, 5, 0xD + 2),
            (code + 0x22, WorldChrMan.Base, 7, 0x22 + 3),
            (code + 0x5B, Functions.ChrInsByHandle, 5, 0x5B + 1),
            (code + 0x73, hit, 6, 0x73 + 2),
            (code + 0x7D, Hooks.EnvKilling + 6, 5, 0x7D + 1),
        ]);

        memoryService.WriteBytes(code, bytes);
        InstallHook(code, Hooks.EnvKilling, [0xF3, 0x0F, 0x11, 0x4C, 0x24, 0x28]);
    }

    private void InstallCheckStateInfoHook()
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.EldenRingCheckStateInfo);
        var stateInfoCheckFlag = Base + StateInfoCheckFlag;
        var hit = Base + Hit;
        var code = Base + StateInfoCheck;

        AsmHelper.WriteRelativeOffsets(bytes, [
            (code, stateInfoCheckFlag, 7, 2),
            (code + 0x12, hit, 6, 0x12 + 2),
            (code + 0x1F, Hooks.CheckStateInfo + 7, 5, 0x1F + 1)
        ]);

        memoryService.WriteBytes(code, bytes);
        InstallHook(code, Hooks.CheckStateInfo, [0x0F, 0xB6, 0x81, 0x59, 0x02, 0x00, 0x00]);
    }

    private void InstallDeflectTearHook()
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.EldenRingDeflectTear);
        var hit = Base + Hit;
        var throwStateFlag = Base + InThrowFlag;
        var deflectTearCheckFlag = Base + DeflectTearCheckFlag;
        var code = Base + DeflectTearCheck;

        AsmHelper.WriteRelativeOffsets(bytes, [
            (code + 0x5, throwStateFlag, 7, 0x5 + 2),
            (code + 0xE, deflectTearCheckFlag, 7, 0xE + 2),
            (code + 0x17, deflectTearCheckFlag, 7, 0x17 + 2),
            (code + 0x35, hit, 6, 0x35 + 2),
            (code + 0x3B, Hooks.CheckDeflectTear + 5, 5, 0x3B + 1),
        ]);

        memoryService.WriteBytes(code, bytes);
        InstallHook(code, Hooks.CheckDeflectTear, [0xF3, 0x0F, 0x10, 0x6D, 0xA0]);
    }

    private void InstallKillChrHook()
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.EldenRingKillChr);
        var hit = Base + Hit;
        var checkPlayerDeadFunc = Base + CheckPlayerDead;

        var code = Base + KillChr;
        AsmHelper.WriteRelativeOffsets(bytes, [
            (code + 0x1, checkPlayerDeadFunc, 5, 0x1 + 1),
            (code + 0x8, WorldChrMan.Base, 7, 0x8 + 3),
            (code + 0x18, hit, 6, 0x18 + 2),
            (code + 0x24, Hooks.KillChr + 6, 5, 0x24 + 1),
        ]);

        memoryService.WriteBytes(code, bytes);
        InstallHook(code, Hooks.KillChr, [0x40, 0x53, 0x48, 0x83, 0xEC, 0x40]);
    }

    private void InstallSetThrowStateHook()
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.EldenRingSetThrowState);
        var throwStateFlag = Base + InThrowFlag;
        var hit = Base + Hit;
        var checkPlayerDeadFunc = Base + CheckPlayerDead;
        var code = Base + SetThrowState;
        
        AsmHelper.WriteRelativeOffsets(bytes, [
            (code + 0x9, checkPlayerDeadFunc, 5, 0x9 + 1),
            (code + 0x10, WorldChrMan.Base, 7, 0x10 + 3),
            (code + 0x3D, throwStateFlag, 7, 0x3D + 2),
            (code + 0x4E, hit, 6, 0x4E + 2),
            (code + 0x55, Hooks.SetThrowState + 8, 5, 0x55 + 1),
        ]);

        memoryService.WriteBytes(code, bytes);
        InstallHook(code, Hooks.SetThrowState, [0x41, 0x80, 0x8E, 0x67, 0x02, 0x00, 0x00, 0x10]);
    }

    private void InstallClearThrowStateHook()
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.EldenRingClearThrowState);
        var throwStateFlag = Base + InThrowFlag;
        var code = Base + ClearThrowState;
        
        AsmHelper.WriteRelativeOffsets(bytes, [
            (code + 0x6, WorldChrMan.Base, 7, 0x6 + 3),
            (code + 0x3C, throwStateFlag, 7, 0x3C + 2),
            (code + 0x44, Hooks.ClearThrowState + 6, 5, 0x44 + 1),
        ]);

        memoryService.WriteBytes(code, bytes);
        InstallHook(code, Hooks.ClearThrowState, [0x40, 0x53, 0x48, 0x83, 0xEC, 0x30]);
    }
}