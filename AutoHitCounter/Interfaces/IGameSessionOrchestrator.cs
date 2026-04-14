//

using System;
using System.Collections.Generic;
using AutoHitCounter.Models;

namespace AutoHitCounter.Interfaces;

public interface IGameSessionOrchestrator : IDisposable
{
    event Action HitReceived;
    event Action RunStartDetected;
    event Action EventSetDetected;
    event Action<List<EventLogEntry>> EventLogEntries;
    event Action<long> TimeChangedMs;
    event Action AttachmentChanged;

    void Initialize(IHitRulesProvider hitRulesProvider,
        Func<Dictionary<uint, (string Name, int Required, int Hit)>> activeEventsProvider);

    Game ActiveGame { get; }
    bool IsAttached { get; }
    string AttachedText { get; }

    void Track(Game game);
    void Stop();
    void UpdateEvents(Dictionary<uint, (string Name, int Required, int Hit)> events);
    void ApplyCurrentSettings();
    void SetEventLogEnabled(bool enabled);

    void ManualStart();
    void ManualStop();
    void ManualReset();
    void ManualSetElapsed(long milliseconds);
}
