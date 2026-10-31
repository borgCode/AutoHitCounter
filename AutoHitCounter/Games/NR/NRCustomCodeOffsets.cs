// 

namespace AutoHitCounter.Games.NR;

public class NRCustomCodeOffsets
{
    public static nint Base;

    public const int InThrowFlag = 0x0;
    public const int SpEffectDamageNegatedFlag = 0x1;
    
    public const int Hit = 0x10;

    public const int IsPlayerDead = 0x20;

    public const int ProcessHit = 0x100;
    public const int SetHealth = 0x400;
    public const int ApplySpEffectAndProcessStatus = 0x500;
    public const int ClearThrowState = 0x700;
    public const int CalcSpEffectMultipliers = 0x800;
    
    public const int EventLogWriteIdx = 0x3000;
    public const int EventLogCode = 0x3020;
    public const int EventLogBuffer = 0x3100; //0x1000
}