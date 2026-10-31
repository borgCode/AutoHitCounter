//

namespace AutoHitCounter.Games.BB;

public class BBCustomCodeOffsets
{
    public static nint Base;

    public const int Hit = 0x10;

    public const int IsPlayerDead = 0x20;
    public const int ReadThrowState = 0x60;
    public const int HasItem = 0x90;

    public const int ProcessHit = 0x100;
    public const int ApplySpEffectAndProcessStatus = 0x300;
    public const int KillBox = 0x500;
    public const int LandingHpCheck = 0x600;
    public const int SpEffectHpCheck = 0x700;

    public const int RunStartFlag = 0x2000;
    public const int RunStartCode = 0x2010;

    public const int EventLogWriteIdx = 0x3000;
    public const int EventLogCode = 0x3020;
    public const int EventLogBuffer = 0x3100;

    public const int EmevdEventLogWriteIdx = 0x3C00;
    public const int EmevdEventLogCode = 0x3C20;
    public const int EmevdEventLogBuffer = 0x3D00;
}
