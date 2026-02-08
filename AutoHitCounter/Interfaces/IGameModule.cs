// 

using System;

namespace AutoHitCounter.Interfaces;

public interface IGameModule
{
    void Initialize();
    
    event Action<int> OnHit;
    event Action OnBossKilled;
}