// 

using System;
using AutoHitCounter.Utilities;
using static AutoHitCounter.Games.ER.EldenRingVersion;

namespace AutoHitCounter.Games.ER;

public static class EldenRingOffsets
{
    private static EldenRingVersion? _version;

    public static EldenRingVersion Version => _version
                                              ?? Version2_6_1;

    public static void Initialize(string fileVersion, nint moduleBase)
    {
        _version = fileVersion switch
        {
            var v when v.StartsWith("1.2.0.") => Version1_2_0,
            var v when v.StartsWith("1.2.1.") => Version1_2_1,
            var v when v.StartsWith("1.2.2.") => Version1_2_2,
            var v when v.StartsWith("1.2.3.") => Version1_2_3,
            var v when v.StartsWith("1.3.0.") => Version1_3_0,
            var v when v.StartsWith("1.3.1.") => Version1_3_1,
            var v when v.StartsWith("1.3.2.") => Version1_3_2,
            var v when v.StartsWith("1.4.0.") => Version1_4_0,
            var v when v.StartsWith("1.4.1.") => Version1_4_1,
            var v when v.StartsWith("1.5.0.") => Version1_5_0,
            var v when v.StartsWith("1.6.0.") => Version1_6_0,
            var v when v.StartsWith("1.7.0.") => Version1_7_0,
            var v when v.StartsWith("1.8.0.") => Version1_8_0,
            var v when v.StartsWith("1.8.1.") => Version1_8_1,
            var v when v.StartsWith("1.9.0.") => Version1_9_0,
            var v when v.StartsWith("1.9.1.") => Version1_9_1,
            var v when v.StartsWith("2.0.0.") => Version2_0_0,
            var v when v.StartsWith("2.0.1.") => Version2_0_1,
            var v when v.StartsWith("2.2.0.") => Version2_2_0,
            var v when v.StartsWith("2.2.3.") => Version2_2_3,
            var v when v.StartsWith("2.3.0.") => Version2_3_0,
            var v when v.StartsWith("2.4.0.") => Version2_4_0,
            var v when v.StartsWith("2.5.0.") => Version2_5_0,
            var v when v.StartsWith("2.6.0.") => Version2_6_0,
            var v when v.StartsWith("2.6.1.") => Version2_6_1,
            _ => null
        };

        if (!_version.HasValue)
        {
            MsgBox.Show(
                $@"Unknown patch version: {_version}, please report it on GitHub",
                "Unknown patch version");
            return;
        }


        InitializeBaseAddresses(moduleBase);
    }

    public static class WorldChrMan
    {
        public static nint Base;
        
        public static int PlayerIns => Version switch
        {
            Version1_2_0 or Version1_2_1 or Version1_2_2 or Version1_2_3 or Version1_3_0 or Version1_3_1
                or Version1_3_2
                or Version1_4_0 or Version1_4_1 or Version1_5_0 or Version1_6_0 => 0x18468,
            _ => 0x1E508,
        };
    }

    public static class GameDataMan
    {
        public static nint Base;

        public const int Igt = 0xA0;
    }

    public static class Hooks
    {
        public static nint Hit;
        public static nint FallDamage;
        public static nint KillBox;
        public static nint AuxDamageAttacker;
        public static nint AuxProc;
        public static nint DeathFromSelfAux;
        public static nint EndureStagger;
        public static nint EnvKilling;
        public static nint CheckStateInfo;
        public static nint CheckDeflectTear;
        public static nint SetEvent;
    }

    public static class Functions
    {
        public static nint ChrInsByHandle;
        public static nint HasSpEffectId;
    }

    private static void InitializeBaseAddresses(nint moduleBase)
    {
        WorldChrMan.Base = moduleBase + Version switch
        {
            Version1_2_0 => 0x3C50268,
            Version1_2_1 => 0x3C50288,
            Version1_2_2 => 0x3C502A8,
            Version1_2_3 => 0x3C532C8,
            Version1_3_0 or Version1_3_1 or Version1_3_2 => 0x3C64E38,
            Version1_4_0 or Version1_4_1 => 0x3C080E8,
            Version1_5_0 => 0x3C1FE98,
            Version1_6_0 => 0x3C310B8,
            Version1_7_0 => 0x3C4BA78,
            Version1_8_0 or Version1_8_1 => 0x3CD9998,
            Version1_9_0 or Version1_9_1 or Version2_0_0 or Version2_0_1 => 0x3CDCDD8,
            Version2_2_0 or Version2_4_0 or Version2_5_0
                or Version2_6_0 or Version2_6_1 => 0x3D65F88,
            Version2_2_3 or Version2_3_0 => 0x3D65FA8,
            _ => 0
        };
        
        GameDataMan.Base = moduleBase + Version switch
        {
            Version1_2_0 => 0x3C481B8,
            Version1_2_1 => 0x3C481D8,
            Version1_2_2 => 0x3C481F8,
            Version1_2_3 => 0x3C4B218,
            Version1_3_0 or Version1_3_1 or Version1_3_2 => 0x3C5CD78,
            Version1_4_0 or Version1_4_1 => 0x3C00028,
            Version1_5_0 => 0x3C17EE8,
            Version1_6_0 => 0x3C29108,
            Version1_7_0 => 0x3C43AC8,
            Version1_8_0 or Version1_8_1 => 0x3CD1948,
            Version1_9_0 or Version1_9_1 or Version2_0_0 or Version2_0_1 => 0x3CD4D88,
            Version2_2_0 => 0x3D5DF38,
            Version2_2_3 or Version2_3_0 => 0x3D5DF58,
            Version2_4_0 or Version2_5_0 or Version2_6_0
                or Version2_6_1 => 0x3D5DF38,
            _ => 0
        };

        Hooks.Hit = moduleBase + Version switch
        {
            Version1_2_0 => 0x440250,
            Version1_2_1 or Version1_2_2 => 0x4402C0,
            Version1_2_3 => 0x4403E0,
            Version1_3_0 or Version1_3_1 or Version1_3_2 => 0x441040,
            Version1_4_0 => 0x443860,
            Version1_4_1 => 0x443770,
            Version1_5_0 => 0x443BB0,
            Version1_6_0 => 0x444C10,
            Version1_7_0 => 0x444D60,
            Version1_8_0 or Version1_8_1 => 0x4466F0,
            Version1_9_0 or Version1_9_1 => 0x446830,
            Version2_0_0 or Version2_0_1 => 0x4469D0,
            Version2_2_0 or Version2_2_3 => 0x4497C0,
            Version2_3_0 => 0x4498D0,
            Version2_4_0 or Version2_5_0 => 0x449910,
            Version2_6_0 or Version2_6_1 => 0x4498E0,
            _ => 0
        };
        
        Hooks.FallDamage = moduleBase + Version switch
        {
            Version1_2_0 => 0x444DB6,
            Version1_2_1 or Version1_2_2 => 0x444E26,
            Version1_2_3 => 0x444F46,
            Version1_3_0 or Version1_3_1 or Version1_3_2 => 0x445BA6,
            Version1_4_0 => 0x4483C6,
            Version1_4_1 => 0x4482D6,
            Version1_5_0 => 0x448656,
            Version1_6_0 => 0x4496B6,
            Version1_7_0 => 0x449806,
            Version1_8_0 or Version1_8_1 => 0x44B196,
            Version1_9_0 or Version1_9_1 => 0x44B2D6,
            Version2_0_0 or Version2_0_1 => 0x44B476,
            Version2_2_0 or Version2_2_3 => 0x44E266,
            Version2_3_0 => 0x44E376,
            Version2_4_0 or Version2_5_0 => 0x44E3B6,
            Version2_6_0 or Version2_6_1 => 0x44E386,
            _ => 0
        };
        
        Hooks.KillBox = moduleBase + Version switch
        {
            Version1_2_0 => 0x3F46FB,
            Version1_2_1 or Version1_2_2 => 0x3F476B,
            Version1_2_3 => 0x3F488B,
            Version1_3_0 or Version1_3_1 or Version1_3_2 => 0x3F535B,
            Version1_4_0 or Version1_4_1 => 0x3F783B,
            Version1_5_0 => 0x3F7C0B,
            Version1_6_0 => 0x3F89EB,
            Version1_7_0 => 0x3F8A6B,
            Version1_8_0 or Version1_8_1 => 0x44AA6D,
            Version1_9_0 or Version1_9_1 => 0x44ABAD,
            Version2_0_0 or Version2_0_1 => 0x44AD4D,
            Version2_2_0 or Version2_2_3 => 0x44DB3D,
            Version2_3_0 => 0x44DC4D,
            Version2_4_0 or Version2_5_0 => 0x44DC8D,
            Version2_6_0 or Version2_6_1 => 0x44DC5D,
            _ => 0
        };
        
        Hooks.AuxDamageAttacker = moduleBase + Version switch
        {
            // WARNING: No match found for: Version1_2_0, Version1_2_1, Version1_2_2, Version1_2_3, Version1_3_0, Version1_3_1, Version1_3_2, Version1_4_0, Version1_4_1, Version1_5_0, Version1_6_0, Version1_7_0
            Version1_8_0 or Version1_8_1 => 0x3F8602,
            Version1_9_0 or Version1_9_1 => 0x3F8732,
            Version2_0_0 or Version2_0_1 => 0x3F8802,
            Version2_2_0 or Version2_2_3 or Version2_6_0 or Version2_6_1 => 0x3FAF92,
            Version2_3_0 => 0x3FAFA2,
            Version2_4_0 or Version2_5_0 => 0x3FAFC2,
            _ => 0
        };
        
        Hooks.AuxProc = moduleBase + Version switch
        {
            Version1_2_0 => 0x434994,
            Version1_2_1 or Version1_2_2 => 0x434A04,
            Version1_2_3 => 0x434B24,
            Version1_3_0 or Version1_3_1 or Version1_3_2 => 0x435784,
            Version1_4_0 => 0x437E34,
            Version1_4_1 => 0x437E44,
            Version1_5_0 => 0x438284,
            Version1_6_0 => 0x4390C4,
            Version1_7_0 => 0x439144,
            Version1_8_0 or Version1_8_1 => 0x43AAA4,
            Version1_9_0 or Version1_9_1 => 0x43ABE4,
            Version2_0_0 or Version2_0_1 => 0x43AC84,
            Version2_2_0 or Version2_2_3 => 0x43D9E4,
            Version2_3_0 => 0x43DA04,
            Version2_4_0 or Version2_5_0 => 0x43DA44,
            Version2_6_0 or Version2_6_1 => 0x43DA14,
            _ => 0
        };

        Hooks.DeathFromSelfAux = moduleBase + Version switch
        {
            Version1_2_0 => 0x437F25,
            Version1_2_1 or Version1_2_2 => 0x437F95,
            Version1_2_3 => 0x4380B5,
            Version1_3_0 or Version1_3_1 or Version1_3_2 => 0x438D15,
            Version1_4_0 => 0x43B3C5,
            Version1_4_1 => 0x43B405,
            Version1_5_0 => 0x43B845,
            Version1_6_0 => 0x43C685,
            Version1_7_0 => 0x43C705,
            Version1_8_0 or Version1_8_1 => 0x43E065,
            Version1_9_0 or Version1_9_1 => 0x43E1A5,
            Version2_0_0 or Version2_0_1 => 0x43E248,
            Version2_2_0 or Version2_2_3 => 0x440FA8,
            Version2_3_0 => 0x4410B8,
            Version2_4_0 or Version2_5_0 => 0x4410F8,
            Version2_6_0 or Version2_6_1 => 0x4410C8,
            _ => 0
        };
        
        Hooks.EndureStagger = moduleBase + Version switch
        {
            Version1_2_0 => 0x43D743,
            Version1_2_1 or Version1_2_2 => 0x43D7B3,
            Version1_2_3 => 0x43D8D3,
            Version1_3_0 or Version1_3_1 or Version1_3_2 => 0x43E4E3,
            Version1_4_0 => 0x440D03,
            Version1_4_1 => 0x440C13,
            Version1_5_0 => 0x441053,
            Version1_6_0 => 0x441E93,
            Version1_7_0 => 0x441F13,
            Version1_8_0 or Version1_8_1 => 0x443873,
            Version1_9_0 or Version1_9_1 => 0x4439B3,
            Version2_0_0 or Version2_0_1 => 0x443A83,
            Version2_2_0 or Version2_2_3 => 0x446853,
            Version2_3_0 => 0x446963,
            Version2_4_0 or Version2_5_0 => 0x4469A3,
            Version2_6_0 or Version2_6_1 => 0x446973,
            _ => 0
        };

        Hooks.EnvKilling = moduleBase + Version switch
        {
            // WARNING: No match found for: Version1_2_0, Version1_2_1, Version1_2_2, Version1_2_3, Version1_3_0, Version1_3_1, Version1_3_2, Version1_4_0, Version1_4_1, Version1_5_0, Version1_6_0, Version1_7_0, Version1_8_0, Version1_8_1, Version1_9_0, Version1_9_1
            Version2_0_0 or Version2_0_1 => 0x44578E,
            Version2_2_0 or Version2_2_3 => 0x44851E,
            Version2_3_0 => 0x44862E,
            Version2_4_0 or Version2_5_0 => 0x44866E,
            Version2_6_0 or Version2_6_1 => 0x44863E,
            _ => 0
        };

        Hooks.CheckStateInfo = moduleBase + Version switch
        {
            Version1_2_0 => 0x43F7E3,
            Version1_2_1 or Version1_2_2 => 0x43F853,
            Version1_2_3 => 0x43F973,
            Version1_3_0 or Version1_3_1 or Version1_3_2 => 0x440583,
            Version1_4_0 => 0x442D9B,
            Version1_4_1 => 0x442CAB,
            Version1_5_0 => 0x4430EB,
            Version1_6_0 => 0x444153,
            Version1_7_0 => 0x4442A3,
            Version1_8_0 or Version1_8_1 => 0x445C33,
            Version1_9_0 or Version1_9_1 => 0x445D73,
            Version2_0_0 or Version2_0_1 => 0x445F0B,
            Version2_2_0 or Version2_2_3 => 0x448CFC,
            Version2_3_0 => 0x448E0C,
            Version2_4_0 or Version2_5_0 => 0x448E4C,
            Version2_6_0 or Version2_6_1 => 0x448E1C,
            _ => 0
        };
        
        Hooks.CheckDeflectTear = moduleBase + Version switch
        {
            // WARNING: No match found for: Version1_2_0, Version1_2_1, Version1_2_2, Version1_2_3, Version1_3_0, Version1_3_1, Version1_3_2, Version1_4_0, Version1_4_1, Version1_5_0, Version1_6_0
            Version1_7_0 => 0x4431DD,
            Version1_8_0 or Version1_8_1 => 0x444B4D,
            Version1_9_0 or Version1_9_1 => 0x444C8D,
            Version2_0_0 or Version2_0_1 => 0x444D5D,
            Version2_2_0 or Version2_2_3 => 0x447AEC,
            Version2_3_0 => 0x447BFC,
            Version2_4_0 or Version2_5_0 => 0x447C3C,
            Version2_6_0 or Version2_6_1 => 0x447C0C,
            _ => 0
        };


        
        Hooks.SetEvent = moduleBase + Version switch
        {
            Version1_2_0 => 0x5D9E40,
            Version1_2_1 or Version1_2_2 => 0x5D9EB0,
            Version1_2_3 => 0x5D9FD0,
            Version1_3_0 or Version1_3_1 => 0x5DB060,
            Version1_3_2 => 0x5DB040,
            Version1_4_0 => 0x5DDD40,
            Version1_4_1 => 0x5DDC50,
            Version1_5_0 => 0x5DE730,
            Version1_6_0 => 0x5DFED0,
            Version1_7_0 => 0x5E0D50,
            Version1_8_0 or Version1_8_1 => 0x5ED450,
            Version1_9_0 => 0x5EE170,
            Version1_9_1 => 0x5EE1D0,
            Version2_0_0 or Version2_0_1 => 0x5EE410,
            Version2_2_0 or Version2_2_3 => 0x5F9970,
            Version2_3_0 => 0x5F9AF0,
            Version2_4_0 or Version2_5_0 => 0x5F9B50,
            Version2_6_0 or Version2_6_1 => 0x5F9CD0,
            _ => 0
        };

        Functions.ChrInsByHandle = moduleBase + Version switch
        {
            Version1_2_0 => 0x4F7580,
            Version1_2_1 or Version1_2_2 => 0x4F75F0,
            Version1_2_3 => 0x4F7710,
            Version1_3_0 or Version1_3_1 or Version1_3_2 => 0x4F8620,
            Version1_4_0 => 0x4FB430,
            Version1_4_1 => 0x4FB340,
            Version1_5_0 => 0x4FB6D0,
            Version1_6_0 => 0x4FC840,
            Version1_7_0 => 0x4FC7F0,
            Version1_8_0 or Version1_8_1 => 0x503B80,
            Version1_9_0 => 0x503EA0,
            Version1_9_1 => 0x503F00,
            Version2_0_0 or Version2_0_1 => 0x504140,
            Version2_2_0 or Version2_2_3 => 0x507BC0,
            Version2_3_0 => 0x507D40,
            Version2_4_0 or Version2_5_0 => 0x507D80,
            Version2_6_0 or Version2_6_1 => 0x507D50,
            _ => 0
        };
        
        Functions.HasSpEffectId = moduleBase + Version switch
        {
            Version1_2_0 => 0x4E99C0,
            Version1_2_1 or Version1_2_2 => 0x4E9A30,
            Version1_2_3 => 0x4E9B50,
            Version1_3_0 or Version1_3_1 or Version1_3_2 => 0x4EAA20,
            Version1_4_0 => 0x4ED780,
            Version1_4_1 => 0x4ED690,
            Version1_5_0 => 0x4EDA20,
            Version1_6_0 => 0x4EEB90,
            Version1_7_0 => 0x4EEB40,
            Version1_8_0 or Version1_8_1 => 0x4F5E10,
            Version1_9_0 => 0x4F6070,
            Version1_9_1 => 0x4F60A0,
            Version2_0_0 or Version2_0_1 => 0x4F62E0,
            Version2_2_0 or Version2_2_3 => 0x4F9880,
            Version2_3_0 => 0x4F9A00,
            Version2_4_0 or Version2_5_0 => 0x4F9A40,
            Version2_6_0 or Version2_6_1 => 0x4F9A10,
            _ => 0
        };
        
        
        
        
        
        #if DEBUG
        _baseAddr = moduleBase;
            Console.WriteLine("--- Base Pointers ---");
            PrintOffset("WorldChrMan.Base", WorldChrMan.Base);
            PrintOffset("GameDataMan.Base", GameDataMan.Base);
            
            Console.WriteLine("\n--- Hooks ---");
            PrintOffset("Hit", Hooks.Hit);
            PrintOffset("FallDamage", Hooks.FallDamage);
            PrintOffset("KillBox", Hooks.KillBox);
            PrintOffset("AuxDamageAttacker", Hooks.AuxDamageAttacker);
            PrintOffset("AuxProc", Hooks.AuxProc);
            PrintOffset("DeathFromSelfAux", Hooks.DeathFromSelfAux);
            PrintOffset("EndureStagger", Hooks.EndureStagger);
            PrintOffset("EnvKilling", Hooks.EnvKilling);
            PrintOffset("CheckStateInfo", Hooks.CheckStateInfo);
            PrintOffset("CheckDeflectTear", Hooks.CheckDeflectTear);
            PrintOffset("SetEvent", Hooks.SetEvent);
           
            
            Console.WriteLine("\n--- Functions ---");
            PrintOffset("ChrInsByHandle", Functions.ChrInsByHandle);
            PrintOffset("HasSpEffectId", Functions.HasSpEffectId);
            

            Console.WriteLine("\n====================================\n");
#endif

    }
    
#if DEBUG
    private static nint _baseAddr;
    private static void PrintOffset(string name, nint value)
    {
        Console.WriteLine(value - _baseAddr <= 0 ? $"  {name,-40} *** NOT SET ***" : $"  {name,-40} 0x{(long)value:X}");
    }
#endif
}