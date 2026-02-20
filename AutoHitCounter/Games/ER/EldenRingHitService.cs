// 

using AutoHitCounter.Enums;
using AutoHitCounter.Games.DS2S;
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
    }

    public bool HasHit()
    {
        var current = memoryService.Read<int>(CodeCaveOffsets.Base + CodeCaveOffsets.Hit);
        var newHits = current - _lastHitCount;
        _lastHitCount = current;
        return newHits > 0;
    }

    private void InstallHitHook()
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.EldenRingHit);
        var hit = CodeCaveOffsets.Base + CodeCaveOffsets.Hit;
        var staggerCheckFlag = CodeCaveOffsets.Base + CodeCaveOffsets.StaggerCheckFlag;
        var code = CodeCaveOffsets.Base + CodeCaveOffsets.HitCode;
        AsmHelper.WriteRelativeOffsets(bytes, [
            (code + 0x26, WorldChrMan.Base, 7, 0x26 + 3),
            (code + 0x72, Functions.ChrInsByHandle, 5, 0x72 + 1),
            (code + 0xD3, staggerCheckFlag, 7, 0xD3 + 2),
            (code + 0xDC, hit, 6, 0xDC + 2),
            (code + 0xE6, Hooks.Hit + 5, 5, 0xE6 + 1),
        ]);
        
        memoryService.WriteBytes(code, bytes);
        hookManager.InstallHook(code, Hooks.Hit, [0x48, 0x89, 0x5C, 0x24, 0x08]);
    }

    private void InstallFallDamageHook()
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.EldenRingFallDamage);
        var hit = CodeCaveOffsets.Base + CodeCaveOffsets.Hit;
        var code = CodeCaveOffsets.Base + CodeCaveOffsets.FallDamage;
        AsmHelper.WriteRelativeOffsets(bytes, [
            (code + 0x7, WorldChrMan.Base, 7, 0x7 + 3),
            (code + 0x3F, Functions.ChrInsByHandle, 5, 0x3F + 1),
            (code + 0x67, hit, 6, 0x67 + 2),
            (code + 0x6F, Hooks.FallDamage + 5, 5, 0x6F + 1),
        ]);
        
        memoryService.WriteBytes(code, bytes);
        hookManager.InstallHook(code, Hooks.FallDamage, [0xC6, 0x44, 0x24, 0x30, 0x01]);
    }

    private void InstallKillBoxHook()
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.EldenRingKillBox);
        var hit = CodeCaveOffsets.Base + CodeCaveOffsets.Hit;
        var code = CodeCaveOffsets.Base + CodeCaveOffsets.KillBox;
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
        var checkAuxFlag = CodeCaveOffsets.Base + CodeCaveOffsets.CheckAuxProcFlag;
        InstallAuxDamageAttackerHook(checkAuxFlag);
        InstallAuxProcHook(checkAuxFlag);
    }

    private void InstallAuxDamageAttackerHook(nint checkAuxFlag)
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.EldenRingAuxDamageAttacker);
        var code = CodeCaveOffsets.Base + CodeCaveOffsets.CheckAuxAttacker;
        AsmHelper.WriteRelativeOffsets(bytes, [
        (code + 0x1, WorldChrMan.Base, 7, 0x1 + 3),
        (code + 0x1A, checkAuxFlag, 7, 0x1A + 2),
        (code + 0x23, checkAuxFlag, 7, 0x23 + 2),
        (code + 0x32, Hooks.AuxDamageAttacker + 7, 5, 0x32 + 1)
        ]);
        
        memoryService.WriteBytes(code, bytes);
        hookManager.InstallHook(code, Hooks.AuxDamageAttacker, [ 0x48, 0x8B, 0x8B, 0x90, 0x01, 0x00, 0x00]);
    }

    private void InstallAuxProcHook(nint checkAuxFlag)
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.EldenRingAuxProc);
        var hit = CodeCaveOffsets.Base + CodeCaveOffsets.Hit;
        var code = CodeCaveOffsets.Base + CodeCaveOffsets.AuxProc;
        
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
        var hit = CodeCaveOffsets.Base + CodeCaveOffsets.Hit;
        var code = CodeCaveOffsets.Base + CodeCaveOffsets.SpEffectTickDamage;
        AsmHelper.WriteRelativeOffsets(bytes, [
            (code + 0x4, WorldChrMan.Base, 7, 0x4 + 3),
            (code + 0x70, hit, 6, 0x70 + 2),
            (code + 0x80, Hooks.SpEffectTickDamage + 6, 5, 0x80 + 1),
        ]);
        
        memoryService.WriteBytes(code, bytes);
        hookManager.InstallHook(code, Hooks.SpEffectTickDamage, [0xF3, 0x0F, 0x11, 0x44, 0x24, 0x20]);
    }

    private void InstallStaggerEndureHook()
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.EldenRingStaggerEndure);
        var staggerCheckFlag = CodeCaveOffsets.Base + CodeCaveOffsets.StaggerCheckFlag;
        var hit = CodeCaveOffsets.Base + CodeCaveOffsets.Hit;
        var code = CodeCaveOffsets.Base + CodeCaveOffsets.StaggerEndure;
        
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
        var hit = CodeCaveOffsets.Base + CodeCaveOffsets.Hit;
        var code = CodeCaveOffsets.Base + CodeCaveOffsets.EnvKilling;
        
        AsmHelper.WriteRelativeOffsets(bytes, [
        (code + 0x15, WorldChrMan.Base, 7, 0x15 + 3),
        (code + 0x4E, Functions.ChrInsByHandle, 5, 0x4E + 1),
        (code + 0x66, hit, 6, 0x66 + 2),
        (code + 0x70, Hooks.EnvKilling + 6, 5, 0x70 + 1),
        ]);
        
        memoryService.WriteBytes(code, bytes);
        hookManager.InstallHook(code, Hooks.EnvKilling, [0xF3, 0x0F, 0x11, 0x4C, 0x24, 0x28]);
        
    }
}