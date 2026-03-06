// 

using System;
using System.Collections.Generic;
using AutoHitCounter.Enums;
using AutoHitCounter.Interfaces;
using AutoHitCounter.Memory;
using AutoHitCounter.Utilities;
using static AutoHitCounter.Games.SK.SKOffsets;

namespace AutoHitCounter.Games.SK;

public class SKModule : IGameModule, IDisposable, IVersionedGameModule
{
    private readonly IMemoryService _memoryService;
    private readonly IStateService _stateService;
    private readonly HookManager _hookManager;
    private readonly ITickService _tickService;
    private readonly Dictionary<uint, string> _events;
    private readonly IGameSettingsProvider _settings;

    public string GameVersion => SKOffsets.Version.GetDescription();
    
    private DateTime? _lastHit;
    private nint _igtPtr;
    private SKHitService _hitService;
    private SKEventService _eventService;
    
    public event Action<int> OnHit;
    public event Action OnEventSet;
    public event Action<long> OnIgtChanged;
    public event Action OnVersionDetected;

    public SKModule(IMemoryService memoryService, IStateService stateService, HookManager hookManager,
        ITickService tickService, Dictionary<uint, string> events, IGameSettingsProvider settings)
    {
        _memoryService = memoryService;
        _stateService = stateService;
        _hookManager = hookManager;
        _tickService = tickService;
        _events = events;
        _settings = settings;
        settings.OnSettingsChanged += ApplySettings;

        stateService.Subscribe(State.Attached, Initialize);
        _lastHit = DateTime.Now;
    }
    
    private void ApplySettings()                                                                                            {
        _hitService?.SetRobertoStaggerCounts(_settings.GetFlag("should_count_roberto"));
    }
    
    private void Initialize()
    {
        InitializeOffsets();
        OnVersionDetected?.Invoke();

        SKCustomCodeOffsets.Base = _memoryService.AllocCustomCodeMem();
        
#if DEBUG
        Console.WriteLine($@"Code cave: 0x{(long)SKCustomCodeOffsets.Base:X}");
#endif

        _hitService = new SKHitService(_memoryService, _hookManager);
        _eventService = new SKEventService(_memoryService, _hookManager, _events);
        _eventService.InstallHook();
        _hitService.InstallHooks();
        _igtPtr = _memoryService.Read<nint>(GameDataMan.Base) + GameDataMan.Igt;
        
        ApplySettings();
        
        _tickService.RegisterGameTick(Tick);
    }
    
    private void InitializeOffsets()
    {
        if (_memoryService.TargetProcess == null) return;
        var module = _memoryService.TargetProcess.MainModule;
        var fileVersion = module?.FileVersionInfo.FileVersion;
        var moduleBase = _memoryService.BaseAddress;
        SKOffsets.Initialize(fileVersion, moduleBase);
    }
    
    
    private void Tick()
    {
        if (!IsLoaded()) return;

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
    
    public void UpdateEvents(Dictionary<uint, string> events)
    {
        _eventService?.UpdateEvents(events);
    }
}