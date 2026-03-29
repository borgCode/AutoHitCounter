// 

using AutoHitCounter.Enums;
using AutoHitCounter.Interfaces;
using AutoHitCounter.Memory;
using AutoHitCounter.Utilities;
using static AutoHitCounter.Games.SK.SKCustomCodeOffsets;
using static AutoHitCounter.Games.SK.SKOffsets;

namespace AutoHitCounter.Games.SK;

public class SKHitService(IMemoryService memoryService, HookManager hookManager) : IHitService
{
    private int _lastHitCount;

    public void InstallHooks()
    {
        WritePlayerDeadCheck();

        InstallHitHook();
        InstallPostHitHook();
        InstallLethalFall();
        InstallFadeFall();
        InstallApplyHealthDeltaHook();
        InstallStaggerIgnoreCheck();
        InstallAuxProcHook();
        InstallCheckAuxAttackerHook();
        InstallHkbFireEventHook();
        InstallFadeFallHeightHook();
        InstallDeferredFallCheckHook();
        InstallApplySpEffectDamageHook();
        InstallSakuraDanceHook();
    }

    public bool HasHit()
    {
        var current = memoryService.Read<int>(Base + Hit);
        var newHits = current - _lastHitCount;
        _lastHitCount = current;
        return newHits > 0;
    }

    public void SetRobertoStaggerCounts(bool isEnabled) =>
        memoryService.Write(Base + ShouldCountRobertoStagger, isEnabled);

    private void WritePlayerDeadCheck()
    {
        var code = Base + CheckPlayerDead;

        var bytes = AsmLoader.GetAsmBytes(AsmScript.SKCheckPlayerDead);
        AsmHelper.WriteRelativeOffset(bytes, code, WorldChrMan.Base, 7, 3);
        memoryService.WriteBytes(code, bytes);
    }

    private void InstallHitHook()
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.SKHit);
        var pendingHitFlag = Base + PendingHitFlag;
        var staggerCheckFlag = Base + StaggerCheckFlag;
        var shouldCountRoberto = Base + ShouldCountRobertoStagger;
        var pureLightningFlag = Base + PureLightningFlag;
        var hit = Base + Hit;
        var checkPlayerDeadFunc = Base + CheckPlayerDead;
        var code = Base + HitCode;


        AsmHelper.WriteRelativeOffsets(bytes, [
            (code, pendingHitFlag, 7, 2),
            (code + 0xD, checkPlayerDeadFunc, 5, 0xD + 1),
            (code + 0x18, WorldChrMan.Base, 7, 0x18 + 3),
            (code + 0x4D, shouldCountRoberto, 7, 0x4D + 2),
            (code + 0x87, WorldChrMan.Base, 7, 0x87 + 3),
            (code + 0x9C, Functions.HasSpEffectId, 5, 0x9C + 1),
            (code + 0x114, WorldChrMan.Base, 7, 0x114 + 3),
            (code + 0x12E, Functions.HasSpEffectId, 5, 0x12E + 1),
            (code + 0x13D, staggerCheckFlag, 7, 0x13D + 2),
            (code + 0x14A, EventFlagMan.Base, 7, 0x14A + 3),
            (code + 0x156, Functions.GetEvent, 5, 0x156 + 1),
            (code + 0x163, hit, 6, 0x163 + 2),
            (code + 0x16D, WorldChrMan.Base, 7, 0x16D + 3),
            (code + 0x187, Functions.HasSpEffectId, 5, 0x187 + 1),
            (code + 0x1AD, pureLightningFlag, 7, 0x1AD + 2),
            (code + 0x1B6, pendingHitFlag, 7, 0x1B6 + 2),
            (code + 0x1BE, Hooks.Hit + 5, 5, 0x1BE + 1),
        ]);
        memoryService.WriteBytes(code, bytes);
        hookManager.InstallHook(code, Hooks.Hit, [0x48, 0x89, 0x44, 0x24, 0x50]);
    }

    private void InstallPostHitHook()
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.SKPostHit);
        var pendingHitFlag = Base + PendingHitFlag;
        var staggerCheckFlag = Base + StaggerCheckFlag;
        var hit = Base + Hit;
        var code = Base + PostHit;

        AsmHelper.WriteRelativeOffsets(bytes, [
            (code + 0x1, pendingHitFlag, 7, 0x1 + 2),
            (code + 0x36, WorldChrMan.Base, 7, 0x36 + 3),
            (code + 0x50, Functions.HasSpEffectId, 5, 0x50 + 1),
            (code + 0x5B, staggerCheckFlag, 7, 0x5B + 2),
            (code + 0x64, pendingHitFlag, 7, 0x64 + 2),
            (code + 0x6B, hit, 6, 0x6B + 2),
            (code + 0x73, pendingHitFlag, 7, 0x73 + 2),
            (code + 0x82, Hooks.PostHit + 7, 5, 0x82 + 1),
        ]);

        memoryService.WriteBytes(code, bytes);
        hookManager.InstallHook(code, Hooks.PostHit, [0x48, 0x81, 0xC4, 0x18, 0x01, 0x00, 0x00]);
    }

    private void InstallLethalFall()
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.SKLethalFall);
        var hit = Base + Hit;
        var code = Base + LethalFall;

        AsmHelper.WriteRelativeOffsets(bytes, [
            (code + 0x1, WorldChrMan.Base, 7, 0x1 + 3),
            (code + 0x16, hit, 6, 0x16 + 2),
            (code + 0x24, Hooks.LethalFall + 7, 5, 0x24 + 1),
        ]);

        memoryService.WriteBytes(code, bytes);
        hookManager.InstallHook(code, Hooks.LethalFall, [0x48, 0x8B, 0x88, 0xF8, 0x1F, 0x00, 0x00]);
    }

    private void InstallFadeFall()
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.SKFadeFall);
        var hit = Base + Hit;
        var code = Base + FadeFall;

        AsmHelper.WriteRelativeOffsets(bytes, [
            (code + 0x1, WorldChrMan.Base, 7, 0x1 + 3),
            (code + 0x16, hit, 6, 0x16 + 2),
            (code + 0x24, Hooks.FadeFall + 7, 5, 0x24 + 1),
        ]);

        memoryService.WriteBytes(code, bytes);
        hookManager.InstallHook(code, Hooks.FadeFall, [0x48, 0x8B, 0x88, 0xF8, 0x1F, 0x00, 0x00]);
    }

    private void InstallApplyHealthDeltaHook()
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.SKApplyHealthDelta);
        var hit = Base + Hit;
        var deferredFallCheckFlag = Base + DeferredFallCheckFlag;
        var code = Base + ApplyHealthDelta;

        AsmHelper.WriteRelativeOffsets(bytes, [
            (code, deferredFallCheckFlag, 7, 2),
            (code + 0x9, WorldChrMan.Base, 7, 0x9 + 3),
            (code + 0x44, deferredFallCheckFlag, 7, 0x44 + 2),
            (code + 0x5D, hit, 6, 0x5D + 2),
            (code + 0x6A, Hooks.ApplyHealthDelta + 5, 5, 0x6A + 1),
        ]);

        AsmHelper.WriteAbsoluteAddress(bytes, FallDmgRetAddr, 0x33 + 2);

        memoryService.WriteBytes(code, bytes);
        hookManager.InstallHook(code, Hooks.ApplyHealthDelta, [0x48, 0x89, 0x5C, 0x24, 0x08]);
    }

    private void InstallStaggerIgnoreCheck()
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.SKStaggerIgnoreCheck);
        var staggerCheckFlag = Base + StaggerCheckFlag;
        var hit = Base + Hit;
        var code = Base + StaggerCheck;

        AsmHelper.WriteRelativeOffsets(bytes, [
            (code, staggerCheckFlag, 7, 2),
            (code + 0x16, hit, 6, 0x16 + 2),
            (code + 0x1C, staggerCheckFlag, 7, 0x1C + 2),
            (code + 0x29, Hooks.StaggerIgnoreCheck + 6, 5, 0x29 + 1),
        ]);

        memoryService.WriteBytes(code, bytes);
        hookManager.InstallHook(code, Hooks.StaggerIgnoreCheck, [0x45, 0x0F, 0x57, 0xC0, 0x85, 0xC0]);
    }

    private void InstallAuxProcHook()
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.SKAuxProc);
        var checkAuxFlag = Base + CheckAuxFlag;
        var hit = Base + Hit;
        var code = Base + CheckAux;

        AsmHelper.WriteRelativeOffsets(bytes, [
            (code + 0x6, checkAuxFlag, 7, 0x6 + 2),
            (code + 0xF, hit, 6, 0xF + 2),
            (code + 0x15, Hooks.AuxProc + 6, 5, 0x15 + 1),
        ]);

        memoryService.WriteBytes(code, bytes);
        hookManager.InstallHook(code, Hooks.AuxProc, [0xD3, 0xE0, 0x41, 0x09, 0x41, 0x4C]);
    }

    private void InstallCheckAuxAttackerHook()
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.SKCheckAuxAttacker);
        var checkAuxFlag = Base + CheckAuxFlag;
        var code = Base + CheckAuxAttacker;

        AsmHelper.WriteRelativeOffsets(bytes, [
            (code + 0x8, WorldChrMan.Base, 7, 0x8 + 3),
            (code + 0x25, checkAuxFlag, 7, 0x25 + 2),
            (code + 0x2E, checkAuxFlag, 7, 0x2E + 2),
            (code + 0x36, Hooks.CheckAuxAttacker + 7, 5, 0x36 + 1),
        ]);

        memoryService.WriteBytes(code, bytes);
        hookManager.InstallHook(code, Hooks.CheckAuxAttacker, [0x49, 0x8B, 0x5D, 0x00, 0x4C, 0x89, 0xE9]);
    }

    private void InstallHkbFireEventHook()
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.SKHkbFireEvent);
        var hit = Base + Hit;
        var checkPlayerDeadFunc = Base + CheckPlayerDead;
        var code = Base + HkbFireEvent;


        AsmHelper.WriteRelativeOffsets(bytes, [
            (code + 0x1, checkPlayerDeadFunc, 5, 0x1 + 1),
            (code + 0x9, WorldChrMan.Base, 7, 0x9 + 3),
            (code + 0x29, hit, 6, 0x29 + 2),
            (code + 0x38, Hooks.HkbFireEvent + 7, 5, 0x38 + 1),
        ]);
        memoryService.WriteBytes(code, bytes);
        hookManager.InstallHook(code, Hooks.HkbFireEvent, [0x48, 0x8B, 0x5F, 0x20, 0x48, 0x85, 0xDB]);
    }

    private void InstallFadeFallHeightHook()
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.SKFadeFallHeight);
        var hit = Base + Hit;
        var code = Base + FadeFallHeight;
        
        AsmHelper.WriteRelativeOffsets(bytes, [
            (code + 0x1, WorldChrMan.Base, 7, 0x1 + 3),
            (code + 0x16, hit, 6, 0x16 + 2),
            (code + 0x24, Hooks.FadeFallHeight + 7, 5, 0x24 + 1),
        ]);
        memoryService.WriteBytes(code, bytes);
        hookManager.InstallHook(code, Hooks.FadeFallHeight, [0x48, 0x8B, 0x88, 0xF8, 0x1F, 0x00, 0x00]);
    }

    private void InstallDeferredFallCheckHook()
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.SKDeferredFallCheck);
        var hit = Base + Hit;
        var deferredFallCheckFlag = Base + DeferredFallCheckFlag;
        var code = Base + DeferredFallCheck;

        AsmHelper.WriteRelativeOffsets(bytes, [
            (code + 0x7, deferredFallCheckFlag, 7, 0x7 + 2),
            (code + 0x10, hit, 6, 0x10 + 2),
            (code + 0x16, Hooks.DeferredFallCheck + 7, 5, 0x16 + 1),
        ]);
        
        memoryService.WriteBytes(code, bytes);
        hookManager.InstallHook(code, Hooks.DeferredFallCheck, [0x48, 0x8B, 0x06, 0x41, 0x0F, 0x28, 0xC9]);
    }

    private void InstallApplySpEffectDamageHook()
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.SKApplySpEffectDamage);
        var hit = Base + Hit;
        var code = Base + ApplySpEffectDamage;
        
        AsmHelper.WriteRelativeOffsets(bytes, [
            (code + 0x3, WorldChrMan.Base, 7, 0x3 + 3),
            (code + 0x45, Functions.HasSpEffectId, 5, 0x45 + 1),
            (code + 0x56, Functions.HasSpEffectId, 5, 0x56 + 1),
            (code + 0x67, Functions.HasSpEffectId, 5, 0x67 + 1),
            (code + 0x78, Functions.HasSpEffectId, 5, 0x78 + 1),
            (code + 0x89, Functions.HasSpEffectId, 5, 0x89 + 1),
            (code + 0x9A, Functions.HasSpEffectId, 5, 0x9A + 1),
            (code + 0xA3, GameDataMan.Base, 7, 0xA3 + 3),
            (code + 0xB6, hit, 6, 0xB6 + 2),
            (code + 0xC7, Hooks.ApplySpEffectDamage + 7, 5, 0xC7 + 1),
        ]);
        
        memoryService.WriteBytes(code, bytes);
        hookManager.InstallHook(code, Hooks.ApplySpEffectDamage, [0x48, 0x8B, 0x88, 0xF8, 0x1F, 0x00, 0x00]);
    }

    private void InstallSakuraDanceHook()
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.SKSakuraDance);
        var hit = Base + Hit;
        var pureLightningFlag = Base + PureLightningFlag;
        var code = Base + SakuraDance;
        
        AsmHelper.WriteRelativeOffsets(bytes, [
        (code + 0x8, pureLightningFlag, 7, 0x8 + 2),
        (code + 0x11, pureLightningFlag, 7, 0x11 + 2),
        (code + 0x26, WorldChrMan.Base, 7, 0x26 + 3),
        (code + 0x3B, Functions.HasSpEffectId, 5, 0x3B + 1),
        (code + 0x47, hit, 6, 0x47 + 2),
        (code + 0x4D, Hooks.SakuraDance  + 8, 5, 0x4D + 1),
        ]);
        
        memoryService.WriteBytes(code, bytes);
        hookManager.InstallHook(code, Hooks.SakuraDance, [0xF3, 0x0F, 0x10, 0xA5, 0x10, 0x02, 0x00, 0x00]);
    }
}