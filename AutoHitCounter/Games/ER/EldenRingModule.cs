// 

using System;
using System.Collections.Generic;
using AutoHitCounter.Enums;
using AutoHitCounter.Interfaces;
using AutoHitCounter.Memory;
using AutoHitCounter.Models;
using AutoHitCounter.Services;
using AutoHitCounter.Utilities;
using static AutoHitCounter.Games.ER.EldenRingCustomCodeOffsets;
using static AutoHitCounter.Games.ER.EldenRingOffsets;

namespace AutoHitCounter.Games.ER;

public class EldenRingModule : IGameModule, IDisposable, IVersionedGameModule
{
    private readonly IMemoryService _memoryService;
    private readonly IStateService _stateService;
    private readonly HookManager _hookManager;
    private readonly ITickService _tickService;
    private readonly Dictionary<uint, (string Name, int Required, int Hit)> _events;
    private EldenRingHitService _hitService;
    private EldenRingEventService _eventService;
    private EldenRingSettingsService _settingsService;
    private EventLogReader _eventLogReader;
    private IRunStartService _runStartService;
    
    public string GameVersion => EldenRingOffsets.Version.GetDescription();

    private DateTime? _lastHit;

    public event Action OnHit;

    public event Action OnEventSet;
    public event Action<List<EventLogEntry>> OnEventLogEntriesReceived;
    public event Action<long> OnTimeChanged;
    public event Action OnRunStart;
    public event Action OnVersionDetected;
    
    public EldenRingModule(IMemoryService memoryService, IStateService stateService, HookManager hookManager,
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
        
        _hitService = new EldenRingHitService(_memoryService, _hookManager);
        _eventService = new EldenRingEventService(_memoryService, _hookManager, _events);
        _settingsService = new EldenRingSettingsService(_memoryService);
        _eventLogReader = new EventLogReader(_memoryService,
            Base + EventLogWriteIdx,
            Base + EventLogBuffer);
        _runStartService = new EldenRingRunStartService(_memoryService, _hookManager);
        _eventLogReader.EntriesReceived += entries => OnEventLogEntriesReceived?.Invoke(entries);

        ApplySettings(onlyEnabled: true);
        
        _eventService.InstallHook();
        _hitService.InstallHooks();
        _runStartService.InstallHook();
        
        _tickService.RegisterGameTick(Tick);
        
        OnVersionDetected?.Invoke();
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
        if (_runStartService.IsNewGameStarted()) OnRunStart?.Invoke();
        
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

        var igtPtr  = _memoryService.Read<nint>(GameDataMan.Base) + GameDataMan.Igt;
        OnTimeChanged?.Invoke(_memoryService.Read<long>(igtPtr));
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
        OnEventLogEntriesReceived = null;
        OnTimeChanged = null;
        OnRunStart = null;
    }
    
    public void UpdateEvents(Dictionary<uint, (string Name, int Required, int Hit)> events)
    {
        _eventService?.UpdateEvents(events);
    }

    public void SetEventLogEnabled(bool enabled)
    {
        if (_eventLogReader != null) _eventLogReader.IsEnabled = enabled;
    }

    public void ApplySettings(bool onlyEnabled = false)
    {
        var noLogo = SettingsManager.Default.ERNoLogo;
        if (noLogo || !onlyEnabled) _settingsService.ToggleNoLogo(noLogo);

        var stutterFix = SettingsManager.Default.ERStutterFix;
        if (stutterFix || !onlyEnabled) _settingsService.ToggleStutterFix(stutterFix);

        var disableAchievements = SettingsManager.Default.ERDisableAchievements;
        if (disableAchievements || !onlyEnabled) _settingsService.ToggleDisableAchievements(disableAchievements);
    }
}
