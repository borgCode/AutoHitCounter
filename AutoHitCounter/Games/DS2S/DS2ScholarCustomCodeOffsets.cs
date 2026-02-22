// 

namespace AutoHitCounter.Games.DS2S;

public static class DS2ScholarCustomCodeOffsets
{
    public static nint Base;

    public const int CheckAuxProcFlag = 0x0;
    
    public const int Hit = 0x10;

    public const int HitCode = 0x20;
    public const int GeneralApplyDamage = 0x200;
    public const int KillBox = 0x400;
    public const int CountAuxHit = 0x500;
    public const int LightPoiseStagger = 0x560;
    
    public const int StaggerEndure = 0x700;
    public const int EnvKilling = 0x760;
    public const int StateInfoCheck = 0x850;
    public const int DeflectTearCheck = 0x900;

    public const int EventLogWriteIdx = 0x1000;
    public const int EventLogCode = 0x1020;
    public const int EventLogBuffer = 0x1100; //0x1000

    public const int IgtState = 0x2110;
    public const int IgtNewGameCode = 0x2120;
    public const int IgtStopCode = 0x2200;
    public const int IgtLoadGameCode = 0x2260;
}