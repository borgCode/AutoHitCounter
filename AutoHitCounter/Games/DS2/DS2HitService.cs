// 

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

    #region Scholar

    private void InstallScholarHooks()
    {
        WriteScholarPlayerDeadCheck();

        InstallScholarHitHook();
        InstallScholarGeneralDamageHook();
        InstallScholarKillBoxHook();
        InstallScholarCountAuxHook();
        InstallScholarLightPoiseStaggerHook();
    }

    private void InstallScholarHitHook()
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.ScholarHit);

        var hit = Base + Hit;
        var auxCheckFlag = Base + CheckAuxProcFlag;
        var shouldIgnoreShulvaSpikesFlag = Base + ShouldIgnoreShulvaSpikesFlag;
        var checkPlayerDeadFunc = Base + CheckPlayerDead;

        var code = Base + HitCode;

        AsmHelper.WriteRelativeOffsets(bytes, [
            (code, auxCheckFlag, 7, 2),
            (code + 0x8, checkPlayerDeadFunc, 5, 0x8 + 1),
            (code + 0x1A, GameManagerImp.Base, 7, 0x1A + 3),
            (code + 0x38, MapId, 10, 0x38 + 2),
            (code + 0x44, shouldIgnoreShulvaSpikesFlag, 7, 0x44 + 2),
            (code + 0x6D, auxCheckFlag, 7, 0x6D + 2),
            (code + 0x7B, hit, 6, 0x7B + 2),
            (code + 0x88, Hooks.Hit + 5, 5, 0x88 + 1)
        ]);

        memoryService.WriteBytes(code, bytes);
        hookManager.InstallHook(code, Hooks.Hit, [0x48, 0x89, 0x5C, 0x24, 0x10]);
    }

    private void WriteScholarPlayerDeadCheck()
    {
        var code = Base + CheckPlayerDead;

        var bytes = AsmLoader.GetAsmBytes(AsmScript.ScholarCheckPlayerDead);
        AsmHelper.WriteRelativeOffset(bytes, code, GameManagerImp.Base, 7, 3);
        memoryService.WriteBytes(code, bytes);
    }

    //Handles fall damage and self aux kill

    private void InstallScholarGeneralDamageHook()
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.ScholarGeneralApplyDamage);
        var hit = Base + Hit;
        var code = Base + GeneralApplyDamage;
        AsmHelper.WriteRelativeOffsets(bytes, [
            (code + 0x1, GameManagerImp.Base, 7, 0x1 + 3),
            (code + 0x15, hit, 6, 0x15 + 2),
            (code + 0x22, Hooks.GeneralApplyDamage + 6, 5, 0x22 + 1)
        ]);

        memoryService.WriteBytes(code, bytes);
        hookManager.InstallHook(code, Hooks.GeneralApplyDamage, [0x89, 0x83, 0x68, 0x01, 0x00, 0x00]);
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
        hookManager.InstallHook(code, Hooks.KillBox, [0x4C, 0x8B, 0x0, 0x89, 0x54, 0x24, 0x10]);
    }

    private void InstallScholarCountAuxHook()
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.ScholarCountAuxHit);

        var hit = Base + Hit;
        var auxCheckFlag = Base + CheckAuxProcFlag;

        var code = Base + CountAuxHit;

        AsmHelper.WriteRelativeOffsets(bytes, [
            (code + 0x6, auxCheckFlag, 7, 0x6 + 2),
            (code + 0xF, auxCheckFlag, 7, 0xF + 2),
            (code + 0x16, hit, 6, 0x16 + 2),
            (code + 0x1C, Hooks.CountAuxHit + 6, 5, 0x1C + 1)
        ]);

        memoryService.WriteBytes(code, bytes);
        hookManager.InstallHook(code, Hooks.CountAuxHit, [0x4C, 0x8B, 0x01, 0x48, 0x63, 0xC2]);
    }

    private void InstallScholarLightPoiseStaggerHook()
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.ScholarLightPoiseStagger);
        var hit = Base + Hit;
        var code = Base + LightPoiseStagger;

        AsmHelper.WriteRelativeOffsets(bytes, [
            (code + 0x6, GameManagerImp.Base, 7, 0x6 + 3),
            (code + 0x19, hit, 6, 0x19 + 2),
            (code + 0x26, Hooks.LightPoiseStagger + 5, 5, 0x26 + 1)
        ]);

        memoryService.WriteBytes(code, bytes);
        hookManager.InstallHook(code, Hooks.LightPoiseStagger, [0x8B, 0x42, 0x04, 0x89, 0x02]);
    }

    #endregion

    #region Vanilla

    private void InstallVanillaHooks()
    {
        WriteVanillaPlayerDeadCheck();

        InstallVanillaHitHook();
        InstallVanillaCountAuxHook();
        InstallVanillaGeneralDamageHook();
        InstallVanillaKillBoxHook();
        InstallVanillaLightPoiseStaggerHook();
    }

    private void WriteVanillaPlayerDeadCheck()
    {
        var code = Base + CheckPlayerDead;

        var bytes = AsmLoader.GetAsmBytes(AsmScript.VanillaCheckPlayerDead);
        AsmHelper.WriteImmediateDword(bytes, (int)GameManagerImp.Base, 1);
        memoryService.WriteBytes(code, bytes);
    }

    private void InstallVanillaHitHook()
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.VanillaHit);

        var hit = Base + Hit;
        var auxCheckFlag = Base + CheckAuxProcFlag;
        var shouldIgnoreShulvaSpikesFlag = Base + ShouldIgnoreShulvaSpikesFlag;
        var checkPlayerDeadFunc = Base + CheckPlayerDead;

        var code = Base + HitCode;

        AsmHelper.WriteImmediateDwords(bytes, [
            ((int)auxCheckFlag, 2),
            ((int)GameManagerImp.Base, 0x1C + 2),
            ((int)MapId, 0x36 + 2),
            ((int)shouldIgnoreShulvaSpikesFlag, 0x42 + 2),
            ((int)auxCheckFlag, 0x72 + 2),
            ((int)hit, 0x83 + 2)
        ]);


        AsmHelper.WriteRelativeOffsets(bytes, [
            (code + 0x8, checkPlayerDeadFunc, 5, 0x8 + 1),
            (code + 0x91, Hooks.Hit + 6, 5, 0x91 + 1)
        ]);

        memoryService.WriteBytes(code, bytes);
        hookManager.InstallHook(code, Hooks.Hit, [0x55, 0x89, 0xE5, 0x83, 0xEC, 0x08]);
    }

    private void InstallVanillaCountAuxHook()
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.VanillaCountAuxHit);

        var hit = Base + Hit;
        var auxCheckFlag = Base + CheckAuxProcFlag;

        var code = Base + CountAuxHit;

        AsmHelper.WriteImmediateDwords(bytes, [
            ((int)auxCheckFlag, 0x5 + 2),
            ((int)auxCheckFlag, 0xE + 2),
            ((int)hit, 0x15 + 2)
        ]);

        AsmHelper.WriteRelativeOffset(bytes, code + 0x1B, Hooks.CountAuxHit + 5, 5, 0x1B + 1);

        memoryService.WriteBytes(code, bytes);
        hookManager.InstallHook(code, Hooks.CountAuxHit, [0xF3, 0x0F, 0x10, 0x55, 0x0C]);
    }

    private void InstallVanillaGeneralDamageHook()
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.VanillaGeneralApplyDamage);
        var hit = Base + Hit;
        var code = Base + GeneralApplyDamage;

        AsmHelper.WriteImmediateDwords(bytes, [
            ((int)GameManagerImp.Base, 0x1 + 1),
            ((int)hit, 0x13 + 2),
        ]);

        AsmHelper.WriteRelativeOffset(bytes, code + 0x20, Hooks.GeneralApplyDamage + 6, 5, 0x20 + 1);
        
        memoryService.WriteBytes(code, bytes);
        hookManager.InstallHook(code, Hooks.GeneralApplyDamage, [0x89, 0x8E, 0xFC, 0x00, 0x00, 0x00]);
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
        hookManager.InstallHook(code, Hooks.KillBox, [0x8B, 0x01, 0x8B, 0x4D, 0x08]);
    }
    
    private void InstallVanillaLightPoiseStaggerHook()
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.VanillaLightPoiseStagger);
        var hit = Base + Hit;
        var code = Base + LightPoiseStagger;

        AsmHelper.WriteImmediateDwords(bytes, [
            ((int)GameManagerImp.Base, 0x1 + 2),
            ((int)hit, 0x10 + 2),
        ]);

        AsmHelper.WriteRelativeOffset(bytes, code + 0x1D, Hooks.LightPoiseStagger + 6, 5, 0x1D + 1);


        memoryService.WriteBytes(code, bytes);
        hookManager.InstallHook(code, Hooks.LightPoiseStagger, [0xD9, 0x80, 0xB0, 0x01, 0x00, 0x00]);
    }

    #endregion
}