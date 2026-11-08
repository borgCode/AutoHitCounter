//

using AutoHitCounter.Interfaces;
using static AutoHitCounter.Games.BB.BBOffsets;

namespace AutoHitCounter.Games.BB;

public class BBSettingsService(IMemoryService memoryService)
{
    public void ToggleNoLogo(bool isEnabled) =>
        memoryService.WriteBytes(Patches.NoLogo,
            isEnabled ? [0xE9, 0x98, 0x00, 0x00, 0x00, 0x90] : [0x0F, 0x84, 0x97, 0x00, 0x00, 0x00]);
}
