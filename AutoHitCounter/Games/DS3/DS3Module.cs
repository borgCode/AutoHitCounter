// 

using System;
using System.Collections.Generic;
using AutoHitCounter.Enums;
using AutoHitCounter.Interfaces;
using AutoHitCounter.Memory;
using AutoHitCounter.Utilities;
using static AutoHitCounter.Games.DS3.DS3Offsets;

namespace AutoHitCounter.Games.DS3;

public class DS3Module : IGameModule, IDisposable, IVersionedGameModule
{
    private readonly IMemoryService _memoryService;
    private readonly IStateService _stateService;
    private readonly HookManager _hookManager;
    private readonly ITickService _tickService;
    private readonly Dictionary<uint, string> _events;

    public string GameVersion => DS3Offsets.Version.GetDescription();

    private DateTime? _lastHit;
    private nint _igtPtr;
    private DS3HitService _hitService;
    private DS3EventService _eventService;
    private DS3SettingsService _settingsService;

    public event Action<int> OnHit;
    public event Action OnEventSet;
    public event Action<long> OnIgtChanged;
    public event Action OnVersionDetected;

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

        DS3CustomCodeOffsets.Base = _memoryService.AllocCustomCodeMem();

#if DEBUG
        Console.WriteLine($@"Code cave: 0x{(long)DS3CustomCodeOffsets.Base:X}");
#endif

        _hitService = new DS3HitService(_memoryService, _hookManager);
        _eventService = new DS3EventService(_memoryService, _hookManager, _events);
        _settingsService = new DS3SettingsService(_memoryService);

        ApplySettings(onlyEnabled: true);

        _eventService.InstallHook();
        _hitService.InstallHooks();

        _igtPtr = _memoryService.Read<nint>(GameDataMan.Base) + GameDataMan.Igt;

        _tickService.RegisterGameTick(Tick);

        OnVersionDetected?.Invoke();
    }

    private void InitializeOffsets()
    {
        if (_memoryService.TargetProcess == null) return;
        var module = _memoryService.TargetProcess.MainModule;
        var fileVersion = module?.FileVersionInfo.FileVersion;
        var moduleBase = _memoryService.BaseAddress;
        DS3Offsets.Initialize(fileVersion, moduleBase);
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
        var worldChrman = _memoryService.Read<nint>(WorldChrMan.Base);
        return _memoryService.Read<nint>(worldChrman + WorldChrMan.PlayerIns) != 0;
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

    public void ApplySettings(bool onlyEnabled = false)
    {
        var noLogo = SettingsManager.Default.DS3NoLogo;
        if (noLogo || !onlyEnabled) _settingsService.ToggleNoLogo(noLogo);

        var stutterFix = SettingsManager.Default.DS3StutterFix;
        if (stutterFix || !onlyEnabled) _settingsService.ToggleStutterFix(stutterFix);
    }
}