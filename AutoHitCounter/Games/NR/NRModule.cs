// 

using System;
using System.Collections.Generic;
using AutoHitCounter.Enums;
using AutoHitCounter.Interfaces;
using AutoHitCounter.Memory;
using AutoHitCounter.Models;
using AutoHitCounter.Services;
using AutoHitCounter.Utilities;
using static AutoHitCounter.Games.NR.NRCustomCodeOffsets;

namespace AutoHitCounter.Games.NR;

public class NRModule : IGameModule, IDisposable, IVersionedGameModule
{
    private readonly IMemoryService _memoryService;
    private readonly IStateService _stateService;
    private readonly HookManager _hookManager;
    private readonly ITickService _tickService;
    private Dictionary<uint, (string Name, int Required, int Hit)> _events;
    
    private DateTime? _lastHit;
    private NRHitService _hitService;
    private NREventService _eventService;
    private NRSettingsService _settingsService;
    private EventLogReader _eventLogReader;

    public string GameVersion => NROffsets.IsAobFallback
        ? NROffsets.DetectedFileVersion ?? "Unknown Patch"
        : NROffsets.Version.GetDescription();
    
    public event Action OnHit;
    public event Action OnEventSet;
    public event Action<List<EventLogEntry>> OnEventLogEntriesReceived;
    public event Action<long> OnTimeChanged;
    public event Action OnRunStart;
    public event Action OnVersionDetected;
    
    public NRModule(IMemoryService memoryService, IStateService stateService, HookManager hookManager,
        ITickService tickService, Dictionary<uint, (string Name, int Required, int Hit)> events)
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

        Base = _memoryService.AllocCustomCodeMem();

#if DEBUG
        Console.WriteLine($@"Code cave: 0x{(long)Base:X}");
#endif

        _hitService = new NRHitService(_memoryService, _hookManager);
        _eventService = new NREventService(_memoryService, _hookManager, _events);
        _settingsService = new NRSettingsService(_memoryService);
        _eventLogReader = new EventLogReader(_memoryService,
            Base + EventLogWriteIdx,
            Base + EventLogBuffer);
        _eventLogReader.EntriesReceived += entries => OnEventLogEntriesReceived?.Invoke(entries);

        ApplySettings(onlyEnabled: true);

        _eventService.InstallHook();
        _hitService.InstallHooks();

        _tickService.RegisterGameTick(Tick);

        OnVersionDetected?.Invoke();
    }
    
    private void InitializeOffsets()
    {
        if (_memoryService.TargetProcess == null) return;
        var module = _memoryService.TargetProcess.MainModule;
        var fileVersion = module?.FileVersionInfo.FileVersion;
        NROffsets.Initialize(fileVersion, _memoryService);
        
#if DEBUG
        NROffsets.Print(_memoryService.BaseAddress);
#endif
    }
    
    private void Tick()
    {
        if (!IsLoaded())
        {
            _hitService.ResetFlags();
            return;
        }

        _hitService.EnsureHooksInstalled();

        if (_hitService.HasHit() && (_lastHit == null || (DateTime.Now - _lastHit.Value).TotalSeconds > 3))
        {
            OnHit?.Invoke();
            _lastHit = DateTime.Now;
        }

        if (_eventService.ShouldSplit())
        {
            OnEventSet?.Invoke();
        }

        _eventLogReader.Poll();
    }
    
    private bool IsLoaded()
    {
        var worldChrman = _memoryService.Read<nint>(NROffsets.WorldChrMan.Base);
        return _memoryService.Read<nint>(worldChrman + NROffsets.WorldChrMan.PlayerIns) != 0;
    }
    
    public void UpdateEvents(Dictionary<uint, (string Name, int Required, int Hit)> events)
    {
        _events = events;
        _eventService?.UpdateEvents(events);
    }

    public void SetEventLogEnabled(bool enabled)
    {
        if (_eventLogReader != null) _eventLogReader.IsEnabled = enabled;
    }

    public void ApplySettings(bool onlyEnabled = false)
    {
        var noLogo = SettingsManager.Default.NRNoLogo;
        if (noLogo || !onlyEnabled) _settingsService.ToggleNoLogo(noLogo);
    }
    
    public void Dispose()
    {
        _stateService.Unsubscribe(State.Attached, Initialize);
        _tickService.UnregisterGameTick();
        OnHit = null;
        OnEventSet = null;
        OnEventLogEntriesReceived = null;
        OnTimeChanged = null;
        OnRunStart = null;
    }
}