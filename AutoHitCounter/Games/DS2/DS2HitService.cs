// 

using System.Collections.Generic;
using System.Linq;
using AutoHitCounter.Enums;
using AutoHitCounter.Interfaces;
using AutoHitCounter.Memory;
using AutoHitCounter.Utilities;
using static AutoHitCounter.Games.DS2.DS2CustomCodeOffsets;
using static AutoHitCounter.Games.DS2.DS2Offsets;

namespace AutoHitCounter.Games.DS2;

public class DS2HitService(IMemoryService memoryService, HookManager hookManager) : IHitService
{
    private int _lastHitCount;
    
    private readonly List<nint> _hooks = [];

    public void InstallHooks()
    {
        if (IsScholar) InstallScholarHooks();
        else InstallVanillaHooks();
    }

    public bool HasHit()
    {
        var current = memoryService.Read<int>(Base + Hit);
        var newHits = current - _lastHitCount;
        _lastHitCount = current;
        return newHits > 0;
    }

    public void SetIsShulvaSpikesIgnored(bool isEnabled) =>
        memoryService.Write(Base + ShouldIgnoreShulvaSpikesFlag, isEnabled);
    
    public void ResetFlags()
    {
        memoryService.Write(Base + WetPoisonFlag, false);
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
    
    public void ClearHooks() => _hooks.Clear();

    #region Scholar

    private void InstallScholarHooks()
    {
        WriteHelpers();
        

        InstallScholarHitHook();
        InstallScholarGeneralDamageHook();
        InstallScholarKillBoxHook();
        InstallScholarCountAuxHook();
        InstallClearWetPoisonBitHook();
        InstallStaggerCheckHook();
    }

    private void WriteHelpers()
    {
        WriteScholarPlayerDeadCheck();
        WriteScholarHasIframesCheck();
    }

    private void WriteScholarPlayerDeadCheck()
    {
        var code = Base + CheckPlayerDead;
        var bytes = AsmLoader.GetAsmBytes(AsmScript.ScholarCheckPlayerDead);
        AsmHelper.WriteRelativeOffset(bytes, code, GameManagerImp.Base, 7, 3);
        memoryService.WriteBytes(code, bytes);
    }

    private void WriteScholarHasIframesCheck()
    {
        var code = Base + HasIframes;
        var bytes = AsmLoader.GetAsmBytes(AsmScript.ScholarHasIframes);
        memoryService.WriteBytes(code, bytes);
    }

    private void InstallScholarHitHook()
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.ScholarHit);

        var hit = Base + Hit;
        var auxCheckFlag = Base + CheckAuxProcFlag;
        var shouldIgnoreShulvaSpikesFlag = Base + ShouldIgnoreShulvaSpikesFlag;
        var wetPoisonFlag = Base + WetPoisonFlag;
        var checkPlayerDeadFunc = Base + CheckPlayerDead;
        var hasIframesFunc = Base + HasIframes;

        var code = Base + HitCode;

        AsmHelper.WriteRelativeOffsets(bytes, [
            (code, auxCheckFlag, 7, 2),
            (code + 0x8, checkPlayerDeadFunc, 5, 0x8 + 1),
            (code + 0x22, GameManagerImp.Base, 7, 0x22 + 3),
            (code + 0x48, hasIframesFunc, 5, 0x48 + 1),
            (code + 0x5E, MapId, 10, 0x5E + 2),
            (code + 0x6A, shouldIgnoreShulvaSpikesFlag, 7, 0x6A + 2),
            (code + 0x9D, wetPoisonFlag, 7, 0x9D + 2),
            (code + 0xA6, auxCheckFlag, 7, 0xA6 + 2),
            (code + 0xC2, hit, 6, 0xC2 + 2),
            (code + 0xCF, Hooks.Hit + 5, 5, 0xCF + 1)
        ]);

        memoryService.WriteBytes(code, bytes);
        InstallHook(code, Hooks.Hit, [0x48, 0x89, 0x5C, 0x24, 0x10]);
    }
    
    //Handles fall damage and self aux kill

    private void InstallScholarGeneralDamageHook()
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.ScholarGeneralApplyDamage);
        var hit = Base + Hit;
        var code = Base + GeneralApplyDamage;
        AsmHelper.WriteRelativeOffsets(bytes, [
            (code + 0x7, GameManagerImp.Base, 7, 0x7 + 3),
            (code + 0x1F, hit, 6, 0x1F + 2),
            (code + 0x26, Hooks.GeneralApplyDamage + 6, 5, 0x26 + 1)
        ]);

        memoryService.WriteBytes(code, bytes);
        InstallHook(code, Hooks.GeneralApplyDamage, [0x8B, 0x8B, 0x68, 0x01, 0x00, 0x00]);
    }

    private void InstallScholarKillBoxHook()
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.ScholarKillBox);
        var hit = Base + Hit;
        var code = Base + KillBox;
        AsmHelper.WriteRelativeOffsets(bytes, [
            (code + 0x4, GameManagerImp.Base, 7, 0x4 + 3),
            (code + 0x18, hit, 6, 0x18 + 2),
            (code + 0x23, Hooks.KillBox + 7, 5, 0x23 + 1)
        ]);

        memoryService.WriteBytes(code, bytes);
        InstallHook(code, Hooks.KillBox, [0x4C, 0x8B, 0x0, 0x89, 0x54, 0x24, 0x10]);
    }

    private void InstallScholarCountAuxHook()
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.ScholarCountAuxHit);

        var hit = Base + Hit;
        var wetPoisonFlag = Base + WetPoisonFlag;
        var auxCheckFlag = Base + CheckAuxProcFlag;

        var code = Base + CountAuxHit;

        AsmHelper.WriteRelativeOffsets(bytes, [
            (code + 0x6, auxCheckFlag, 7, 0x6 + 2),
            (code + 0xF, auxCheckFlag, 7, 0xF + 2),
            (code + 0x16, hit, 6, 0x16 + 2),
            (code + 0x1E, wetPoisonFlag, 7, 0x1E + 2),
            (code + 0x2C, wetPoisonFlag, 7, 0x2C + 2),
            (code + 0x33, hit, 6, 0x33 + 2),
            (code + 0x39, Hooks.CountAuxHit + 6, 5, 0x39 + 1)
        ]);

        memoryService.WriteBytes(code, bytes);
        InstallHook(code, Hooks.CountAuxHit, [0x4C, 0x8B, 0x01, 0x48, 0x63, 0xC2]);
    }

    private void InstallClearWetPoisonBitHook()
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.ScholarClearWetPoisonBit);
        var wetPoisonFlag = Base + WetPoisonFlag;
        var code = Base + ClearWetPoisonBit;

        AsmHelper.WriteRelativeOffsets(bytes, [
            (code + 0x7, wetPoisonFlag, 7, 0x7 + 2),
            (code + 0x11, GameManagerImp.Base, 7, 0x11 + 3),
            (code + 0x3B, wetPoisonFlag, 7, 0x3B + 2),
            (code + 0x43, Hooks.ClearWetPoisonBit + 7, 5, 0x43 + 1)
        ]);

        memoryService.WriteBytes(code, bytes);
        InstallHook(code, Hooks.ClearWetPoisonBit, [0x20, 0x84, 0x2A, 0x20, 0x01, 0x00, 0x00]);
    }

    private void InstallStaggerCheckHook()
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.ScholarStaggerCheck);
        var hit = Base + Hit;
        var code = Base + StaggerCheck;

        AsmHelper.WriteRelativeOffsets(bytes, [
            (code + 0xE, GameManagerImp.Base, 7, 0xE + 3),
            (code + 0x33, hit, 6, 0x33 + 2),
            (code + 0x3A, Hooks.StaggerCheck + 8, 5, 0x3A + 1)
        ]);

        memoryService.WriteBytes(code, bytes);
        InstallHook(code, Hooks.StaggerCheck, [0x41, 0x88, 0x4E, 0x17, 0x48, 0x8B, 0x47, 0x08]);
    }

    #endregion

    #region Vanilla

    private void InstallVanillaHooks()
    {
        WriteVanillaPlayerDeadCheck();
        WriteVanillaHasIframesCheck();

        InstallVanillaHitHook();
        InstallVanillaCountAuxHook();
        InstallVanillaGeneralDamageHook();
        InstallVanillaKillBoxHook();
        InstallVanillaClearWetPoisonHook();
        InstallVanillaStaggerCheckHook();
    }

    private void WriteVanillaPlayerDeadCheck()
    {
        var code = Base + CheckPlayerDead;

        var bytes = AsmLoader.GetAsmBytes(AsmScript.VanillaCheckPlayerDead);
        AsmHelper.WriteImmediateDword(bytes, (int)GameManagerImp.Base, 1);
        memoryService.WriteBytes(code, bytes);
    }

    private void WriteVanillaHasIframesCheck()
    {
        var code = Base + HasIframes;
        var bytes = AsmLoader.GetAsmBytes(AsmScript.VanillaHasIframes);
        memoryService.WriteBytes(code, bytes);
    }

    private void InstallVanillaHitHook()
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.VanillaHit);

        var hit = Base + Hit;
        var auxCheckFlag = Base + CheckAuxProcFlag;
        var wetPoisonFlag = Base + WetPoisonFlag;
        var shouldIgnoreShulvaSpikesFlag = Base + ShouldIgnoreShulvaSpikesFlag;
        var checkPlayerDeadFunc = Base + CheckPlayerDead;
        var hasIframesFunc = Base + HasIframes;
        
        var code = Base + HitCode;

        AsmHelper.WriteImmediateDwords(bytes, [
            ((int)auxCheckFlag, 2),
            ((int)GameManagerImp.Base, 0x24 + 2),
            ((int)MapId, 0x61 + 2),
            ((int)shouldIgnoreShulvaSpikesFlag, 0x6D + 2),
            ((int)wetPoisonFlag, 0xA9 + 2),
            ((int)auxCheckFlag, 0xB2 + 2),
            ((int)hit, 0xD3 + 2)
        ]);


        AsmHelper.WriteRelativeOffsets(bytes, [
            (code + 0x8, checkPlayerDeadFunc, 5, 0x8 + 1),
            (code + 0x45, hasIframesFunc, 5, 0x45 + 1),
            (code + 0xE1, Hooks.Hit + 6, 5, 0xE1 + 1)
        ]);

        memoryService.WriteBytes(code, bytes);
        InstallHook(code, Hooks.Hit, [0x55, 0x89, 0xE5, 0x83, 0xEC, 0x08]);
    }

    private void InstallVanillaCountAuxHook()
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.VanillaCountAuxHit);

        var hit = Base + Hit;
        var auxCheckFlag = Base + CheckAuxProcFlag;
        var wetPoisonFlag = Base + WetPoisonFlag;
        var code = Base + CountAuxHit;

        AsmHelper.WriteImmediateDwords(bytes, [
            ((int)auxCheckFlag, 0x5 + 2),
            ((int)auxCheckFlag, 0xE + 2),
            ((int)hit, 0x15 + 2),
            ((int)wetPoisonFlag, 0x1D + 2),
            ((int)wetPoisonFlag, 0x2B + 2),
            ((int)hit, 0x32 + 2)
        ]);

        AsmHelper.WriteRelativeOffset(bytes, code + 0x38, Hooks.CountAuxHit + 5, 5, 0x38 + 1);

        memoryService.WriteBytes(code, bytes);
        InstallHook(code, Hooks.CountAuxHit, [0xF3, 0x0F, 0x10, 0x55, 0x0C]);
    }

    private void InstallVanillaGeneralDamageHook()
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.VanillaGeneralApplyDamage);
        var hit = Base + Hit;
        var code = Base + GeneralApplyDamage;

        AsmHelper.WriteImmediateDwords(bytes, [
            ((int)GameManagerImp.Base, 0x7 + 2),
            ((int)hit, 0x1E + 2),
        ]);

        AsmHelper.WriteRelativeOffset(bytes, code + 0x25, Hooks.GeneralApplyDamage + 6, 5, 0x25 + 1);
        
        memoryService.WriteBytes(code, bytes);
        InstallHook(code, Hooks.GeneralApplyDamage, [0x8B, 0x86, 0xFC, 0x00, 0x00, 0x00]);
    }

    private void InstallVanillaKillBoxHook()
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.VanillaKillBox);
        var hit = Base + Hit;
        var code = Base + KillBox;
        
        AsmHelper.WriteImmediateDwords(bytes, [
            ((int)GameManagerImp.Base, 0x6 + 2),
            ((int)hit, 0x19 + 2),
        ]);

        AsmHelper.WriteRelativeOffset(bytes, code + 0x20, Hooks.KillBox + 5, 5, 0x20 + 1);

        memoryService.WriteBytes(code, bytes);
        InstallHook(code, Hooks.KillBox, [0x8B, 0x01, 0x8B, 0x4D, 0x08]);
    }

    
    private void InstallVanillaClearWetPoisonHook()
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.VanillaClearWetPoisonBit);
        var wetPoisonFlag = Base + WetPoisonFlag;
        var code = Base + ClearWetPoisonBit;
        
        AsmHelper.WriteImmediateDwords(bytes, [
        ((int)wetPoisonFlag, 0x7 + 2),
        ((int)GameManagerImp.Base, 0x11 + 1),
        ((int)wetPoisonFlag, 0x30 + 2),
        ]);

        AsmHelper.WriteRelativeOffset(bytes, code + 0x38, Hooks.ClearWetPoisonBit + 7, 5, 0x38 + 1);

        memoryService.WriteBytes(code, bytes);
        InstallHook(code, Hooks.ClearWetPoisonBit, [0x8D, 0x94, 0x1A, 0x10, 0x01, 0x00, 0x00]);
    }

    private void InstallVanillaStaggerCheckHook()
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.VanillaStaggerCheck);
        var hit = Base + Hit;
        var code = Base + StaggerCheck;

        AsmHelper.WriteImmediateDwords(bytes, [
            ((int)GameManagerImp.Base, 0xC + 1),
            ((int)hit, 0x2A + 2),
        ]);

        AsmHelper.WriteRelativeOffset(bytes, code + 0x31, Hooks.StaggerCheck + 6, 5, 0x31 + 1);


        memoryService.WriteBytes(code, bytes);
        InstallHook(code, Hooks.StaggerCheck, [0x88, 0x4F, 0x17, 0x8B, 0x56, 0x10]);
    }

    #endregion

    
}