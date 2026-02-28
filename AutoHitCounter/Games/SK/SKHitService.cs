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
        InstallDeflectCheck();
        InstallLethalFall();
        InstallFadeFall();
        InstallApplyHealthDeltaHook();
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
        var checkDeflectFlag = Base + CheckDeflectFlag;
        var hit = Base + Hit;
        var checkPlayerDeadFunc = Base + CheckPlayerDead;
        var code = Base + HitCode;
        
        AsmHelper.WriteRelativeOffsets(bytes, [
            (code, checkDeflectFlag, 7, 2),
            (code + 0xD, checkPlayerDeadFunc, 5, 0xD + 1),
            (code + 0x14, WorldChrMan.Base, 7, 0x14 + 3),
            (code + 0x79, checkDeflectFlag, 7, 0x79 + 2),
            (code + 0x82, hit, 6, 0x82 + 2),
            (code + 0x89, Hooks.Hit + 5, 5, 0x89 + 1),
        ]);
        
        memoryService.WriteBytes(code, bytes);
        hookManager.InstallHook(code, Hooks.Hit, [0x48, 0x89, 0x44, 0x24, 0x50]);
    }

    private void InstallDeflectCheck()
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.SKDeflectCheck);
        var checkDeflectFlag = Base + CheckDeflectFlag;
        var hit = Base + Hit;
        var code = Base + DeflectCheck;
        
        AsmHelper.WriteRelativeOffsets(bytes, [
            (code + 0x7, checkDeflectFlag, 7, 0x7 + 2),
            (code + 0x1A, hit, 6, 0x1A + 2),
            (code + 0x20, Hooks.DidSuccessfulDeflect + 7, 5, 0x20 + 1),
        ]);
        
        memoryService.WriteBytes(code, bytes);
        hookManager.InstallHook(code, Hooks.DidSuccessfulDeflect, [0xC7, 0x45, 0xC0, 0xFF, 0xFF, 0xFF, 0xFF]);
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
        hookManager.InstallHook(code, Hooks.LethalFall, [ 0x48, 0x8B, 0x88, 0xF8, 0x1F, 0x00, 0x00]);
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
        hookManager.InstallHook(code, Hooks.FadeFall, [ 0x48, 0x8B, 0x88, 0xF8, 0x1F, 0x00, 0x00]);
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
        hookManager.InstallHook(code, Hooks.ApplyHealthDelta, [  0x48, 0x89, 0x5C, 0x24, 0x08]);
    }
}