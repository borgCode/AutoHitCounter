// 

using AutoHitCounter.Enums;
using AutoHitCounter.Interfaces;
using AutoHitCounter.Memory;
using AutoHitCounter.Utilities;
using static AutoHitCounter.Games.DS2S.DS2ScholarOffsets;

namespace AutoHitCounter.Games.DS2S;

public class DS2ScholarHitService(IMemoryService memoryService, HookManager hookManager) : IHitService
{
    private int _lastHitCount;

    public void InstallHooks()
    {
        InstallHitHook();
        InstallGeneralDamageHook();
        InstallKillBoxHook();
        InstallCountAuxHook();
        InstallLightPoiseStaggerHook();
    }

    public bool HasHit()
    {
        var current = memoryService.Read<int>(DS2ScholarCustomCodeOffsets.Base + DS2ScholarCustomCodeOffsets.Hit);
        var newHits = current - _lastHitCount;
        _lastHitCount = current;
        return newHits > 0;
    }

    private void InstallHitHook()
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.ScholarHit);

        var hit = DS2ScholarCustomCodeOffsets.Base + DS2ScholarCustomCodeOffsets.Hit;
        var auxCheckFlag = DS2ScholarCustomCodeOffsets.Base + DS2ScholarCustomCodeOffsets.CheckAuxProcFlag;

        var code = DS2ScholarCustomCodeOffsets.Base + DS2ScholarCustomCodeOffsets.HitCode;

        AsmHelper.WriteRelativeOffsets(bytes, [
            (code, auxCheckFlag, 7, 2),
            (code + 0x8, GameManagerImp.Base, 7, 0x8 + 3),
            (code + 0x22, auxCheckFlag, 7, 0x22 + 2),
            (code + 0x30, hit, 6, 0x30 + 2),
            (code + 0x3C, Hooks.Hit + 5, 5, 0x3C + 1)
        ]);

        memoryService.WriteBytes(code, bytes);
        hookManager.InstallHook(code, Hooks.Hit, [0x48, 0x89, 0x5C, 0x24, 0x10]);
    }

    //Handles fall damage and self aux kill

    private void InstallGeneralDamageHook()
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.ScholarGeneralApplyDamage);
        var hit = DS2ScholarCustomCodeOffsets.Base + DS2ScholarCustomCodeOffsets.Hit;
        var code = DS2ScholarCustomCodeOffsets.Base + DS2ScholarCustomCodeOffsets.GeneralApplyDamage;
        AsmHelper.WriteRelativeOffsets(bytes, [
            (code + 0x1, GameManagerImp.Base, 7, 0x1 + 3),
            (code + 0x15, hit, 6, 0x15 + 2),
            (code + 0x22, Hooks.GeneralApplyDamage + 6, 5, 0x22 + 1)
        ]);

        memoryService.WriteBytes(code, bytes);
        hookManager.InstallHook(code, Hooks.GeneralApplyDamage, [0x89, 0x83, 0x68, 0x01, 0x00, 0x00]);
    }

    private void InstallKillBoxHook()
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.ScholarKillBox);
        var hit = DS2ScholarCustomCodeOffsets.Base + DS2ScholarCustomCodeOffsets.Hit;
        var code = DS2ScholarCustomCodeOffsets.Base + DS2ScholarCustomCodeOffsets.KillBox;
        AsmHelper.WriteRelativeOffsets(bytes, [
            (code + 0x4, GameManagerImp.Base, 7, 0x4 + 3),
            (code + 0x18, hit, 6, 0x18 + 2),
            (code + 0x23, Hooks.KillBox + 7, 5, 0x23 + 1)
        ]);

        memoryService.WriteBytes(code, bytes);
        hookManager.InstallHook(code, Hooks.KillBox, [0x4C, 0x8B, 0x0, 0x89, 0x54, 0x24, 0x10]);
    }

    private void InstallCountAuxHook()
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.ScholarCountAuxHit);
        
        var hit = DS2ScholarCustomCodeOffsets.Base + DS2ScholarCustomCodeOffsets.Hit;
        var auxCheckFlag = DS2ScholarCustomCodeOffsets.Base + DS2ScholarCustomCodeOffsets.CheckAuxProcFlag;
        
        var code = DS2ScholarCustomCodeOffsets.Base + DS2ScholarCustomCodeOffsets.CountAuxHit;
        
        AsmHelper.WriteRelativeOffsets(bytes, [
            (code + 0x6, auxCheckFlag, 7, 0x6 + 2),
            (code + 0xF, auxCheckFlag, 7, 0xF + 2),
            (code + 0x16, hit, 6, 0x16 + 2),
            (code + 0x1C, Hooks.CountAuxHit + 6, 5, 0x1C + 1)
        ]);
        
        memoryService.WriteBytes(code, bytes);
        hookManager.InstallHook(code, Hooks.CountAuxHit, [0x4C, 0x8B, 0x01, 0x48, 0x63, 0xC2]);
    }

    private void InstallLightPoiseStaggerHook()
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.ScholarLightPoiseStagger);
        var hit = DS2ScholarCustomCodeOffsets.Base + DS2ScholarCustomCodeOffsets.Hit;
        var code = DS2ScholarCustomCodeOffsets.Base + DS2ScholarCustomCodeOffsets.LightPoiseStagger;
        
        AsmHelper.WriteRelativeOffsets(bytes, [
            (code + 0x6, GameManagerImp.Base, 7, 0x6 + 3),
            (code + 0x19, hit, 6, 0x19 + 2),
            (code + 0x26, Hooks.LightPoiseStagger + 5, 5, 0x26 + 1)
        ]);
        
        memoryService.WriteBytes(code, bytes);
        hookManager.InstallHook(code, Hooks.LightPoiseStagger, [0x8B, 0x42, 0x04, 0x89, 0x02]);
    }
}