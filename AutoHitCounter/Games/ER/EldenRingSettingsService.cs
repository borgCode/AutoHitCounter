// 

using AutoHitCounter.Interfaces;
using static AutoHitCounter.Games.ER.EldenRingOffsets;

namespace AutoHitCounter.Games.ER;

public class EldenRingSettingsService(IMemoryService memoryService)
{
    public void ToggleNoLogo(bool isEnabled) =>
        memoryService.WriteBytes(Patches.NoLogo, isEnabled ? [0x90, 0x90] : [0x74, 0x53]);
    
    public void ToggleStutterFix(bool isEnabled) =>
        memoryService.Write(memoryService.Read<nint>(UserInputManager.Base) + UserInputManager.SteamInputEnum, isEnabled);
    
    public void ToggleDisableAchievements(bool isEnabled)
    {
        var isAwardAchievementsEnabledFlag = memoryService.FollowPointers64(CSTrophy.Base, [
            CSTrophy.CSTrophyPlatformImp_forSteam,
            CSTrophy.IsAwardAchievementEnabled
        ], false);
        memoryService.Write(isAwardAchievementsEnabledFlag, isEnabled);
    }
}