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
        
        
        
        _baseAddr = moduleBase;
        
        #if DEBUG
        
            Console.WriteLine("--- Base Pointers ---");
            PrintOffset("WorldChrMan.Base", WorldChrMan.Base);
            PrintOffset("GameDataMan.Base", GameDataMan.Base);
            
            Console.WriteLine("\n--- Hooks ---");
            PrintOffset("Hit", Hooks.Hit);
            PrintOffset("FallDamage", Hooks.FallDamage);
            PrintOffset("KillBox", Hooks.KillBox);
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