// 

namespace AutoHitCounter.Games.DS3;

public static class DS3CustomCodeOffsets
{
    public static nint Base;
    
    public const int CheckAuxProcFlag = 0x0;
    public const int CheckStaggerFlag = 0x1;
    public const int FallHitCountedFlag = 0x2;
    public const int InThrowFlag = 0x3;
    
    public const int Hit = 0x10;
    
    public const int CheckPlayerDead = 0x20;

    public const int HitCode = 0x100;
    public const int StoredFallHeight = 0x400;
    public const int StoreFallHeight = 0x404;
    public const int CheckAuxAttacker = 0x500;
    public const int AuxProc = 0x600;
    public const int GetSpEffect = 0x660;

    public const int LastJailerCountTime = 0x700;
    public const int JailerDrain = 0x710;
    public const int ApplyHealthDelta = 0x800;
    public const int KillBox = 0xA00;
    public const int CheckStagger = 0xC00;
    public const int LethalFallCheck = 0xE00;
    public const int SetThrowState = 0x1000;
    public const int ClearThrowState = 0x1100;
    
    public const int RunStartFlag = 0x2000;
    public const int RunStartCode = 0x2010;
    
    public const int EventLogWriteIdx = 0x3000;
    public const int EventLogCode = 0x3020;
    public const int EventLogBuffer = 0x3100;


}