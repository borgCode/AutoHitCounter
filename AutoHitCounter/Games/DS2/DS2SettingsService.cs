// 

using System;
using AutoHitCounter.Enums;
using AutoHitCounter.Interfaces;
using AutoHitCounter.Memory;
using AutoHitCounter.Utilities;
using static AutoHitCounter.Games.DS2.DS2CustomCodeOffsets;
using static AutoHitCounter.Games.DS2.DS2Offsets;

namespace AutoHitCounter.Games.DS2;

public class DS2SettingsService(IMemoryService memoryService, HookManager hookManager)
{
    public void ToggleBabyJumpFix(bool isEnabled)
    {
        var code = Base + BabyJump;

        if (!isEnabled)
        {
            hookManager.UninstallHook(code);
            return;
        }
            
        var origin = Hooks.NoBabyJump;

        if (IsScholar) InstallScholarBabyJumpFix(code, origin);
        else InstallVanillaBabyJumpFix(code, origin);
    }
    
    
    private void InstallScholarBabyJumpFix(nint code, nint origin)
    {
        var codeBytes = AsmLoader.GetAsmBytes(AsmScript.ScholarBabyJump);
        var bytes = BitConverter.GetBytes(GameManagerImp.Base);
        Array.Copy(bytes, 0, codeBytes, 0x1 + 2, 8);
        bytes = AsmHelper.GetJmpOriginOffsetBytes(origin, 5, code + 0x33);
        Array.Copy(bytes, 0, codeBytes, 0x2E + 1, 4);

        memoryService.WriteBytes(code, codeBytes);
        hookManager.InstallHook(code, origin, [0x0F, 0x29, 0x44, 0x24, 0x20]);
    }
        
    private void InstallVanillaBabyJumpFix(nint code, nint origin)
    {
        var codeBytes = AsmLoader.GetAsmBytes(AsmScript.VanillaBabyJump);
        var bytes = BitConverter.GetBytes(GameManagerImp.Base);
        Array.Copy(bytes, 0, codeBytes, 0x4 + 1, 4);
        bytes = AsmHelper.GetJmpOriginOffsetBytes(origin, 7, code + 0x24);
        Array.Copy(bytes, 0, codeBytes, 0x1F + 1, 4);
        memoryService.WriteBytes(code, codeBytes);
        hookManager.InstallHook(code, origin, [0x0F, 0x51, 0xC0, 0x0F, 0x29, 0x45, 0xB0]);
    }
    
    
    public void ToggleCreditSkip(bool isCreditSkipEnabled)
    {
        var code = Base + CreditSkip;

        if (!isCreditSkipEnabled)
        {
            hookManager.UninstallHook(code);
            return;
        }

        var hookLoc = Hooks.CreditSkip;
        var modifyOnceFlag = Base + CreditSkipFlag;
        memoryService.Write(modifyOnceFlag, 0);

        if (IsScholar) InstallScholarCreditSkip(modifyOnceFlag, hookLoc, code);
        else InstallVanillaCreditSkip(modifyOnceFlag, hookLoc, code);

    }
    
    private void InstallScholarCreditSkip(nint modifyOnceFlag, nint hookLoc, nint code)
    {
        var codeBytes = AsmLoader.GetAsmBytes(AsmScript.ScholarCreditSkip);
        AsmHelper.WriteRelativeOffsets(codeBytes, [
            (code + 0x7, modifyOnceFlag, 7, 0x7 + 2),
            (code + 0x17, modifyOnceFlag, 10, 0x17 + 2),
            (code + 0x21, hookLoc + 7, 5, 0x21 + 1)
        ]);
        memoryService.WriteBytes(code, codeBytes);
        hookManager.InstallHook(code, hookLoc, [0x48, 0x81, 0xEC, 0x20, 0x02, 0x00, 0x00]);
    }

    private void InstallVanillaCreditSkip(nint modifyOnceFlag, nint hookLoc, nint code)
    {
        var codeBytes = AsmLoader.GetAsmBytes(AsmScript.VanillaCreditSkip);
        var bytes = AsmHelper.GetJmpOriginOffsetBytes(hookLoc, 6, code + 0x25);
        Array.Copy(bytes, 0, codeBytes, 0x20 + 1, 4);
        AsmHelper.WriteImmediateDwords(codeBytes, [
            ((int)modifyOnceFlag, 0x6 + 2),
            ((int)modifyOnceFlag, 0x16 + 2)
        ]);

        memoryService.WriteBytes(code, codeBytes);
        hookManager.InstallHook(code, hookLoc, [0x81, 0xEC, 0xFC, 0x01, 0x00, 0x00]);
    }
    
    
    public void ToggleDoubleClick(bool isDisableDoubleClickEnabled)
    {
        var ptr = IsScholar
            ? memoryService.FollowPointers64(KatanaMainApp.Base, KatanaMainApp.DoubleClickPtrChain, false)
            : memoryService.FollowPointers32(KatanaMainApp.Base, KatanaMainApp.DoubleClickPtrChain, false);
        memoryService.Write(ptr, isDisableDoubleClickEnabled);
    }
}