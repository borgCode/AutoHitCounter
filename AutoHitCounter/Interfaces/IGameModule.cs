// 

using System;
using System.Collections.Generic;
using AutoHitCounter.Models;

namespace AutoHitCounter.Interfaces;

public interface IGameModule
{
    event Action OnHit;
    event Action OnEventSet;
    event Action<List<EventLogEntry>> OnEventLogEntriesReceived;
    event Action<long> OnTimeChanged;
    event Action OnRunStart;
    void UpdateEvents(Dictionary<uint, (string Name, int Required, int Hit)> events);
    void ApplySettings(bool onlyEnabled = false);
    void SetEventLogEnabled(bool enabled);
}