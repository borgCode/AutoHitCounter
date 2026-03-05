// 

namespace AutoHitCounter.Enums;

public enum AsmScript
{
    DSRHit,
    DSRApplyHealthDelta,
    DSRKillChr,
    DSREventLog,
    
    VanillaCheckPlayerDead,
    VanillaHit,
    VanillaCountAuxHit,
    VanillaGeneralApplyDamage,
    VanillaKillBox,
    VanillaLightPoiseStagger,
    
    ScholarHit,
    ScholarGeneralApplyDamage,
    ScholarKillBox,
    ScholarCountAuxHit,
    ScholarLightPoiseStagger,
    ScholarEventLog,
    ScholarIgtNewGame,
    ScholarIgtStop,
    ScholarIgtLoadGame,
    ScholarCheckPlayerDead,
    
    DS3CheckPlayerDead,
    DS3Hit,
    DS3LethalFall,
    DS3CheckAuxAttacker,
    DS3AuxProc,
    DS3GetSpEffect,
    DS3JailerDrain,
    DS3ApplyHealthDelta,
    DS3KillBox,
    DS3CheckStaggerIgnore,
    DS3EventLog,
    
    
    SKCheckPlayerDead,
    SKHit,
    SKLethalFall,
    SKFadeFall,
    SKApplyHealthDelta,
    SKPostHit,
    SKStaggerIgnoreCheck,
    SKAuxProc,
    SKCheckAuxAttacker,
    SKHkbFireEvent,
    SKEventLog,

    EldenRingHit,
    EldenRingCheckPlayerDead,
    EldenRingFallDamage,
    EldenRingKillBox,
    EldenRingAuxDamageAttacker,
    EldenRingAuxProc,
    EldenRingSpEffectTickDamage,
    EldenRingStaggerEndure,
    EldenRingEnvKilling,
    EldenRingCheckStateInfo,
    EldenRingDeflectTear,
    EldenRingKillChr,
    EldenRingEventLog
}