// 

using System.Diagnostics;
using AutoHitCounter.Enums;
using AutoHitCounter.Interfaces;
using AutoHitCounter.Memory;
using AutoHitCounter.Utilities;
using static AutoHitCounter.Games.DS2S.DS2ScholarOffsets;

namespace AutoHitCounter.Games.DS2S;

public class DS2ScholarIgtService(IMemoryService memoryService, HookManager hookManager)
{
    private long _baseMs;
    private readonly Stopwatch _stopwatch = new();
    private readonly nint _igtState = CodeCaveOffsets.Base + CodeCaveOffsets.IgtState;

    private readonly nint _saveDataManager = memoryService.FollowPointers(GameManagerImp.Base,
    [
        GameManagerImp.GameDataManager, GameManagerImp.SaveDataManager
    ], true);

    private const int SaveSlotSize = 0x1F0;

    public long ElapsedMilliseconds => _baseMs + _stopwatch.ElapsedMilliseconds;
    
    public void InstallHooks()
    {
        InstallIgtNewGameHook();
        InstallIgtStopHook();
        InstallIgtLoadGameHook();
    }

    private void InstallIgtNewGameHook()
    {
        var code = CodeCaveOffsets.Base + CodeCaveOffsets.IgtNewGameCode;
        var bytes = AsmLoader.GetAsmBytes(AsmScript.ScholarIgtNewGame);
        var hook = Hooks.IgtNewGame;
        
        AsmHelper.WriteRelativeOffsets(bytes, [
            (code + 0x7, _igtState, 10, 0x7 + 2),
            (code + 0x11, hook + 7, 5, 0x11 + 1)
        ]);
        
        memoryService.WriteBytes(code, bytes);
        hookManager.InstallHook(code, hook, [0x48, 0x89, 0x83, 0x60, 0x13, 0x00, 0x00]);
    }

    private void InstallIgtStopHook()
    {
        var code = CodeCaveOffsets.Base + CodeCaveOffsets.IgtStopCode;
        var bytes = AsmLoader.GetAsmBytes(AsmScript.ScholarIgtStop);
        var hook = Hooks.IgtStop;
        var originalBytes = memoryService.ReadBytes(hook, 5);
        
        AsmHelper.WriteRelativeOffsets(bytes, [
            (code, Functions.RequestSave, 5, 1),
            (code + 5, _igtState, 10, 0x5 + 2),
            (code + 0xF, hook + 5, 5, 0xF + 1)
        ]);
        
        memoryService.WriteBytes(code, bytes);
        hookManager.InstallHook(code, hook, originalBytes);
    }

    private void InstallIgtLoadGameHook()
    {
        var code = CodeCaveOffsets.Base + CodeCaveOffsets.IgtLoadGameCode;
        var bytes = AsmLoader.GetAsmBytes(AsmScript.ScholarIgtLoadGame);
        var hook = Hooks.IgtLoadGame;
        
        AsmHelper.WriteRelativeOffsets(bytes, [
            (code + 0x7, _igtState, 10, 0x7 + 2),
            (code + 0x11, hook + 7, 5, 0x11 + 1)
        ]);
        
        memoryService.WriteBytes(code, bytes);
        hookManager.InstallHook(code, hook, [0x48, 0x89, 0x87, 0x60, 0x13, 0x00, 0x00]);
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
                return;
            case (int)DS2TimerStatus.StopTimer:
                _stopwatch.Stop();
                return;
            case (int)DS2TimerStatus.LoadGame:
                var slotIndex = memoryService.Read<int>(_saveDataManager + GameManagerImp.SaveDataManagerOffsets.CurrentSaveSlotIdx);
                if (slotIndex >= 0 && slotIndex < 10)
                {
                    var slotBase = _saveDataManager + slotIndex * SaveSlotSize;
                    _baseMs = memoryService.Read<uint>(slotBase + GameManagerImp.SaveSlotOffsets.PlayTime) * 1000L;
                    _stopwatch.Restart();
                }
                return;
        }
    }
    

    public void Stop() => _stopwatch.Stop();
}