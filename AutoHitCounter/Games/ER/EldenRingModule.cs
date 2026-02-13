// 

using System;
using System.Collections.Generic;
using AutoHitCounter.Enums;
using AutoHitCounter.Interfaces;
using AutoHitCounter.Memory;
using static AutoHitCounter.Games.ER.EldenRingOffsets;

namespace AutoHitCounter.Games.ER;

public class EldenRingModule : IGameModule, IDisposable
{
    private readonly IMemoryService _memoryService;
    private readonly IStateService _stateService;
    private readonly HookManager _hookManager;
    private readonly ITickService _tickService;
    private readonly Dictionary<uint, string> _events;
    private EldenRingHitService _hitService;
    private EldenRingEventService _eventService;

    private DateTime? _lastHit;

    public event Action<int> OnHit;

    public event Action OnEventSet;
    public event Action<long> OnIgtChanged;

    private nint _igtPtr;

    public EldenRingModule(IMemoryService memoryService, IStateService stateService, HookManager hookManager,
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

        _hitService = new EldenRingHitService(_memoryService, _hookManager);
        _eventService = new EldenRingEventService(_memoryService, _hookManager, _events);
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
        EldenRingOffsets.Initialize(fileVersion, moduleBase);
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

        OnIgtChanged?.Invoke(_memoryService.Read<uint>(_igtPtr));
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