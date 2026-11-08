//

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using AutoHitCounter.Enums;
using AutoHitCounter.Interfaces;
using AutoHitCounter.Memory;
using AutoHitCounter.Models;
using AutoHitCounter.Services;
using AutoHitCounter.Utilities;
using static AutoHitCounter.Games.BB.BBCustomCodeOffsets;

namespace AutoHitCounter.Games.BB;

public class BBModule : IGameModule, IDisposable, IVersionedGameModule
{
    private static readonly Regex TitleVersion = new(@"(CUSA\d{5}).*<([\d.]+)>");
    private static readonly byte[] EbootSignature = "/libexec/ld-elf.so.1\0"u8.ToArray();

    private readonly IMemoryService _memoryService;
    private readonly IStateService _stateService;
    private readonly HookManager _hookManager;
    private readonly ITickService _tickService;
    private Dictionary<uint, (string Name, int Required, int Hit)> _events;

    private DateTime? _lastHit;
    private BBHitService _hitService;
    private BBEventService _eventService;
    private BBEmevdEventService _emevdEventService;
    private BBRunStartService _runStartService;
    private BBSettingsService _settingsService;
    private EventLogReader _eventLogReader;
    private EventLogReader _emevdEventLogReader;

    public string GameVersion { get; private set; }

    public event Action OnHit;
    public event Action OnEventSet;
    public event Action<List<EventLogEntry>> OnEventLogEntriesReceived;
    public event Action<long> OnTimeChanged;
    public event Action OnRunStart;
    public event Action OnVersionDetected;

    public BBModule(IMemoryService memoryService, IStateService stateService, HookManager hookManager,
        ITickService tickService, Dictionary<uint, (string Name, int Required, int Hit)> events)
    {
        _memoryService = memoryService;
        _stateService = stateService;
        _hookManager = hookManager;
        _tickService = tickService;
        _events = events;

        stateService.Subscribe(State.Attached, OnAttached);
        _lastHit = DateTime.Now;
    }

    private void OnAttached()
    {
        _tickService.RegisterGameTick(WaitForEboot);
    }

    private void WaitForEboot()
    {
        var ebootBase = _memoryService.FindAllocationStartingWith(EbootSignature);
        if (ebootBase != 0 && _memoryService.IsExecutable(ebootBase))
            Initialize(ebootBase);
    }

    private void Initialize(nint ebootBase)
    {
        GameVersion = ParseVersion(GetWindowTitle());

        BBOffsets.Initialize(ebootBase);

        _settingsService = new BBSettingsService(_memoryService);
        ApplySettings(onlyEnabled: true);

        Base = _memoryService.AllocCustomCodeMem();

#if DEBUG
        Console.WriteLine($@"eboot.bin base: 0x{(long)ebootBase:X}");
        BBOffsets.Print(ebootBase);
        Console.WriteLine($@"Code cave: 0x{(long)Base:X}");
#endif

        _hitService = new BBHitService(_memoryService, _hookManager);
        _eventService = new BBEventService(_memoryService, _hookManager, _events);
        _eventLogReader = new EventLogReader(_memoryService,
            Base + EventLogWriteIdx,
            Base + EventLogBuffer);
        _eventLogReader.EntriesReceived += entries => OnEventLogEntriesReceived?.Invoke(entries);
        _emevdEventService = new BBEmevdEventService(_memoryService, _hookManager, _events);
        _emevdEventLogReader = new EventLogReader(_memoryService,
            Base + EmevdEventLogWriteIdx,
            Base + EmevdEventLogBuffer);
        _emevdEventLogReader.EntriesReceived += entries => OnEventLogEntriesReceived?.Invoke(entries);
        _runStartService = new BBRunStartService(_memoryService, _hookManager);

        _eventService.InstallHook();
        _emevdEventService.InstallHook();
        _hitService.InstallHooks();
        _runStartService.InstallHook();

        _tickService.RegisterGameTick(Tick);

        OnVersionDetected?.Invoke();
    }

    private string GetWindowTitle()
    {
        var process = _memoryService.TargetProcess;
        if (process == null) return null;

        process.Refresh();
        return process.MainWindowTitle;
    }

    private static string ParseVersion(string windowTitle)
    {
        var match = TitleVersion.Match(windowTitle ?? "");
        return match.Success ? $"{match.Groups[1].Value} {match.Groups[2].Value}" : "Unknown version";
    }

    private void Tick()
    {
        if (_runStartService.IsNewGameStarted()) OnRunStart?.Invoke();

        if (!IsLoaded()) return;

        if (_hitService.HasHit() && (_lastHit == null || (DateTime.Now - _lastHit.Value).TotalSeconds > 3))
        {
            OnHit?.Invoke();
            _lastHit = DateTime.Now;
        }

        if (_eventService.ShouldSplit())
        {
            OnEventSet?.Invoke();
        }

        if (_emevdEventService.ShouldSplit())
        {
            OnEventSet?.Invoke();
        }

        _eventLogReader.Poll();
        _emevdEventLogReader.Poll();

        var igtPtr = _memoryService.Read<nint>(BBOffsets.GameDataMan.Base) + BBOffsets.GameDataMan.Igt;
        OnTimeChanged?.Invoke(_memoryService.Read<uint>(igtPtr));
    }

    private bool IsLoaded()
    {
        var worldChrMan = _memoryService.Read<nint>(BBOffsets.WorldChrMan.Base);
        return _memoryService.Read<nint>(worldChrMan + BBOffsets.WorldChrMan.PlayerIns) != 0;
    }

    public void UpdateEvents(Dictionary<uint, (string Name, int Required, int Hit)> events)
    {
        _events = events;
        _eventService?.UpdateEvents(events);
        _emevdEventService?.UpdateEvents(events);
    }

    public void SetEventLogEnabled(bool enabled)
    {
        if (_eventLogReader != null) _eventLogReader.IsEnabled = enabled;
        if (_emevdEventLogReader != null) _emevdEventLogReader.IsEnabled = enabled;
    }

    public void ApplySettings(bool onlyEnabled = false)
    {
        if (_settingsService == null) return;
        var noLogo = SettingsManager.Default.BBNoLogo;
        if (noLogo || !onlyEnabled) _settingsService.ToggleNoLogo(noLogo);
    }

    public void Dispose()
    {
        _stateService.Unsubscribe(State.Attached, OnAttached);
        _tickService.UnregisterGameTick();
        OnHit = null;
        OnEventSet = null;
        OnEventLogEntriesReceived = null;
        OnTimeChanged = null;
        OnRunStart = null;
    }
}
