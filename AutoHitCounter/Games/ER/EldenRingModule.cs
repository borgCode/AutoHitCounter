// 

using System;
using System.Collections.Generic;
using AutoHitCounter.Enums;
using AutoHitCounter.Interfaces;
using AutoHitCounter.Memory;

namespace AutoHitCounter.Games.ER;

public class EldenRingModule : IGameModule
{
    private readonly IMemoryService _memoryService;
    private readonly HookManager _hookManager;
    private readonly ITickService _tickService;
    private readonly Dictionary<uint, string> _eldenRingEvents;
    private EldenRingHitService _eldenRingHitService;
    private EldenRingEventService _eldenRingEventService;
    
    private DateTime? _lastHit;

    public EldenRingModule(IMemoryService memoryService, IStateService stateService, HookManager hookManager,
        ITickService tickService, Dictionary<uint, string> eldenRingEvents)
    {
        _memoryService = memoryService;
        _hookManager = hookManager;
        _tickService = tickService;
        _eldenRingEvents = eldenRingEvents;

        stateService.Subscribe(State.Attached, Initialize);
        _lastHit = DateTime.Now;
    }


    
    private void Initialize()
    {
        InitializeOffsets();
        
        _eldenRingHitService = new EldenRingHitService(_memoryService, _hookManager);
        _eldenRingEventService = new EldenRingEventService(_memoryService, _hookManager, _eldenRingEvents);
        _eldenRingEventService.InstallHook();
        _eldenRingEventService.InstallHook();
        _eldenRingHitService.InstallHooks();
        _tickService.RegisterGameTick(Tick);
    }

    public event Action<int> OnHit;

    public event Action OnEventSet;

    private void InitializeOffsets()
    {
        if (_memoryService.TargetProcess == null) return;
        var module = _memoryService.TargetProcess.MainModule;
        var fileVersion = module?.FileVersionInfo.FileVersion;
        var moduleBase = _memoryService.BaseAddress;
        EldenRingOffsets.Initialize(fileVersion, moduleBase);
    }
    
    private void Tick()
    {
        if (_eldenRingHitService.HasHit())
        {
            if (_lastHit != null && (DateTime.Now - _lastHit.Value).TotalSeconds < 3) return;
            OnHit?.Invoke(1);
            _lastHit = DateTime.Now;
        }

        if (_eldenRingEventService.ShouldSplit())
        {
            OnEventSet?.Invoke();
        }
    }
    
}