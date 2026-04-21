// 

using AutoHitCounter.Enums;
using AutoHitCounter.Interfaces;
using AutoHitCounter.Memory;
using AutoHitCounter.Utilities;
using static AutoHitCounter.Games.DS3.DS3CustomCodeOffsets;
using static AutoHitCounter.Games.DS3.DS3Offsets;

namespace AutoHitCounter.Games.DS3;

public class DS3SettingsService(IMemoryService memoryService, HookManager hookManager)
{
    private bool _noInvasionsEnabled;

    public void ToggleNoLogo(bool isEnabled)
    {
        if (isEnabled)
        {
            memoryService.WriteBytes(Patches.NoLogo,
            [
                0x48, 0x31, 0xC0, 0x48, 0x89, 0x02, 0x49, 0x89, 0x04, 0x24, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90,
                0x90, 0x90, 0x90
            ]);
        }
        else
        {
            byte[] bytes =
            [
                0xE8, 0x00, 0x00, 0x00, 0x00,     
                0x90,                             
                0x4D, 0x8B, 0xC7,                 
                0x49, 0x8B, 0xD4,                 
                0x48, 0x8B, 0xC8,                 
                0xE8, 0x00, 0x00, 0x00, 0x00      
            ];
            
           AsmHelper.WriteRelativeOffsets(bytes, [
           (Patches.NoLogo, Functions.OriginalLogoFunc, 5, 1),
           (Patches.NoLogo + 0xF, Functions.OriginalLogoFunc, 5, 0xF + 1),
           
           ]);

            memoryService.WriteBytes(Patches.NoLogo, bytes);
            
        }
    }

    public void ToggleStutterFix(bool isEnabled) =>
        memoryService.Write(memoryService.Read<nint>(UserInputManager.Base) + UserInputManager.SteamInputEnum,
            isEnabled);

    public void ToggleNoInvasions(bool noInvasions)
    {
        var code = Base + NoInvasions;
        if (noInvasions)
        {
            var bytes = AsmLoader.GetAsmBytes(AsmScript.DS3NoInvasions);
            AsmHelper.WriteRelativeOffsets(bytes, [
            (code, Functions.IsOnline, 5, 1),
            (code + 0x18, Hooks.IsOnlineHook + 5, 5, 0x18 + 1),
            ]);
            byte[] originalBytes = [0xE8, 0x00, 0x00, 0x00, 0x00];
            AsmHelper.WriteRelativeOffset(originalBytes, Hooks.IsOnlineHook, Functions.IsOnline, 5, 1);

            memoryService.WriteBytes(code, bytes);
            hookManager.InstallHook(code, Hooks.IsOnlineHook, originalBytes);
            _noInvasionsEnabled = true;
        }
        else
        {
            _noInvasionsEnabled = false;
            hookManager.UninstallHook(code);
        }
    }

    // Needed because arxan can restore any hook site
    public void EnsureHooksInstalled()
    {
        if (_noInvasionsEnabled && memoryService.Read<byte>(Hooks.IsOnlineHook) != 0xE9)
            ToggleNoInvasions(true);
    }
}