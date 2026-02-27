// 

namespace AutoHitCounter.Games.DS3;

public static class DS3CustomCodeOffsets
{
    public static nint Base;
    
    public const int CheckAuxProcFlag = 0x0;

    public const int Hit = 0x10;
    public const int CheckPlayerDead = 0x20;

    public const int HitCode = 0x100;
    public const int LethalFall = 0x400;
    public const int CheckAuxAttacker = 0x500;
    public const int AuxProc = 0x600;
    public const int GetSpEffect = 0x660;

    public const int LastJailerCountTime = 0x700;
    public const int JailerDrain = 0x710;
    public const int FallDamage = 0x800;
    
    
    public const int EventLogWriteIdx = 0x2000;
    public const int EventLogCode = 0x2020;
    public const int EventLogBuffer = 0x2100;


}