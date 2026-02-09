// 

namespace AutoHitCounter.Memory;

public static class CodeCaveOffsets
{
    public static nint Base;

    public const int Hit = 0x0;

    public const int HitCode = 0x10;
    public const int FallDamage = 0x200;
    public const int KillBox = 0x400;

    public const int EventLogWriteIdx = 0x1000;
    public const int EventLogCode = 0x1020;
    public const int EventLogBuffer = 0x1100; //0x1000
}