// 

using AutoHitCounter.Enums;
using AutoHitCounter.Interfaces;
using AutoHitCounter.Memory;
using AutoHitCounter.Utilities;
using static AutoHitCounter.Games.ER.EldenRingOffsets;

namespace AutoHitCounter.Games.ER;

public class EldenRingHitService(IMemoryService memoryService, HookManager hookManager) : IHitService
{
    private int _lastHitCount;
    
    public void InstallHooks()
    {
        InstallHitHook();
        InstallFallDamageHook();
        InstallKillBoxHook();
        InstallAuxHooks();
        InstallSpEffectTickDamageHook();
        InstallStaggerEndureHook();
        InstallEnvKillingHook();
        InstallCheckStateInfoHook();
        InstallDeflectTearHook();
    }

    public bool HasHit()
    {
        var current = memoryService.Read<int>(EldenRingCustomCodeOffsets.Base + EldenRingCustomCodeOffsets.Hit);
        var newHits = current - _lastHitCount;
        _lastHitCount = current;
        return newHits > 0;
    }

    private void InstallHitHook()
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.EldenRingHit);
        var hit = EldenRingCustomCodeOffsets.Base + EldenRingCustomCodeOffsets.Hit;
        var staggerCheckFlag = EldenRingCustomCodeOffsets.Base + EldenRingCustomCodeOffsets.StaggerCheckFlag;
        var stateInfoCheckFlag = EldenRingCustomCodeOffsets.Base + EldenRingCustomCodeOffsets.StateInfoCheckFlag;
        var deflectTearCheckFlag = EldenRingCustomCodeOffsets.Base + EldenRingCustomCodeOffsets.DeflectTearCheckFlag;
        var code = EldenRingCustomCodeOffsets.Base + EldenRingCustomCodeOffsets.HitCode;
        
        AsmHelper.WriteRelativeOffsets(bytes, [
            (code, stateInfoCheckFlag, 7, 2),
            (code + 0x7, deflectTearCheckFlag, 7, 0x7 + 2),
            (code + 0x39, WorldChrMan.Base, 7, 0x39 + 3),
            (code + 0x85, Functions.ChrInsByHandle, 5, 0x85 + 1),
            (code + 0xEF, deflectTearCheckFlag, 7, 0xEF + 2),
            (code + 0x115, staggerCheckFlag, 7, 0x115 + 2),
            (code + 0x126, stateInfoCheckFlag, 7, 0x126 + 2),
            (code + 0x134, GameDataMan.Base, 7, 0x134 + 3),
            (code + 0x147, hit, 6, 0x147 + 2),
            (code + 0x151, Hooks.Hit + 5, 5, 0x151 + 1),
        ]);
        
        memoryService.WriteBytes(code, bytes);
        hookManager.InstallHook(code, Hooks.Hit, [0x48, 0x89, 0x5C, 0x24, 0x08]);
    }

    private void InstallFallDamageHook()
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.EldenRingFallDamage);
        var hit = EldenRingCustomCodeOffsets.Base + EldenRingCustomCodeOffsets.Hit;
        var code = EldenRingCustomCodeOffsets.Base + EldenRingCustomCodeOffsets.FallDamage;
        AsmHelper.WriteRelativeOffsets(bytes, [
            (code + 0x7, WorldChrMan.Base, 7, 0x7 + 3),
            (code + 0x2F, hit, 6, 0x2F + 2),
            (code + 0x37, Hooks.FallDamage + 5, 5, 0x37 + 1),
        ]);
        
        memoryService.WriteBytes(code, bytes);
        hookManager.InstallHook(code, Hooks.FallDamage, [0xC6, 0x44, 0x24, 0x30, 0x01]);
    }

    private void InstallKillBoxHook()
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.EldenRingKillBox);
        var hit = EldenRingCustomCodeOffsets.Base + EldenRingCustomCodeOffsets.Hit;
        var code = EldenRingCustomCodeOffsets.Base + EldenRingCustomCodeOffsets.KillBox;
        AsmHelper.WriteRelativeOffsets(bytes, [
            (code + 0x7, WorldChrMan.Base, 7, 0x7 + 3),
            (code + 0x33, Functions.HasSpEffectId, 5, 0x33 + 1),
            (code + 0x41, hit, 6, 0x41 + 2),
            (code + 0x49, Hooks.KillBox + 5, 5, 0x49 + 1),
        ]);
        memoryService.WriteBytes(code, bytes);
        hookManager.InstallHook(code, Hooks.KillBox, [ 0xC6, 0x44, 0x24, 0x28, 0x01]);
    }

    private void InstallAuxHooks()
    {
        var checkAuxFlag = EldenRingCustomCodeOffsets.Base + EldenRingCustomCodeOffsets.CheckAuxProcFlag;
        InstallAuxDamageAttackerHook(checkAuxFlag);
        InstallAuxProcHook(checkAuxFlag);
    }

    private void InstallAuxDamageAttackerHook(nint checkAuxFlag)
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.EldenRingAuxDamageAttacker);
        var code = EldenRingCustomCodeOffsets.Base + EldenRingCustomCodeOffsets.CheckAuxAttacker;
        AsmHelper.WriteRelativeOffsets(bytes, [
        (code + 0x1, WorldChrMan.Base, 7, 0x1 + 3),
        (code + 0x28, checkAuxFlag, 7, 0x28 + 2),
        (code + 0x31, checkAuxFlag, 7, 0x31 + 2),
        (code + 0x40, Hooks.AuxDamageAttacker + 7, 5, 0x40 + 1)
        ]);
        
        memoryService.WriteBytes(code, bytes);
        hookManager.InstallHook(code, Hooks.AuxDamageAttacker, [ 0x48, 0x8B, 0x8B, 0x90, 0x01, 0x00, 0x00]);
    }

    private void InstallAuxProcHook(nint checkAuxFlag)
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.EldenRingAuxProc);
        var hit = EldenRingCustomCodeOffsets.Base + EldenRingCustomCodeOffsets.Hit;
        var code = EldenRingCustomCodeOffsets.Base + EldenRingCustomCodeOffsets.AuxProc;
        
        AsmHelper.WriteRelativeOffsets(bytes, [
        (code + 0x6, checkAuxFlag, 7, 0x6 + 2),
        (code + 0xF, hit, 6, 0xF + 2),
        (code + 0x15, Hooks.AuxProc + 6, 5, 0x15 + 1)
        ]);
        
        memoryService.WriteBytes(code, bytes);
        hookManager.InstallHook(code, Hooks.AuxProc, [0x09, 0x83, 0xB8, 0x00, 0x00, 0x00]);
    }

    private void InstallSpEffectTickDamageHook()
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.EldenRingSpEffectTickDamage);
        var hit = EldenRingCustomCodeOffsets.Base + EldenRingCustomCodeOffsets.Hit;
        var code = EldenRingCustomCodeOffsets.Base + EldenRingCustomCodeOffsets.SpEffectTickDamage;
        AsmHelper.WriteRelativeOffsets(bytes, [
            (code + 0x4, WorldChrMan.Base, 7, 0x4 + 3),
            (code + 0xB2, hit, 6, 0xB2 + 2),
            (code + 0xC2, Hooks.SpEffectTickDamage + 6, 5, 0xC2 + 1),
        ]);
        
        memoryService.WriteBytes(code, bytes);
        hookManager.InstallHook(code, Hooks.SpEffectTickDamage, [0xF3, 0x0F, 0x11, 0x44, 0x24, 0x20]);
    }

    private void InstallStaggerEndureHook()
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.EldenRingStaggerEndure);
        var staggerCheckFlag = EldenRingCustomCodeOffsets.Base + EldenRingCustomCodeOffsets.StaggerCheckFlag;
        var hit = EldenRingCustomCodeOffsets.Base + EldenRingCustomCodeOffsets.Hit;
        var code = EldenRingCustomCodeOffsets.Base + EldenRingCustomCodeOffsets.StaggerEndure;
        
        AsmHelper.WriteRelativeOffsets(bytes, [
        (code, staggerCheckFlag, 7, 2),
        (code + 0xD, hit, 6, 0xD + 2),
        (code + 0x13, staggerCheckFlag, 7, 0x13 + 2),
        (code + 0x20, Hooks.EndureStagger + 6, 5, 0x20 + 1),
        ]);
        
        memoryService.WriteBytes(code, bytes);
        hookManager.InstallHook(code, Hooks.EndureStagger, [0x45, 0x0F, 0x57, 0xC9, 0x84, 0xC0]);
    }

    private void InstallEnvKillingHook()
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.EldenRingEnvKilling);
        var hit = EldenRingCustomCodeOffsets.Base + EldenRingCustomCodeOffsets.Hit;
        var code = EldenRingCustomCodeOffsets.Base + EldenRingCustomCodeOffsets.EnvKilling;
        
        AsmHelper.WriteRelativeOffsets(bytes, [
        (code + 0x15, WorldChrMan.Base, 7, 0x15 + 3),
        (code + 0x4E, Functions.ChrInsByHandle, 5, 0x4E + 1),
        (code + 0x66, hit, 6, 0x66 + 2),
        (code + 0x70, Hooks.EnvKilling + 6, 5, 0x70 + 1),
        ]);
        
        memoryService.WriteBytes(code, bytes);
        hookManager.InstallHook(code, Hooks.EnvKilling, [0xF3, 0x0F, 0x11, 0x4C, 0x24, 0x28]);
        
    }

    private void InstallCheckStateInfoHook()
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.EldenRingCheckStateInfo);
        var stateInfoCheckFlag = EldenRingCustomCodeOffsets.Base + EldenRingCustomCodeOffsets.StateInfoCheckFlag;
        var hit = EldenRingCustomCodeOffsets.Base + EldenRingCustomCodeOffsets.Hit;
        var code = EldenRingCustomCodeOffsets.Base + EldenRingCustomCodeOffsets.StateInfoCheck;
        
        AsmHelper.WriteRelativeOffsets(bytes, [
            (code, stateInfoCheckFlag, 7, 2),
            (code + 0x12, hit, 6, 0x12 + 2),
            (code + 0x1F, Hooks.CheckStateInfo + 7, 5, 0x1F + 1)
        ]);
        
        memoryService.WriteBytes(code, bytes);
        hookManager.InstallHook(code, Hooks.CheckStateInfo, [0x0F, 0xB6, 0x81, 0x59, 0x02, 0x00, 0x00]);

    }

    private void InstallDeflectTearHook()
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.EldenRingDeflectTear);
        var hit = EldenRingCustomCodeOffsets.Base + EldenRingCustomCodeOffsets.Hit;
        var deflectTearCheckFlag = EldenRingCustomCodeOffsets.Base + EldenRingCustomCodeOffsets.DeflectTearCheckFlag;
        var code = EldenRingCustomCodeOffsets.Base + EldenRingCustomCodeOffsets.DeflectTearCheck;
        
        AsmHelper.WriteRelativeOffsets(bytes, [
            (code + 0x5, deflectTearCheckFlag, 7, 0x5 + 2),
            (code + 0xE, deflectTearCheckFlag, 7, 0xE + 2),
            (code + 0x2C, hit, 6, 0x2C + 2),
            (code + 0x32, Hooks.CheckDeflectTear + 5, 5, 0x32 + 1),
        ]);
        
        memoryService.WriteBytes(code, bytes);
        hookManager.InstallHook(code, Hooks.CheckDeflectTear, [0xF3, 0x0F, 0x10, 0x6D, 0xA0]);
    }
}