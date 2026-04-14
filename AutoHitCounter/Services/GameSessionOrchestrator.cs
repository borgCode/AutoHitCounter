//

using System;
using System.Collections.Generic;
using AutoHitCounter.Enums;
using AutoHitCounter.Games.Manual;
using AutoHitCounter.Interfaces;
using AutoHitCounter.Models;

namespace AutoHitCounter.Services;

public class GameSessionOrchestrator : IGameSessionOrchestrator
{
    private readonly IMemoryService _memoryService;
    private readonly IHotkeyManager _hotkeyManager;
    private readonly IGameModuleFactory _gameModuleFactory;
    private IHitRulesProvider _hitRulesProvider;
    private Func<Dictionary<uint, (string Name, int Required, int Hit)>> _activeEventsProvider;

    private IGameModule _currentModule;
    private Game _activeGame;
    private bool _isAttached;
    private string _attachedText;
    private bool _eventLogEnabled;

    public GameSessionOrchestrator(
        IMemoryService memoryService,
        IHotkeyManager hotkeyManager,
        IGameModuleFactory gameModuleFactory,
        IStateService stateService)
    {
        _memoryService = memoryService;
        _hotkeyManager = hotkeyManager;
        _gameModuleFactory = gameModuleFactory;

        stateService.Subscribe(State.Attached, OnAttached);
        stateService.Subscribe(State.NotAttached, OnNotAttached);
    }

    public void Initialize(IHitRulesProvider hitRulesProvider,
        Func<Dictionary<uint, (string Name, int Required, int Hit)>> activeEventsProvider)
    {
        _hitRulesProvider = hitRulesProvider;
        _activeEventsProvider = activeEventsProvider;
    }

    public event Action HitReceived;
    public event Action RunStartDetected;
    public event Action EventSetDetected;
    public event Action<List<EventLogEntry>> EventLogEntries;
    public event Action<long> TimeChangedMs;
    public event Action AttachmentChanged;

    public Game ActiveGame => _activeGame;
    public bool IsAttached => _isAttached;
    public string AttachedText => _attachedText;

    public void Track(Game game)
    {
        _activeGame = game;
        SwapModule();
    }

    public void Stop()
    {
        (_currentModule as IDisposable)?.Dispose();
        _currentModule = null;
        _activeGame = null;
        _isAttached = false;
        _attachedText = "Not attached";
        AttachmentChanged?.Invoke();
    }

    public void UpdateEvents(Dictionary<uint, (string Name, int Required, int Hit)> events)
        => _currentModule?.UpdateEvents(events);

    public void ApplyCurrentSettings() => _currentModule?.ApplySettings();

    public void SetEventLogEnabled(bool enabled)
    {
        _eventLogEnabled = enabled;
        _currentModule?.SetEventLogEnabled(enabled);
    }

    public void ManualStart()
    {
        if (_currentModule is ManualGameModule m) m.StartTimer();
    }

    public void ManualStop()
    {
        if (_currentModule is ManualGameModule m) m.StopTimer();
    }

    public void ManualReset()
    {
        if (_currentModule is ManualGameModule m) m.ResetTimer();
    }

    public void ManualSetElapsed(long milliseconds)
    {
        if (_currentModule is ManualGameModule m) m.SetElapsed(milliseconds);
    }

    public void Dispose()
    {
        (_currentModule as IDisposable)?.Dispose();
        _currentModule = null;

        HitReceived = null;
        RunStartDetected = null;
        EventSetDetected = null;
        EventLogEntries = null;
        TimeChangedMs = null;
        AttachmentChanged = null;
    }

    private void SwapModule()
    {
        (_currentModule as IDisposable)?.Dispose();

        if (_activeGame == null) return;

        _currentModule = _gameModuleFactory.CreateModule(_activeGame, _activeEventsProvider(), _hitRulesProvider);
        _hotkeyManager.SetManualGameActive(_activeGame.IsManual);

        if (_activeGame.IsManual)
        {
            _isAttached = true;
            _attachedText = $"Custom Game: {_activeGame.GameName}";
            AttachmentChanged?.Invoke();
        }
        else
        {
            if (_currentModule is IVersionedGameModule versioned)
            {
                var game = _activeGame;
                versioned.OnVersionDetected += () =>
                {
                    var version = versioned.GameVersion;
                    _attachedText = string.IsNullOrEmpty(version)
                        ? $"Attached to {game.GameName}"
                        : $"Attached to {game.GameName} ({version})";
                    AttachmentChanged?.Invoke();
                };
            }

            _memoryService.StartAutoAttach(_activeGame.ProcessName);
        }

        _currentModule.OnHit += () => HitReceived?.Invoke();
        _currentModule.OnRunStart += () => RunStartDetected?.Invoke();
        _currentModule.OnEventSet += () => EventSetDetected?.Invoke();
        _currentModule.OnEventLogEntriesReceived += entries => EventLogEntries?.Invoke(entries);
        _currentModule.OnTimeChanged += ms => TimeChangedMs?.Invoke(ms);

        if (_eventLogEnabled)
            _currentModule.SetEventLogEnabled(true);
    }

    private void OnAttached()
    {
        _isAttached = true;
        if (_activeGame != null)
            _attachedText = $"Attached to {_activeGame.GameName}";
        AttachmentChanged?.Invoke();
    }

    private void OnNotAttached()
    {
        if (_activeGame?.IsManual == true) return;
        _isAttached = false;
        _attachedText = _activeGame != null ? $"Waiting for {_activeGame.GameName}..." : "Not attached";
        AttachmentChanged?.Invoke();
    }
}
