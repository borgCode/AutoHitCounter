// 

using System.Linq;
using AutoHitCounter.Enums;
using AutoHitCounter.Interfaces;
using AutoHitCounter.Memory;
using AutoHitCounter.Utilities;
using static AutoHitCounter.Games.DS3.DS3CustomCodeOffsets;
using static AutoHitCounter.Games.DS3.DS3Offsets;

namespace AutoHitCounter.Games.DS3;

public class DS3HitService(IMemoryService memoryService, HookManager hookManager) : IHitService
{
    private int _lastHitCount;
    
    public void InstallHooks()
    {
        WritePlayerDeadCheck();

        InstallHitHook();
        InstallLethalFallHook();
        InstallAuxHitHooks();
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

        var bytes = AsmLoader.GetAsmBytes(AsmScript.DS3CheckPlayerDead);
        AsmHelper.WriteRelativeOffset(bytes, code, WorldChrMan.Base, 7, 3);
        memoryService.WriteBytes(code, bytes);
    }

    private void InstallHitHook()
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.DS3Hit);
        var hit = Base + Hit;
        var checkPlayerDeadFunc = Base + CheckPlayerDead;
        var code = Base + HitCode;
        
        AsmHelper.WriteRelativeOffsets(bytes, [
            (code + 0x1, checkPlayerDeadFunc, 5, 0x1 + 1),
            (code + 0x15, WorldChrMan.Base, 7, 0x15 + 3),
            (code + 0x71, hit, 6, 0x71 + 2),
            (code + 0x83, Hooks.Hit + 8, 5, 0x83 + 1),
        ]);
        
        memoryService.WriteBytes(code, bytes);
        hookManager.InstallHook(code, Hooks.Hit, [0x48, 0x83, 0xEC, 0x50, 0x48, 0x8B, 0x41, 0x08]);
    }

    private void InstallLethalFallHook()
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.DS3LethalFall);
        var hit = Base + Hit;
        var checkPlayerDeadFunc = Base + CheckPlayerDead;
        var code = Base + LethalFall;

        AsmHelper.WriteRelativeOffsets(bytes, [
            (code + 0x1, checkPlayerDeadFunc, 5, 0x1 + 1),
            (code + 0x8, WorldChrMan.Base, 7, 0x8 + 3),
            (code + 0x18, hit, 6, 0x18 + 2),
            (code + 0x1F, FallDamageKillFloor, 8, 0x1F + 4),
            (code + 0x27, Hooks.LethalFall + 8, 5, 0x27 + 1)
        ]);
        
        var originalBytes = bytes.Skip(0x1F).Take(8).ToArray();
        
        memoryService.WriteBytes(code, bytes);
        
        hookManager.InstallHook(code, Hooks.LethalFall, originalBytes);
        
    }

    private void InstallAuxHitHooks()
    {
        var auxCheckFlag = Base + CheckAuxProcFlag;
        InstallCheckAuxAttackerHook(auxCheckFlag);
        InstallAuxProcHook(auxCheckFlag);
    }

    private void InstallCheckAuxAttackerHook(nint auxCheckFlag)
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.DS3CheckAuxAttacker);
        var checkPlayerDeadFunc = Base + CheckPlayerDead;
        var code = Base + CheckAuxAttacker;
        
        AsmHelper.WriteRelativeOffsets(bytes, [
            (code + 0x1, checkPlayerDeadFunc, 5, 0x1 + 1),
            (code + 0x8, WorldChrMan.Base, 7, 0x8 + 3),
            (code + 0x26, auxCheckFlag, 7, 0x26 + 2),
            (code + 0x2F, auxCheckFlag, 7, 0x2F + 2),
            (code + 0x3E, Hooks.CheckAuxAttacker + 7, 5, 0x3E + 1)
        ]);
        
        memoryService.WriteBytes(code, bytes);
        hookManager.InstallHook(code, Hooks.CheckAuxAttacker, [0x49, 0x89, 0xE3, 0x49, 0x89, 0x4B, 0x08]);
    }

    private void InstallAuxProcHook(nint auxCheckFlag)
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.DS3AuxProc);
        var hit = Base + Hit;
        var code = Base + AuxProc;
        
        AsmHelper.WriteRelativeOffsets(bytes, [
            (code + 0x9, auxCheckFlag, 7, 0x9 + 2),
            (code + 0x12, hit, 6, 0x12 + 2),
            (code + 0x18, Hooks.AuxProc + 9, 5, 0x18 + 1)
        ]);
        
        memoryService.WriteBytes(code, bytes);
        hookManager.InstallHook(code, Hooks.AuxProc, [0x41, 0x09, 0x42, 0x4C, 0x43, 0x8B, 0x4C, 0x9A, 0x24]);
        
    }
}