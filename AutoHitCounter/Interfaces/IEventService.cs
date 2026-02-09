// 

namespace AutoHitCounter.Interfaces;

public interface IEventService
{
    void InstallHook();
    bool ShouldSplit();
}