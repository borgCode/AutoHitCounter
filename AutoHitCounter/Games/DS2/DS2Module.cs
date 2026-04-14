// 

using System;
using System.Collections.Generic;
using System.IO;
using AutoHitCounter.Enums;
using AutoHitCounter.Interfaces;
using AutoHitCounter.Memory;
using AutoHitCounter.Models;
using AutoHitCounter.Services;
using AutoHitCounter.Utilities;
using static AutoHitCounter.Games.DS2.DS2CustomCodeOffsets;
using static AutoHitCounter.Games.DS2.DS2Offsets;

namespace AutoHitCounter.Games.DS2;

public class DS2Module : IGameModule, IDisposable, IVersionedGameModule
{
    private readonly IMemoryService _memoryService;
    private readonly IStateService _stateService;
    private readonly HookManager _hookManager;
    private readonly ITickService _tickService;
    private readonly Dictionary<uint, (string Name, int Required, int Hit)> _events;
    private DS2HitService _hitService;
    private DS2EventService _eventService;
    private DS2SettingsService _settingsService;
    private DS2IgtService _igtService;
    private EventLogReader _eventLogReader;
    private readonly IHitRulesProvider _rules;

    public string GameVersion => DS2Offsets.Version.GetDescription();

    private DateTime? _lastHit;

    public event Action OnHit;
    public event Action OnEventSet;
    public event Action<List<EventLogEntry>> OnEventLogEntriesReceived;
    public event Action<long> OnTimeChanged;
    public event Action OnRunStart;
    public event Action OnVersionDetected;

    public DS2Module(IMemoryService memoryService, IStateService stateService, HookManager hookManager,
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
        _hitService?.SetIsShulvaSpikesIgnored(_rules.GetRule("ignore_shulva_spikes"));
    }

    private void Initialize()
    {
        InitializeOffsets();

        Base = _memoryService.AllocCustomCodeMem();

#if DEBUG
        Console.WriteLine($@"Code cave: 0x{(long)Base:X}");
#endif

        _hitService = new DS2HitService(_memoryService, _hookManager);
        _eventService = new DS2EventService(_memoryService, _hookManager, _events);
        _settingsService = new DS2SettingsService(_memoryService, _hookManager);
        _igtService = new DS2IgtService(_memoryService, _hookManager, OnRunStart);
        _eventLogReader = new EventLogReader(_memoryService,
            Base + EventLogWriteIdx,
            Base + EventLogBuffer);
        _eventLogReader.EntriesReceived += entries => OnEventLogEntriesReceived?.Invoke(entries);

        ApplySettings(onlyEnabled: true);

        _hitService.ClearHooks();
        _hitService.InstallHooks();
        _eventService.InstallHook();
        _igtService.InstallHooks();
        _igtService.InitializeFromCurrentState();

        ApplyRules();

        _tickService.RegisterGameTick(Tick);
        
        OnVersionDetected?.Invoke();
    }

    private void InitializeOffsets()
    {
        var module = _memoryService.TargetProcess?.MainModule;
        if (module == null) return;
        var fileInfo = new FileInfo(module.FileName);
        var fileSize = fileInfo.Length;
        var moduleBase = _memoryService.BaseAddress;
        DS2Offsets.Initialize(fileSize, moduleBase);
    }

    private void Tick()
    {
        
        
        if (!IsLoaded()) _hitService.ResetFlags();
        
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

        _igtService.Update();
        OnTimeChanged?.Invoke(_igtService.ElapsedMilliseconds);
    }

    private bool IsLoaded()
    {
        if (IsScholar)
        {
            var gameMan = _memoryService.Read<nint>(GameManagerImp.Base);
            return _memoryService.Read<nint>(gameMan + GameManagerImp.PlayerCtrl) != IntPtr.Zero;
        }
        else
        {
            var gameMan = _memoryService.Read<int>(GameManagerImp.Base);
            return (nint)_memoryService.Read<int>(gameMan + GameManagerImp.PlayerCtrl) != IntPtr.Zero;
        }
    }
    
    public void Dispose()
    {
        _rules.OnHitRulesChanged -= ApplyRules;
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
        var noBabyJump = SettingsManager.Default.DS2NoBabyJump;
        if (noBabyJump || !onlyEnabled) _settingsService.ToggleBabyJumpFix(noBabyJump);

        var skipCredits = SettingsManager.Default.DS2SkipCredits;
        if (skipCredits || !onlyEnabled) _settingsService.ToggleCreditSkip(skipCredits);

        var disableDoubleClick = SettingsManager.Default.DS2DisableDoubleClick;
        if (disableDoubleClick || !onlyEnabled) _settingsService.ToggleDoubleClick(disableDoubleClick);
    }
}
