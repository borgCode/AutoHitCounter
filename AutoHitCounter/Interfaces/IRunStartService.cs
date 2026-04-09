// 

namespace AutoHitCounter.Interfaces;

public interface IRunStartService
{
    void InstallHook();
    bool IsNewGameStarted();
}