// 

using System;
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
    
    private DateTime? _lastHit;
    
    public event Action<int> OnHit;
    public event Action OnEventSet;
    public event Action<long> OnIgtChanged;

    public DS2ScholarModule(IMemoryService memoryService, IStateService stateService, HookManager hookManager, ITickService tickService)
    {
        _memoryService = memoryService;
        _stateService = stateService;
        _hookManager = hookManager;
        _tickService = tickService;
        
        stateService.Subscribe(State.Attached, Initialize);
        _lastHit = DateTime.Now;
    }

    private void Initialize()
    {
        InitializeOffsets();
        
        _hitService = new DS2ScholarHitService(_memoryService, _hookManager);
        _hitService.InstallHooks();
        
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

    private int _testHitCount = 0;
    private void Tick()
    {
        if (_hitService.HasHit())
        {
            if (_lastHit != null && (DateTime.Now - _lastHit.Value).TotalSeconds < 3) return;
            // OnHit?.Invoke(1);
            _testHitCount++;
            Console.WriteLine($"Test hit: {_testHitCount}");
            _lastHit = DateTime.Now;
        }

        // if (_eventService.ShouldSplit())
        // {
        //     OnEventSet?.Invoke();
        // }
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