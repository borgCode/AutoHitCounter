// 

namespace AutoHitCounter.Games.SK;

public class SKCustomCodeOffsets
{
    public static nint Base;

    public const int CheckDeflectFlag = 0x0;

    public const int Hit = 0x10;
    public const int CheckPlayerDead = 0x20;

    public const int HitCode = 0x100;
    public const int DeflectCheck = 0x400;
    public const int LethalFall = 0x500;
    public const int FadeFall = 0x580;
    public const int ApplyHealthDelta = 0x600;
}