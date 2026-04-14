//

using System;
using System.Collections.Generic;
using System.Windows.Threading;
using AutoHitCounter.Interfaces;
using AutoHitCounter.Models;
using Stopwatch = System.Diagnostics.Stopwatch;

namespace AutoHitCounter.Games.Manual;

public class ManualGameModule : IGameModule, IDisposable
{
    private readonly Stopwatch _stopwatch = new();
    private readonly DispatcherTimer _timer;
    private long _offset;

    public long ElapsedMilliseconds => _offset + _stopwatch.ElapsedMilliseconds;

    public event Action OnHit;
    public event Action OnEventSet;
    public event Action<List<EventLogEntry>> OnEventLogEntriesReceived;
    public event Action<long> OnTimeChanged;
    public event Action OnRunStart;

    public ManualGameModule()
    {
        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _timer.Tick += (_, _) => OnTimeChanged?.Invoke(ElapsedMilliseconds);
    }

    public void StartTimer()
    {
        _stopwatch.Start();
        _timer.Start();
    }

    public void StopTimer()
    {
        _stopwatch.Stop();
        _timer.Stop();
    }

    public void ResetTimer()
    {
        _stopwatch.Reset();
        _offset = 0;
        OnTimeChanged?.Invoke(0);
    }

    public void SetElapsed(long milliseconds)
    {
        _stopwatch.Reset();
        _offset = milliseconds;
    }

    public void UpdateEvents(Dictionary<uint, (string Name, int Required, int Hit)> events) { }
    public void ApplySettings(bool onlyEnabled = false) { }
    public void SetEventLogEnabled(bool enabled) { }

    public void Dispose()
    {
        _timer.Stop();
        OnHit = null;
        OnEventSet = null;
        OnEventLogEntriesReceived = null;
        OnTimeChanged = null;
        OnRunStart = null;
    }
}
