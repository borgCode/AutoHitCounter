// 

namespace AutoHitCounter.Games.DS2;

public static class DS2CustomCodeOffsets
{
    public static nint Base;

    public const int CheckAuxProcFlag = 0x0;
    public const int ShouldIgnoreShulvaSpikesFlag = 0x1;
    
    public const int Hit = 0x10;

    public const int HitCode = 0x20;
    public const int GeneralApplyDamage = 0x200;
    public const int KillBox = 0x400;
    public const int CountAuxHit = 0x500;
    public const int LightPoiseStagger = 0x560;

    public const int CheckPlayerDead = 0x700;
    
    public const int EventLogWriteIdx = 0x2000;
    public const int EventLogCode = 0x2020;
    public const int EventLogBuffer = 0x2100; //0x1000

    public const int IgtState = 0x3110;
    public const int IgtNewGameCode = 0x3120;
    public const int IgtStopCode = 0x3200;
    public const int IgtLoadGameCode = 0x3260;

    public const int BabyJump = 0x3600;

    public const int CreditSkipFlag = 0x3700;
    public const int CreditSkip = 0x3710;
}