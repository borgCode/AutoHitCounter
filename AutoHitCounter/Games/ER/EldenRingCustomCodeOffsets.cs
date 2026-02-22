// 

namespace AutoHitCounter.Games.ER;

public static class EldenRingCustomCodeOffsets
{
    public static nint Base;

    public const int CheckAuxProcFlag = 0x0;
    public const int StaggerCheckFlag = 0x1;
    public const int StateInfoCheckFlag = 0x2;
    public const int DeflectTearCheckFlag = 0x3;
    public const int Hit = 0x10;

    public const int HitCode = 0x20;
    public const int FallDamage = 0x400;
    public const int KillBox = 0x500;
    
    public const int CheckAuxAttacker = 0x600;
    public const int AuxProc = 0x800;
    public const int SpEffectTickDamage = 0xA00;
    public const int StaggerEndure = 0xD00;
    public const int EnvKilling = 0x1000;
    public const int StateInfoCheck = 0x1200;
    public const int DeflectTearCheck = 0x1400;

    public const int EventLogWriteIdx = 0x2000;
    public const int EventLogCode = 0x2020;
    public const int EventLogBuffer = 0x2100; //0x1000
}