// 

namespace AutoHitCounter.Games.ER;

public static class EldenRingCustomCodeOffsets
{
    public static nint Base;

    public const int CheckAuxProcFlag = 0x0;
    public const int StaggerCheckFlag = 0x1;
    public const int StateInfoCheckFlag = 0x2;
    public const int DeflectTearCheckFlag = 0x3;
    public const int InThrowFlag = 0x4;
    public const int HasRaptorFlag = 0x5;
    public const int HasCountedKillBoxFlag = 0x6;
    public const int Hit = 0x10;
    public const int CheckPlayerDead = 0x20;

    public const int HitCode = 0x100;
    public const int FallDamage = 0x400;
    public const int KillBox = 0x500;

    public const int CheckAuxAttacker = 0x600;
    public const int AuxProc = 0x800;
    public const int SpEffectTickDamage = 0xA00;
    public const int StaggerEndure = 0xD00;
    public const int EnvKilling = 0x1000;
    public const int StateInfoCheck = 0x1200;
    public const int DeflectTearCheck = 0x1400;

    public const int KillChr = 0x1600;
    public const int SetThrowState = 0x1800;
    public const int ClearThrowState = 0x1900;

    public const int RunStartFlag = 0x2000;
    public const int RunStartCode = 0x2010;
    
    public const int EventLogWriteIdx = 0x3000;
    public const int EventLogCode = 0x3020;
    public const int EventLogBuffer = 0x3100; //0x1000
}