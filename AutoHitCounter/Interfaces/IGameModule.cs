// 

using System;
using System.Collections.Generic;

namespace AutoHitCounter.Interfaces;

public interface IGameModule
{
    event Action<int> OnHit;
    event Action OnEventSet;
    event Action<long> OnIgtChanged;
    void UpdateEvents(Dictionary<uint, string> events);
    void ApplySettings(bool onlyEnabled = false);
}