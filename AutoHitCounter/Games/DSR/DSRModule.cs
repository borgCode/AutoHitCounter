// 

using System;
using System.Collections.Generic;
using System.IO;
using AutoHitCounter.Enums;
using AutoHitCounter.Interfaces;
using AutoHitCounter.Memory;
using AutoHitCounter.Utilities;
using static AutoHitCounter.Games.DSR.DSROffsets;

namespace AutoHitCounter.Games.DSR;

public class DSRModule : IGameModule, IDisposable, IVersionedGameModule
{
    private readonly IMemoryService _memoryService;
    private readonly IStateService _stateService;
    private readonly HookManager _hookManager;
    private readonly ITickService _tickService;
    private readonly Dictionary<uint, string> _events;
    
    public string GameVersion => DSROffsets.Version.GetDescription();

    private DateTime? _lastHit;
    private nint _igtPtr;
    private DSRHitService _hitService;
    private DSREventService _eventService;
    
    public event Action<int> OnHit;
    public event Action OnEventSet;
    public event Action<long> OnIgtChanged;
    public event Action OnVersionDetected;
    
    public DSRModule(IMemoryService memoryService, IStateService stateService, HookManager hookManager,
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
        OnVersionDetected?.Invoke();

        DSRCustomCodeOffsets.Base = _memoryService.AllocCustomCodeMem();
        
#if DEBUG
        Console.WriteLine($@"Code cave: 0x{(long)DSRCustomCodeOffsets.Base:X}");
#endif

         _hitService = new DSRHitService(_memoryService, _hookManager);
         _eventService = new DSREventService(_memoryService, _hookManager, _events);
         _eventService.InstallHook();
         _hitService.InstallHooks();
         _igtPtr = _memoryService.Read<nint>(GameDataMan.Base) + GameDataMan.Igt;
         _tickService.RegisterGameTick(Tick);
    }
    
    private void InitializeOffsets()
    {
        var module = _memoryService.TargetProcess?.MainModule;
        if (module == null) return;
        var fileInfo = new FileInfo(module.FileName);
        var fileSize = fileInfo.Length;
        var moduleBase = _memoryService.BaseAddress;
        DSROffsets.Initialize(fileSize, moduleBase);
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
        var worldChrMan = _memoryService.Read<nint>(WorldChrMan.Base);
        return _memoryService.Read<nint>(worldChrMan + WorldChrMan.PlayerIns) != 0;
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