// 

using System;

namespace AutoHitCounter.Interfaces;

public interface IGameModule
{
    event Action<int> OnHit;
    event Action OnEventSet;
    event Action<long> OnIgtChanged;
}