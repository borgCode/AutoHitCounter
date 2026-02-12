// 

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
        InstallFallDamageHook();
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
        var bytes = AsmLoader.GetAsmBytes("ScholarHit");
        var hit = CodeCaveOffsets.Base + CodeCaveOffsets.Hit;
        var code = CodeCaveOffsets.Base + CodeCaveOffsets.HitCode;
        AsmHelper.WriteRelativeOffsets(bytes, [
        (code + 0x1, GameManagerImp.Base, 7, 0x1 + 3),
        (code + 0x22, hit, 6, 0x22 + 2),
        (code + 0x31, Hooks.Hit + 8, 5, 0x31 + 1)
        ]);
        
        memoryService.WriteBytes(code, bytes);
        hookManager.InstallHook(code, Hooks.Hit, [0x41, 0x88, 0x46, 0x15, 0x0F, 0xB6, 0x45, 0x20]);
    }

    private void InstallFallDamageHook()
    {
        var bytes = AsmLoader.GetAsmBytes("ScholarFallDamage");
        var hit = CodeCaveOffsets.Base + CodeCaveOffsets.Hit;
        var code = CodeCaveOffsets.Base + CodeCaveOffsets.FallDamage;
        AsmHelper.WriteRelativeOffsets(bytes, [
            (code + 0x1, GameManagerImp.Base, 7, 0x1 + 3),
            (code + 0x15, hit, 6, 0x15 + 2),
            (code + 0x22, Hooks.FallDamage + 6, 5, 0x22 + 1)
        ]);
        
        memoryService.WriteBytes(code, bytes);
        hookManager.InstallHook(code, Hooks.FallDamage, [0x89, 0x83, 0x68, 0x01, 0x00, 0x00]);
    }
}