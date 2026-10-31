//

using AutoHitCounter.Enums;
using AutoHitCounter.Interfaces;
using AutoHitCounter.Memory;
using AutoHitCounter.Utilities;
using static AutoHitCounter.Games.BB.BBCustomCodeOffsets;

namespace AutoHitCounter.Games.BB;

public class BBRunStartService(IMemoryService memoryService, HookManager hookManager) : IRunStartService
{
    public void InstallHook()
    {
        var code = Base + RunStartCode;
        var bytes = AsmLoader.GetAsmBytes(AsmScript.BBRunStart);
        var hookLoc = BBOffsets.Hooks.StartNewGame;
        byte[] originalBytes = [0xC7, 0x43, 0x0C, 0x06, 0x00, 0x00, 0x00, 0xC6, 0x83, 0xC0, 0x01, 0x00, 0x00, 0x01];

        AsmHelper.WriteRelativeOffset(bytes, code, Base + RunStartFlag, 7, 2);
        AsmHelper.WriteAbsoluteAddress(bytes, hookLoc + originalBytes.Length, 0x1B);

        memoryService.WriteBytes(code, bytes);
        hookManager.InstallAbsoluteHook(code, hookLoc, originalBytes);
    }

    public bool IsNewGameStarted()
    {
        if (memoryService.Read<byte>(Base + RunStartFlag) != 1)
            return false;
        memoryService.WriteBytes(Base + RunStartFlag, [0]);
        return true;
    }
}
