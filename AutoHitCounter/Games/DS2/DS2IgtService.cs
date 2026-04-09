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

public class DS2IgtService
{
    private long _baseMs;
    private readonly Stopwatch _stopwatch = new();
    private readonly nint _igtState = Base + IgtState;

    private readonly nint _saveDataManager;

    private readonly IMemoryService _memoryService;
    private readonly HookManager _hookManager;
    private readonly Action _onRunStart;

    private const int SaveSlotSize = 0x1F0;

    public DS2IgtService(IMemoryService memoryService, HookManager hookManager, Action onRunStart)
    {
        _memoryService = memoryService;
        _hookManager = hookManager;
        _onRunStart = onRunStart;
        _saveDataManager = ResolveSaveDataManager();
    }

    private nint ResolveSaveDataManager()
    {
        if (IsScholar)
        {
            var gameMan = _memoryService.Read<nint>(GameManagerImp.Base);
            var gameDataMan = _memoryService.Read<nint>(gameMan + GameManagerImp.GameDataManager);
            return _memoryService.Read<nint>(gameDataMan + GameManagerImp.SaveDataManager);
        }
        else
        {
            var gameMan = _memoryService.Read<int>(GameManagerImp.Base);
            var gameDataMan = _memoryService.Read<int>(gameMan + GameManagerImp.GameDataManager);
            return _memoryService.Read<int>(gameDataMan + GameManagerImp.SaveDataManager);
        }
    }

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

        var slotIndex = _memoryService.Read<int>(
            _saveDataManager + GameManagerImp.SaveDataManagerOffsets.CurrentSaveSlotIdx);

        if (slotIndex < 0 || slotIndex >= 10)
            return;

        var slotBase = _saveDataManager + slotIndex * SaveSlotSize;
        var playTime = _memoryService.Read<uint>(slotBase + GameManagerImp.SaveSlotOffsets.PlayTime);

        if (playTime == 0)
            return;

        _baseMs = playTime * 1000L;
        _stopwatch.Restart();
    }

    private bool IsPlayerLoaded()
    {
        if (IsScholar)
        {
            var gameMan = _memoryService.Read<nint>(GameManagerImp.Base);
            return _memoryService.Read<nint>(gameMan + GameManagerImp.PlayerCtrl) != IntPtr.Zero;
        }
        else
        {
            var gameMan = _memoryService.Read<int>(GameManagerImp.Base);
            return (nint)_memoryService.Read<int>(gameMan + GameManagerImp.PlayerCtrl) != IntPtr.Zero;
        }
    }

    public void Update()
    {
        var status = _memoryService.Read<int>(_igtState);
        if (status == (int)DS2TimerStatus.NoChange) return;

        _memoryService.Write(_igtState, (int)DS2TimerStatus.NoChange);

        switch (status)
        {
            case (int)DS2TimerStatus.NewGame:
                _baseMs = 0;
                _stopwatch.Restart();
                _onRunStart?.Invoke();
                return;
            case (int)DS2TimerStatus.StopTimer:
                _stopwatch.Stop();
                return;
            case (int)DS2TimerStatus.LoadGame:
                var slotIndex =
                    _memoryService.Read<int>(_saveDataManager +
                                             GameManagerImp.SaveDataManagerOffsets.CurrentSaveSlotIdx);
                if (slotIndex >= 0 && slotIndex < 10)
                {
                    var slotBase = _saveDataManager + slotIndex * SaveSlotSize;
                    _baseMs = _memoryService.Read<uint>(slotBase + GameManagerImp.SaveSlotOffsets.PlayTime) * 1000L;
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

        _memoryService.WriteBytes(code, bytes);
        _hookManager.InstallHook(code, hook, [0x48, 0x89, 0x83, 0x60, 0x13, 0x00, 0x00]);
    }

    private void InstallScholarIgtStopHook()
    {
        var code = Base + IgtStopCode;
        var bytes = AsmLoader.GetAsmBytes(AsmScript.ScholarIgtStop);
        var hook = Hooks.IgtStop;
        var originalBytes = _memoryService.ReadBytes(hook, 12);

        AsmHelper.WriteRelativeOffsets(bytes, [
            (code + 0x5, _igtState, 10, 0x5 + 2),
            (code + 0x1B, hook + 12, 5, 0x1B + 1)
        ]);

        _memoryService.WriteBytes(code, bytes);
        _hookManager.InstallHook(code, hook, originalBytes);
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

        _memoryService.WriteBytes(code, bytes);
        _hookManager.InstallHook(code, hook, [0x48, 0x89, 0x87, 0x60, 0x13, 0x00, 0x00]);
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

        _memoryService.WriteBytes(code, bytes);
        _hookManager.InstallHook(code, hook, [0x89, 0x8B, 0x60, 0x13, 0x00, 0x00]);
    }

    private void InstallVanillaIgtStopHook()
    {
        var code = Base + IgtStopCode;
        var bytes = AsmLoader.GetAsmBytes(AsmScript.VanillaIgtStop);
        var hook = Hooks.IgtStop;
        var originalBytes = _memoryService.ReadBytes(hook, 10);

        AsmHelper.WriteImmediateDword(bytes, (int)_igtState, 0x4 + 2);

        AsmHelper.WriteRelativeOffsets(bytes, [
            (code + 0x18, hook + 10, 5, 0x18 + 1)
        ]);

        _memoryService.WriteBytes(code, bytes);
        _hookManager.InstallHook(code, hook, originalBytes);
    }

    private void InstallVanillaIgtLoadGameHook()
    {
        var code = Base + IgtLoadGameCode;
        var bytes = AsmLoader.GetAsmBytes(AsmScript.VanillaIgtLoadGame);
        var hook = Hooks.IgtLoadGame;

        AsmHelper.WriteImmediateDword(bytes, (int)_igtState, 0x6 + 2);
        AsmHelper.WriteRelativeOffset(bytes, code + 0x10, Hooks.IgtLoadGame + 6, 5, 0x10 + 1);

        _memoryService.WriteBytes(code, bytes);
        _hookManager.InstallHook(code, hook, [0x89, 0x8F, 0x60, 0x13, 0x00, 0x00]);
    }

    #endregion
}