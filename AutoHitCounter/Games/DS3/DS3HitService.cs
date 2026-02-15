// 

using AutoHitCounter.Interfaces;
using AutoHitCounter.Memory;

namespace AutoHitCounter.Games.DS3;

public class DS3HitService(IMemoryService memoryService, HookManager hookManager) : IHitService
{
    public void InstallHooks()
    {
        throw new System.NotImplementedException();
    }

    public bool HasHit()
    {
        throw new System.NotImplementedException();
    }
}