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

        var bytes = AsmLoader.GetAsmBytes(AsmScript.SKCheckPlayerDead);
        AsmHelper.WriteRelativeOffset(bytes, code, WorldChrMan.Base, 7, 3);
        memoryService.WriteBytes(code, bytes);
    }

    private void InstallHitHook()
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.SKHit);
        var pendingHitFlag = Base + PendingHitFlag;
        var staggerCheckFlag = Base + StaggerCheckFlag;
        var hit = Base + Hit;
        var checkPlayerDeadFunc = Base + CheckPlayerDead;
        var code = Base + HitCode;


        AsmHelper.WriteRelativeOffsets(bytes, [
            (code, pendingHitFlag, 7, 2),
            (code + 0xD, checkPlayerDeadFunc, 5, 0xD + 1),
            (code + 0x18, WorldChrMan.Base, 7, 0x18 + 3),
            (code + 0x62, WorldChrMan.Base, 7, 0x62 + 3),
            (code + 0x77, Functions.HasSpEffectId, 5, 0x77 + 1),
            (code + 0xE2, WorldChrMan.Base, 7, 0xE2 + 3),
            (code + 0xFC, Functions.HasSpEffectId, 5, 0xFC + 1),
            (code + 0x107, staggerCheckFlag, 7, 0x107 + 2),
            (code + 0x114, EventFlagMan.Base, 7, 0x114 + 3),
            (code + 0x120, Functions.GetEvent, 5, 0x120 + 1),
            (code + 0x12D, hit, 6, 0x12D + 2),
            (code + 0x137, WorldChrMan.Base, 7, 0x137 + 3),
            (code + 0x151, Functions.HasSpEffectId, 5, 0x151 + 1),
            (code + 0x15C, pendingHitFlag, 7, 0x15C + 2),
            (code + 0x164, Hooks.Hit + 5, 5, 0x164 + 1),
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
        var code = Base + ApplyHealthDelta;

        AsmHelper.WriteRelativeOffsets(bytes, [
            (code + 0x12, WorldChrMan.Base, 7, 0x12 + 3),
            (code + 0x4C, hit, 6, 0x4C + 2),
            (code + 0x58, Hooks.ApplyHealthDelta + 5, 5, 0x58 + 1),
        ]);

        AsmHelper.WriteAbsoluteAddress(bytes, FallDmgRetAddr, 0x1 + 2);

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
            (code + 0xD, hit, 6, 0xD + 2),
            (code + 0x13, staggerCheckFlag, 7, 0x13 + 2),
            (code + 0x20, Hooks.StaggerIgnoreCheck + 6, 5, 0x20 + 1),
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
        hookManager.InstallHook(code, Hooks.CheckAuxAttacker, [0x48, 0x81, 0xEC, 0xA0, 0x00, 0x00, 0x00]);
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
}