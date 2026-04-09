// 

namespace AutoHitCounter.Games.DSR;

public class DSRCustomCodeOffsets
{
    public static nint Base;

    public const int CheckEnvDeathFlag = 0x0;
    public const int CheckAuxProcFlag = 0x1;
    public const int InThrowFlag = 0x2;
    
    public const int Hit = 0x10;
    public const int HitCode = 0x100;

    public const int ApplyHealthDelta = 0x400;
    public const int KillChr = 0x600;
    public const int CheckAuxAttacker = 0x700;
    public const int CheckAuxProc = 0x800;
    public const int ClearThrowState = 0xA00;
    public const int SetThrowState = 0xB00;
    
    public const int RunStartFlag = 0x2000;
    public const int RunStartCode = 0x2010;
    
    
    public const int EventLogWriteIdx = 0x3000;
    public const int EventLogCode = 0x3020;
    public const int EventLogBuffer = 0x3100;

}