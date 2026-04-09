// 

namespace AutoHitCounter.Games.SK;

public class SKCustomCodeOffsets
{
    public static nint Base;

    public const int PendingHitFlag = 0x0;
    public const int StaggerCheckFlag = 0x1;
    public const int CheckAuxFlag = 0x2;
    public const int ShouldCountRobertoStagger = 0x3;
    public const int DeferredFallCheckFlag = 0x4;
    public const int PureLightningFlag = 0x5;

    public const int Hit = 0x10;
    public const int CheckPlayerDead = 0x20;

    public const int HitCode = 0x100;
    public const int PostHit = 0x400;
    public const int LethalFall = 0x500;
    public const int FadeFall = 0x580;
    public const int ApplyHealthDelta = 0x600;
    public const int StaggerCheck = 0x700;
    public const int CheckAux = 0x800;
    public const int CheckAuxAttacker = 0x850;
    public const int HkbFireEvent = 0x900;
    public const int FadeFallHeight = 0xB00;
    public const int DeferredFallCheck = 0xC00;
    public const int ApplySpEffectDamage = 0x1000;
    public const int SakuraDance = 0x1200;
    
    public const int RunStartFlag = 0x2000;
    public const int RunStartCode = 0x2010;
    
    public const int EventLogWriteIdx = 0x3000;
    public const int EventLogCode = 0x3020;
    public const int EventLogBuffer = 0x3100;
}