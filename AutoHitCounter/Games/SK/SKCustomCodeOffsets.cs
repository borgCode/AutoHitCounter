// 

namespace AutoHitCounter.Games.SK;

public class SKCustomCodeOffsets
{
    public static nint Base;

    public const int PendingHitFlag = 0x0;
    public const int StaggerCheckFlag = 0x1;
    public const int CheckAuxFlag = 0x2;
    public const int ShouldCountRobertoStagger = 0x3;

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
    
    public const int EventLogWriteIdx = 0x2000;
    public const int EventLogCode = 0x2020;
    public const int EventLogBuffer = 0x2100;
}