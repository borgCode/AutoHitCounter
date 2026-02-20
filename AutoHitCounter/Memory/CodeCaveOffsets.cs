// 

namespace AutoHitCounter.Memory;

public static class CodeCaveOffsets
{
    public static nint Base;

    public const int Hit = 0x0;

    public const int HitCode = 0x10;
    public const int FallDamage = 0x200;
    public const int KillBox = 0x400;
    public const int CheckAuxProcFlag = 0x501;
    public const int StaggerCheckFlag = 0x502;
    public const int CheckAuxAttacker = 0x510;
    public const int AuxProc = 0x570;
    public const int SpEffectTickDamage = 0x600;
    public const int StaggerEndure = 0x700;
    public const int EnvKilling = 0x760;

    public const int EventLogWriteIdx = 0x1000;
    public const int EventLogCode = 0x1020;
    public const int EventLogBuffer = 0x1100; //0x1000

    public const int IgtState = 0x2110;
    public const int IgtNewGameCode = 0x2120;
    public const int IgtStopCode = 0x2200;
    public const int IgtLoadGameCode = 0x2260;
}