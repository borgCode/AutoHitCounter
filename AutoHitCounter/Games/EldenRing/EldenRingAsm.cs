// 

using AutoHitCounter.Interfaces;
using AutoHitCounter.Memory;
using AutoHitCounter.Utilities;
using static AutoHitCounter.Games.EldenRing.EldenRingOffsets;

namespace AutoHitCounter.Games.EldenRing;

public class EldenRingAsm(IMemoryService memoryService, HookManager hookManager)
{
    private int _lastHitCount;
    
    public void InstallEventHook()
    {
        
    }

    public void InstallHitHooks()
    {
        var bytes = AsmLoader.GetAsmBytes("EldenRingHit");
        var hit = CodeCaveOffsets.Base + CodeCaveOffsets.Hit;
        var code = CodeCaveOffsets.Base + CodeCaveOffsets.HitCode;
        AsmHelper.WriteRelativeOffsets(bytes, [
        (code + 0x8, WorldChrMan.Base, 7, 0x8 + 3),
        (code + 0x43, Functions.ChrInsByHandle, 5, 0x43 + 1),
        (code + 0x60, hit, 6, 0x60 + 2),
        (code + 0x69, Hooks.Hit + 5, 5, 0x69 + 1),
        ]);
        
        memoryService.WriteBytes(code, bytes);
        hookManager.InstallHook(code, Hooks.Hit, [0x48, 0x89, 0x5C, 0x24, 0x08]);
    }

    public bool HasHit()
    {
        var current = memoryService.Read<int>(CodeCaveOffsets.Base + CodeCaveOffsets.Hit);
        var newHits = current - _lastHitCount;
        _lastHitCount = current;
        return newHits > 0;
    }
}