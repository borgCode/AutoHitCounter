// 

using System;
using System.Collections.Generic;
using System.IO;
using AutoHitCounter.Enums;
using AutoHitCounter.Interfaces;
using AutoHitCounter.Memory;
using AutoHitCounter.Utilities;

namespace AutoHitCounter.Games.DS2;

public class DS2Module : IGameModule, IDisposable, IVersionedGameModule
{
    private readonly IMemoryService _memoryService;
    private readonly IStateService _stateService;
    private readonly HookManager _hookManager;
    private readonly ITickService _tickService;
    private readonly Dictionary<uint, string> _events;
    private DS2HitService _hitService;
    private DS2EventService _eventService;
    private DS2SettingsService _settingsService;
    private DS2IgtService _igtService;
    private readonly IHitRulesProvider _rules;

    public string GameVersion => DS2Offsets.Version.GetDescription();

    private DateTime? _lastHit;

    public event Action<int> OnHit;
    public event Action OnEventSet;
    public event Action<long> OnIgtChanged;
    public event Action OnVersionDetected;

    public DS2Module(IMemoryService memoryService, IStateService stateService, HookManager hookManager,
        ITickService tickService, Dictionary<uint, string> events, IHitRulesProvider rules)
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

        DS2CustomCodeOffsets.Base = _memoryService.AllocCustomCodeMem();

#if DEBUG
        Console.WriteLine($@"Code cave: 0x{(long)DS2CustomCodeOffsets.Base:X}");
#endif

        _hitService = new DS2HitService(_memoryService, _hookManager);
        _eventService = new DS2EventService(_memoryService, _hookManager, _events);
        _settingsService = new DS2SettingsService(_memoryService, _hookManager);
        _igtService = new DS2IgtService(_memoryService, _hookManager);

        ApplySettings(onlyEnabled: true);

        _hitService.InstallHooks();
        _eventService.InstallHook();
        _igtService.InstallHooks();

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
        if (_hitService.HasHit() && (_lastHit == null || (DateTime.Now - _lastHit.Value).TotalSeconds > 3))
        {
            OnHit?.Invoke(1);
            _lastHit = DateTime.Now;
        }

        if (_eventService.ShouldSplit())
        {
            OnEventSet?.Invoke();
        }

        _igtService.Update();
        OnIgtChanged?.Invoke(_igtService.ElapsedMilliseconds);
    }

    public void Dispose()
    {
        _rules.OnHitRulesChanged -= ApplyRules;
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