// 

using AutoHitCounter.Enums;
using AutoHitCounter.Interfaces;
using AutoHitCounter.Memory;
using AutoHitCounter.Utilities;
using static AutoHitCounter.Games.SK.SKCustomCodeOffsets;
using static AutoHitCounter.Games.SK.SKOffsets;

namespace AutoHitCounter.Games.SK;

public class SKRunStartService(IMemoryService memoryService, HookManager hookManager) : IRunStartService
{
    public void InstallHook()
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.SKRunStart);
        var code = Base + RunStartCode;
        AsmHelper.WriteRelativeOffsets(bytes, [
            (code + 0x5, Base + RunStartFlag, 7, 0x5 + 2),
            (code + 0xC, Hooks.StartNewGame + 5, 5, 0xC + 1)
        ]);

        memoryService.WriteBytes(code, bytes);
        hookManager.InstallHook(code, Hooks.StartNewGame, [0x41, 0xB0, 0x01, 0x31, 0xD2]);
    }

    public bool IsNewGameStarted()
    {
        if (memoryService.Read<byte>(Base + RunStartFlag) != 1)
            return false;
        memoryService.WriteBytes(Base + RunStartFlag, [0]);
        return true;
    }
}