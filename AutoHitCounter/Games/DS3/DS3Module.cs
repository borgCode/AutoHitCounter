// 

using System;
using System.Collections.Generic;
using AutoHitCounter.Enums;
using AutoHitCounter.Interfaces;
using AutoHitCounter.Memory;
using static AutoHitCounter.Games.DS3.DS3Offsets;

namespace AutoHitCounter.Games.DS3;

public class DS3Module : IGameModule, IDisposable, IVersionedGameModule
{
    private readonly IMemoryService _memoryService;
    private readonly IStateService _stateService;
    private readonly HookManager _hookManager;
    private readonly ITickService _tickService;
    private readonly Dictionary<uint, string> _events;
    
    public string GameVersion => DS3Offsets.Version switch
{
    DS3Version.Version1_3_2_0 => "1.3.2",
    DS3Version.Version1_4_1_0 => "1.4.1",
    DS3Version.Version1_4_2_0 => "1.4.2",
    DS3Version.Version1_4_3_0 => "1.4.3",
    DS3Version.Version1_5_0_0 => "1.5.0",
    DS3Version.Version1_5_1_0 => "1.5.1",
    DS3Version.Version1_6_0_0 => "1.6.0",
    DS3Version.Version1_7_0_0 => "1.7.0",
    DS3Version.Version1_8_0_0 => "1.8.0",
    DS3Version.Version1_9_0_0 => "1.9.0",
    DS3Version.Version1_10_0_0 => "1.10.0",
    DS3Version.Version1_11_0_0 => "1.11.0",
    DS3Version.Version1_12_0_0 => "1.12.0",
    DS3Version.Version1_13_0_0 => "1.13.0",
    DS3Version.Version1_14_0_0 => "1.14.0",
    DS3Version.Version1_15_0_0 => "1.15.0",
    DS3Version.Version1_15_1_0 => "1.15.1",
    DS3Version.Version1_15_2_0 => "1.15.2",
    _ => "Unknown"
};
    
    private DateTime? _lastHit;
    private nint _igtPtr;
    private DS3HitService _hitService;
    private DS3EventService _eventService;
    
    public event Action<int> OnHit;
    public event Action OnEventSet;
    public event Action<long> OnIgtChanged;

    public DS3Module(IMemoryService memoryService, IStateService stateService, HookManager hookManager,
        ITickService tickService, Dictionary<uint, string> events)
    {
        _memoryService = memoryService;
        _stateService = stateService;
        _hookManager = hookManager;
        _tickService = tickService;
        _events = events;
        
        stateService.Subscribe(State.Attached, Initialize);
        _lastHit = DateTime.Now;
    }
    
    private void Initialize()
    {
        InitializeOffsets();

        DS3CustomCodeOffsets.Base = _memoryService.AllocCustomCodeMem();
        
#if DEBUG
        Console.WriteLine($@"Code cave: 0x{(long)DS3CustomCodeOffsets.Base:X}");
#endif

        _hitService = new DS3HitService(_memoryService, _hookManager);
        _eventService = new DS3EventService(_memoryService, _hookManager, _events);
        _eventService.InstallHook();
        _hitService.InstallHooks();
        _igtPtr = _memoryService.Read<nint>(GameDataMan.Base) + GameDataMan.Igt;
        _tickService.RegisterGameTick(Tick);
    }
    
    private void InitializeOffsets()
    {
        if (_memoryService.TargetProcess == null) return;
        var module = _memoryService.TargetProcess.MainModule;
        var fileVersion = module?.FileVersionInfo.FileVersion;
        var moduleBase = _memoryService.BaseAddress;
        DS3Offsets.Initialize(fileVersion, moduleBase);
    }
    
    private void Tick()
    {
        if (!IsLoaded()) return;

        _hitService.EnsureHooksInstalled();

        if (_hitService.HasHit() && (_lastHit == null || (DateTime.Now - _lastHit.Value).TotalSeconds > 3))
        {
            OnHit?.Invoke(1);
            _lastHit = DateTime.Now;
        }

        if (_eventService.ShouldSplit())
        {
            OnEventSet?.Invoke();
        }

        OnIgtChanged?.Invoke(_memoryService.Read<uint>(_igtPtr));
    }

    private bool IsLoaded()
    {
        var worldChrman = _memoryService.Read<nint>(WorldChrMan.Base);
        return _memoryService.Read<nint>(worldChrman + WorldChrMan.PlayerIns) != 0;
    }

    public void Dispose()
    {
        _stateService.Unsubscribe(State.Attached, Initialize);
        _tickService.UnregisterGameTick();
        OnHit = null;
        OnEventSet = null;
        OnIgtChanged = null;
    }
    
}