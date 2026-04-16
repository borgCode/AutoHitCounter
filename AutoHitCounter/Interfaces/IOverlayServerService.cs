//

using AutoHitCounter.Models;

namespace AutoHitCounter.Interfaces;

public interface IOverlayServerService
{
    void Start();
    void Stop();
    void BroadcastState(OverlayState state);
    void BroadcastIgt(string formatted);
    void BroadcastConfig(OverlayConfig config);
}
