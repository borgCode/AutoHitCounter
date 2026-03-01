// 

using System;
using AutoHitCounter.Utilities;
using static AutoHitCounter.Games.SK.SKVersion;

namespace AutoHitCounter.Games.SK;

public static class SKOffsets
{
    private static SKVersion? _version;

    public static SKVersion Version => _version
                                       ?? Version1_6_0;

    public static void Initialize(string fileVersion, nint moduleBase)
    {
        _version = fileVersion switch
        {
            var v when v.StartsWith("1.2.0.") => Version1_2_0,
            var v when v.StartsWith("1.3.0.") => Version1_3_0,
            var v when v.StartsWith("1.4.0.") => Version1_4_0,
            var v when v.StartsWith("1.5.0.") => Version1_5_0,
            var v when v.StartsWith("1.6.0.") => Version1_6_0,
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

        public const int PlayerIns = 0x88;
    }

    public static class EventFlagMan
    {
        public static IntPtr Base;
    }

    public static nint FallDmgRetAddr;

    public static class Hooks
    {
        public static nint Hit;
        public static nint LethalFall;
        public static nint FadeFall;
        public static nint ApplyHealthDelta;
        public static nint PostHit;
        public static nint StaggerIgnoreCheck;
        public static nint AuxProc;
        public static nint CheckAuxAttacker;
        public static nint HkbFireEvent;
    }

    public static class Functions
    {
        public static nint HasSpEffectId;
        public static nint GetEvent;
    }

    private static void InitializeBaseAddresses(nint moduleBase)
    {
        WorldChrMan.Base = moduleBase + Version switch
        {
            Version1_2_0 => 0x3B67DF0,
            Version1_3_0 or Version1_4_0 => 0x3B68E30,
            Version1_5_0 => 0x3D7A140,
            Version1_6_0 => 0x3D7A1E0,
            _ => 0
        };

        EventFlagMan.Base = moduleBase + Version switch
        {
            Version1_2_0 => 0x3B43248,
            Version1_3_0 or Version1_4_0 => 0x3B44288,
            Version1_5_0 => 0x3D55F48,
            Version1_6_0 => 0x3D55FE8,
            _ => 0
        };

        FallDmgRetAddr = moduleBase + Version switch
        {
            Version1_2_0 => 0xB811C7,
            Version1_3_0 or Version1_4_0 => 0xB81877,
            Version1_5_0 or Version1_6_0 => 0xB97FC7,
            _ => 0
        };


        Hooks.Hit = moduleBase + Version switch
        {
            Version1_2_0 => 0xB589F6,
            Version1_3_0 or Version1_4_0 => 0xB590A6,
            Version1_5_0 or Version1_6_0 => 0xB6F6A6,
            _ => 0
        };

        Hooks.LethalFall = moduleBase + Version switch
        {
            Version1_2_0 => 0xB809C4,
            Version1_3_0 or Version1_4_0 => 0xB81074,
            Version1_5_0 or Version1_6_0 => 0xB977C4,
            _ => 0
        };

        Hooks.FadeFall = moduleBase + Version switch
        {
            Version1_2_0 => 0xB80964,
            Version1_3_0 or Version1_4_0 => 0xB81014,
            Version1_5_0 or Version1_6_0 => 0xB97764,
            _ => 0
        };

        Hooks.ApplyHealthDelta = moduleBase + Version switch
        {
            Version1_2_0 => 0xBBDBE0,
            Version1_3_0 or Version1_4_0 => 0xBBE290,
            Version1_5_0 or Version1_6_0 => 0xBD4D40,
            _ => 0
        };

        Hooks.PostHit = moduleBase + Version switch
        {
            Version1_2_0 => 0xB58475,
            Version1_3_0 or Version1_4_0 => 0xB58B25,
            Version1_5_0 or Version1_6_0 => 0xB6F125,
            _ => 0
        };

        Hooks.StaggerIgnoreCheck = moduleBase + Version switch
        {
            Version1_2_0 => 0xB55456,
            Version1_3_0 or Version1_4_0 => 0xB55B06,
            Version1_5_0 or Version1_6_0 => 0xB6C106,
            _ => 0
        };

        Hooks.AuxProc = moduleBase + Version switch
        {
            Version1_2_0 => 0xBC2C2C,
            Version1_3_0 or Version1_4_0 => 0xBC32DC,
            Version1_5_0 or Version1_6_0 => 0xBD9D8C,
            _ => 0
        };


        Hooks.CheckAuxAttacker = moduleBase + Version switch
        {
            Version1_2_0 => 0x9F101D,
            Version1_3_0 or Version1_4_0 => 0x9F16AD,
            Version1_5_0 or Version1_6_0 => 0xA00F7D,
            _ => 0
        };
        
        Hooks.HkbFireEvent = moduleBase + Version switch
        {
            Version1_2_0 => 0x13AEC89,
            Version1_3_0 or Version1_4_0 => 0x13AF7B9,
            Version1_5_0 => 0x13F8FA9,
            Version1_6_0 => 0x13F9379,
            _ => 0
        };

        Functions.HasSpEffectId = moduleBase + Version switch
        {
            Version1_2_0 => 0xBE77E0,
            Version1_3_0 or Version1_4_0 => 0xBE7E90,
            Version1_5_0 or Version1_6_0 => 0xBFEFB0,
            _ => 0
        };

        Functions.GetEvent = moduleBase + Version switch
        {
            Version1_2_0 => 0x6C15A0,
            Version1_3_0 or Version1_4_0 => 0x6C1600,
            Version1_5_0 or Version1_6_0 => 0x6C3E60,
            _ => 0
        };


#if DEBUG
        _baseAddr = moduleBase;
        Console.WriteLine("--- Globals ---");
        PrintOffset("WorldChrMan.Base", WorldChrMan.Base);
        PrintOffset("EventFlagMan.Base", EventFlagMan.Base);
        PrintOffset("FallDmgRetAddr", FallDmgRetAddr);


        Console.WriteLine("\n--- Hooks ---");
        PrintOffset("Hooks.Hit", Hooks.Hit);
        PrintOffset("Hooks.LethalFall", Hooks.LethalFall);
        PrintOffset("Hooks.FadeFall", Hooks.FadeFall);
        PrintOffset("Hooks.ApplyHealthDelta", Hooks.ApplyHealthDelta);
        PrintOffset("Hooks.PostHit", Hooks.PostHit);
        PrintOffset("Hooks.StaggerIgnoreCheck", Hooks.StaggerIgnoreCheck);
        PrintOffset("Hooks.AuxProc", Hooks.AuxProc);
        PrintOffset("Hooks.CheckAuxAttacker", Hooks.CheckAuxAttacker);
        PrintOffset("Hooks.HkbFireEvent", Hooks.HkbFireEvent);


        Console.WriteLine("\n--- Functions ---");
        PrintOffset("Functions.HasSpEffectId", Functions.HasSpEffectId);
        PrintOffset("Functions.GetEvent", Functions.GetEvent);


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