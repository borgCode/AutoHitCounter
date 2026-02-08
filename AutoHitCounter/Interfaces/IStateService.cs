// 

using System;
using AutoHitCounter.Enums;

namespace AutoHitCounter.Interfaces;

public interface IStateService
{
    public bool IsLoaded();
    void Publish(State eventType);
    void Subscribe(State eventType, Action handler);
    void Unsubscribe(State eventType, Action handler);
}