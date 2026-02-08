// 

using System;

namespace AutoHitCounter.Interfaces;

public interface ITickService
{
    void RegisterGameTick(Action tick);
    void UnregisterGameTick();
}