// 

namespace AutoHitCounter.Interfaces;

public interface IHitService
{
    void InstallHooks();
    bool HasHit();
}