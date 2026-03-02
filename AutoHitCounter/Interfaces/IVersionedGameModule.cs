using System;

namespace AutoHitCounter.Interfaces;

public interface IVersionedGameModule
{
    string GameVersion { get; }
    event Action OnVersionDetected;
}