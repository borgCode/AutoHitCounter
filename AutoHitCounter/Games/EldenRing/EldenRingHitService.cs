// 

using AutoHitCounter.Interfaces;
using AutoHitCounter.Memory;
using AutoHitCounter.Utilities;
using static AutoHitCounter.Games.EldenRing.EldenRingOffsets;

namespace AutoHitCounter.Games.EldenRing;

public class EldenRingHitService(IMemoryService memoryService, HookManager hookManager) : IHitService
{
    private int _lastHitCount;
    
    public void InstallHooks()
    {
        InstallHitHook();
        InstallFallDamageHook();
        InstallKillBoxHook();
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
        var bytes = AsmLoader.GetAsmBytes("EldenRingHit");
        var hit = CodeCaveOffsets.Base + CodeCaveOffsets.Hit;
        var code = CodeCaveOffsets.Base + CodeCaveOffsets.HitCode;
        AsmHelper.WriteRelativeOffsets(bytes, [
            (code + 0x8, WorldChrMan.Base, 7, 0x8 + 3),
            (code + 0x47, Functions.ChrInsByHandle, 5, 0x47 + 1),
            (code + 0xA3, hit, 6, 0xA3 + 2),
            (code + 0xAC, Hooks.Hit + 5, 5, 0xAC + 1),
        ]);
        
        memoryService.WriteBytes(code, bytes);
        hookManager.InstallHook(code, Hooks.Hit, [0x48, 0x89, 0x5C, 0x24, 0x08]);
    }

    private void InstallFallDamageHook()
    {
        var bytes = AsmLoader.GetAsmBytes("EldenRingFallDamage");
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
        var bytes = AsmLoader.GetAsmBytes("EldenRingKillBox");
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
}