// 

using System;
using System.Collections.Generic;
using System.IO;
using AutoHitCounter.Enums;
using AutoHitCounter.Interfaces;
using AutoHitCounter.Memory;

namespace AutoHitCounter.Games.DS2S;

public class DS2ScholarModule : IGameModule, IDisposable
{
    private readonly IMemoryService _memoryService;
    private readonly IStateService _stateService;
    private readonly HookManager _hookManager;
    private readonly ITickService _tickService;
    private DS2ScholarHitService _hitService;
    private DS2ScholarEventService _eventService;
    private readonly Dictionary<uint, string> _events;
    
    private DateTime? _lastHit;
    
    public event Action<int> OnHit;
    public event Action OnEventSet;
    public event Action<long> OnIgtChanged;

    public DS2ScholarModule(IMemoryService memoryService, IStateService stateService, HookManager hookManager,
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
        
        _hitService = new DS2ScholarHitService(_memoryService, _hookManager);
        _hitService.InstallHooks();
        _eventService = new DS2ScholarEventService(_memoryService, _hookManager, _events);
        _eventService.InstallHook();
        
        _tickService.RegisterGameTick(Tick);
    }

    private void InitializeOffsets()
    {
        var module = _memoryService.TargetProcess?.MainModule;
        if (module == null) return;
        var fileInfo = new FileInfo(module.FileName);
        var fileSize = fileInfo.Length;
        var moduleBase = _memoryService.BaseAddress;
        DS2ScholarOffsets.Initialize(fileSize, moduleBase);
    }
    
    private void Tick()
    {
        if (_hitService.HasHit() && _lastHit != null && (DateTime.Now - _lastHit.Value).TotalSeconds < 3)
        {
            OnHit?.Invoke(1);
            _lastHit = DateTime.Now;
        }

        if (_eventService.ShouldSplit())
        {
            OnEventSet?.Invoke();
        }
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