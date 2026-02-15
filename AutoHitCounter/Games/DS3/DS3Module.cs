// 

using System;
using System.Collections.Generic;
using AutoHitCounter.Enums;
using AutoHitCounter.Interfaces;
using AutoHitCounter.Memory;

namespace AutoHitCounter.Games.DS3;

public class DS3Module : IGameModule
{
    private readonly IMemoryService _memoryService;
    private readonly IStateService _stateService;
    private readonly HookManager _hookManager;
    private readonly ITickService _tickService;
    private readonly Dictionary<uint, string> _events;
    
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

        _hitService = new DS3HitService(_memoryService, _hookManager);
        _eventService = new DS3EventService(_memoryService, _hookManager, _events);
        // _eventService.InstallHook();
        // _hitService.InstallHooks();
        // _igtPtr = _memoryService.Read<nint>(GameDataMan.Base) + GameDataMan.Igt;
        // _tickService.RegisterGameTick(Tick);
    }
    
    private void InitializeOffsets()
    {
        if (_memoryService.TargetProcess == null) return;
        var module = _memoryService.TargetProcess.MainModule;
        var fileVersion = module?.FileVersionInfo.FileVersion;
        var moduleBase = _memoryService.BaseAddress;
        DS3Offsets.Initialize(fileVersion, moduleBase);
    }

    
}