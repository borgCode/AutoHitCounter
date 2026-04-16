//

using System;
using AutoHitCounter.Enums;

namespace AutoHitCounter.Interfaces;

public interface IHotkeyManager
{
    void RegisterAction(HotkeyActions actionId, Action action);
    void SetManualGameActive(bool isActive);
}