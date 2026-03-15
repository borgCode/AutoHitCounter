// 

using System;
using System.Collections.Generic;
using AutoHitCounter.Enums;
using AutoHitCounter.Interfaces;
using AutoHitCounter.Memory;
using AutoHitCounter.Models;
using AutoHitCounter.Services;
using AutoHitCounter.Utilities;
using static AutoHitCounter.Games.SK.SKCustomCodeOffsets;
using static AutoHitCounter.Games.SK.SKOffsets;

namespace AutoHitCounter.Games.SK;

public class SKModule : IGameModule, IDisposable, IVersionedGameModule
{
    private readonly IMemoryService _memoryService;
    private readonly IStateService _stateService;
    private readonly HookManager _hookManager;
    private readonly ITickService _tickService;
    private readonly Dictionary<uint, (string Name, int Required, int Hit)> _events;
    private readonly IHitRulesProvider _rules;

    public string GameVersion => SKOffsets.Version.GetDescription();

    private DateTime? _lastHit;
    private SKHitService _hitService;
    private SKEventService _eventService;
    private SKSettingsService _settingsService;
    private EventLogReader _eventLogReader;

    public event Action<int> OnHit;
    public event Action OnEventSet;
    public event Action<List<EventLogEntry>> OnEventLogEntriesReceived;
    public event Action<long> OnTimeChanged;
    public event Action OnVersionDetected;

    public SKModule(IMemoryService memoryService, IStateService stateService, HookManager hookManager,
        ITickService tickService, Dictionary<uint, (string Name, int Required, int Hit)> events, IHitRulesProvider rules)
    {
        _memoryService = memoryService;
        _stateService = stateService;
        _hookManager = hookManager;
        _tickService = tickService;
        _events = events;
        _rules = rules;

        rules.OnHitRulesChanged += ApplyRules;

        stateService.Subscribe(State.Attached, Initialize);
        _lastHit = DateTime.Now;
    }

    private void ApplyRules()
    {
        _hitService?.SetRobertoStaggerCounts(_rules.GetRule("should_count_roberto"));
    }

    private void Initialize()
    {
        InitializeOffsets();


        Base = _memoryService.AllocCustomCodeMem();

#if DEBUG
        Console.WriteLine($@"Code cave: 0x{(long)Base:X}");
#endif

        _hitService = new SKHitService(_memoryService, _hookManager);
        _eventService = new SKEventService(_memoryService, _hookManager, _events);
        _settingsService = new SKSettingsService(_memoryService);
        _eventLogReader = new EventLogReader(_memoryService,
            Base + EventLogWriteIdx,
            Base + EventLogBuffer);
        _eventLogReader.EntriesReceived += entries => OnEventLogEntriesReceived?.Invoke(entries);

        ApplySettings(onlyEnabled: true);

        _eventService.InstallHook();
        _hitService.InstallHooks();
        
        ApplyRules();

        _tickService.RegisterGameTick(Tick);

        OnVersionDetected?.Invoke();
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

        _eventLogReader.Poll();

        var igtPtr  = _memoryService.Read<nint>(GameDataMan.Base) + GameDataMan.Igt;
        OnTimeChanged?.Invoke(_memoryService.Read<uint>(igtPtr));
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
        var noLogo = SettingsManager.Default.SKNoLogo;
        if (noLogo || !onlyEnabled) _settingsService.ToggleNoLogo(noLogo);

        var noTutorials = SettingsManager.Default.SKNoTutorials;
        if (noTutorials || !onlyEnabled) _settingsService.ToggleNoTutorials(noTutorials);
    }
}