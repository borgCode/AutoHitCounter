// 

using AutoHitCounter.Interfaces;
using static AutoHitCounter.Games.NR.NROffsets;

namespace AutoHitCounter.Games.NR;

public class NRSettingsService(IMemoryService memoryService)
{
    public void ToggleNoLogo(bool isEnabled) =>
        memoryService.WriteBytes(Patches.NoLogo, isEnabled ? [0x90, 0x90] : [0x74, 0x57]);
}