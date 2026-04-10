// 

using System;
using System.Diagnostics;
using AutoHitCounter.Enums;
using AutoHitCounter.Interfaces;
using AutoHitCounter.Memory;
using AutoHitCounter.Utilities;
using static AutoHitCounter.Games.DS2.DS2CustomCodeOffsets;
using static AutoHitCounter.Games.DS2.DS2Offsets;

namespace AutoHitCounter.Games.DS2;

public class DS2IgtService(IMemoryService memoryService, HookManager hookManager, Action onRunStart)
{
    private long _baseMs;
    private readonly Stopwatch _stopwatch = new();
    private readonly nint _igtState = Base + IgtState;

    private const int SaveSlotSize = 0x1F0;

    
    public long ElapsedMilliseconds => _baseMs + _stopwatch.ElapsedMilliseconds;

    public void InstallHooks()
    {
        if (IsScholar) InstallScholarHooks();
        else InstallVanillaHooks();
    }

    public void InitializeFromCurrentState()
    {
        if (!IsPlayerLoaded())
            return;
        
        var saveDataManager = ResolveSaveDataManager();

        var slotIndex = memoryService.Read<int>(
            saveDataManager + GameManagerImp.SaveDataManagerOffsets.CurrentSaveSlotIdx);

        if (slotIndex < 0 || slotIndex >= 10)
            return;

        var slotBase = saveDataManager + slotIndex * SaveSlotSize;
        var playTime = memoryService.Read<uint>(slotBase + GameManagerImp.SaveSlotOffsets.PlayTime);

        if (playTime == 0)
            return;

        _baseMs = playTime * 1000L;
        _stopwatch.Restart();
    }
    
    private nint ResolveSaveDataManager()
    {
        if (IsScholar)
        {
            var gameMan = memoryService.Read<nint>(GameManagerImp.Base);
            var gameDataMan = memoryService.Read<nint>(gameMan + GameManagerImp.GameDataManager);
            return memoryService.Read<nint>(gameDataMan + GameManagerImp.SaveDataManager);
        }
        else
        {
            var gameMan = memoryService.Read<int>(GameManagerImp.Base);
            var gameDataMan = memoryService.Read<int>(gameMan + GameManagerImp.GameDataManager);
            return memoryService.Read<int>(gameDataMan + GameManagerImp.SaveDataManager);
        }
    }

    private bool IsPlayerLoaded()
    {
        if (IsScholar)
        {
            var gameMan = memoryService.Read<nint>(GameManagerImp.Base);
            return memoryService.Read<nint>(gameMan + GameManagerImp.PlayerCtrl) != IntPtr.Zero;
        }
        else
        {
            var gameMan = memoryService.Read<int>(GameManagerImp.Base);
            return (nint)memoryService.Read<int>(gameMan + GameManagerImp.PlayerCtrl) != IntPtr.Zero;
        }
    }

    public void Update()
    {
        var status = memoryService.Read<int>(_igtState);
        if (status == (int)DS2TimerStatus.NoChange) return;

        memoryService.Write(_igtState, (int)DS2TimerStatus.NoChange);

        switch (status)
        {
            case (int)DS2TimerStatus.NewGame:
                _baseMs = 0;
                _stopwatch.Restart();
                onRunStart?.Invoke();
                return;
            case (int)DS2TimerStatus.StopTimer:
                _stopwatch.Stop();
                return;
            case (int)DS2TimerStatus.LoadGame:
                var saveDataManager = ResolveSaveDataManager();
                var slotIndex =
                    memoryService.Read<int>(saveDataManager +
                                            GameManagerImp.SaveDataManagerOffsets.CurrentSaveSlotIdx);
                if (slotIndex >= 0 && slotIndex < 10)
                {
                    var slotBase = saveDataManager + slotIndex * SaveSlotSize;
                    _baseMs = memoryService.Read<uint>(slotBase + GameManagerImp.SaveSlotOffsets.PlayTime) * 1000L;
                    _stopwatch.Restart();
                }

                return;
        }
    }

    public void Stop() => _stopwatch.Stop();
    
    

    #region Scholar

    private void InstallScholarHooks()
    {
        InstallScholarIgtNewGameHook();
        InstallScholarIgtStopHook();
        InstallScholarIgtLoadGameHook();
    }

    private void InstallScholarIgtNewGameHook()
    {
        var code = Base + IgtNewGameCode;
        var bytes = AsmLoader.GetAsmBytes(AsmScript.ScholarIgtNewGame);
        var hook = Hooks.IgtNewGame;

        AsmHelper.WriteRelativeOffsets(bytes, [
            (code + 0x7, _igtState, 10, 0x7 + 2),
            (code + 0x11, hook + 7, 5, 0x11 + 1)
        ]);

        memoryService.WriteBytes(code, bytes);
        hookManager.InstallHook(code, hook, [0x48, 0x89, 0x83, 0x60, 0x13, 0x00, 0x00]);
    }

    private void InstallScholarIgtStopHook()
    {
        var code = Base + IgtStopCode;
        var bytes = AsmLoader.GetAsmBytes(AsmScript.ScholarIgtStop);
        var hook = Hooks.IgtStop;
        var originalBytes = memoryService.ReadBytes(hook, 12);

        AsmHelper.WriteRelativeOffsets(bytes, [
            (code + 0x5, _igtState, 10, 0x5 + 2),
            (code + 0x1B, hook + 12, 5, 0x1B + 1)
        ]);

        memoryService.WriteBytes(code, bytes);
        hookManager.InstallHook(code, hook, originalBytes);
    }

    private void InstallScholarIgtLoadGameHook()
    {
        var code = Base + IgtLoadGameCode;
        var bytes = AsmLoader.GetAsmBytes(AsmScript.ScholarIgtLoadGame);
        var hook = Hooks.IgtLoadGame;

        AsmHelper.WriteRelativeOffsets(bytes, [
            (code + 0x7, _igtState, 10, 0x7 + 2),
            (code + 0x11, hook + 7, 5, 0x11 + 1)
        ]);

        memoryService.WriteBytes(code, bytes);
        hookManager.InstallHook(code, hook, [0x48, 0x89, 0x87, 0x60, 0x13, 0x00, 0x00]);
    }

    #endregion

    #region Vanilla

    private void InstallVanillaHooks()
    {
        InstallVanillaIgtNewGameHook();
        InstallVanillaIgtStopHook();
        InstallVanillaIgtLoadGameHook();
    }

    private void InstallVanillaIgtNewGameHook()
    {
        var code = Base + IgtNewGameCode;
        var bytes = AsmLoader.GetAsmBytes(AsmScript.VanillaIgtNewGame);
        var hook = Hooks.IgtNewGame;

        AsmHelper.WriteImmediateDword(bytes, (int)_igtState, 0x6 + 2);
        AsmHelper.WriteRelativeOffset(bytes, code + 0x10, Hooks.IgtNewGame + 6, 5, 0x10 + 1);

        memoryService.WriteBytes(code, bytes);
        hookManager.InstallHook(code, hook, [0x89, 0x8B, 0x60, 0x13, 0x00, 0x00]);
    }

    private void InstallVanillaIgtStopHook()
    {
        var code = Base + IgtStopCode;
        var bytes = AsmLoader.GetAsmBytes(AsmScript.VanillaIgtStop);
        var hook = Hooks.IgtStop;
        var originalBytes = memoryService.ReadBytes(hook, 10);

        AsmHelper.WriteImmediateDword(bytes, (int)_igtState, 0x4 + 2);

        AsmHelper.WriteRelativeOffsets(bytes, [
            (code + 0x18, hook + 10, 5, 0x18 + 1)
        ]);

        memoryService.WriteBytes(code, bytes);
        hookManager.InstallHook(code, hook, originalBytes);
    }

    private void InstallVanillaIgtLoadGameHook()
    {
        var code = Base + IgtLoadGameCode;
        var bytes = AsmLoader.GetAsmBytes(AsmScript.VanillaIgtLoadGame);
        var hook = Hooks.IgtLoadGame;

        AsmHelper.WriteImmediateDword(bytes, (int)_igtState, 0x6 + 2);
        AsmHelper.WriteRelativeOffset(bytes, code + 0x10, Hooks.IgtLoadGame + 6, 5, 0x10 + 1);

        memoryService.WriteBytes(code, bytes);
        hookManager.InstallHook(code, hook, [0x89, 0x8F, 0x60, 0x13, 0x00, 0x00]);
    }

    #endregion
}