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
                $@"Unknown patch version: {fileVersion}, please report it on GitHub",
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

    public static class GameDataMan
    {
        public static nint Base;

        public const int Igt = 0x9C;
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
        public static nint FadeFallHeight;
        public static nint DeferredFallCheck;
        public static nint SetEvent;
        public static nint ApplySpEffectDamage;
        public static nint SakuraDance;
    }

    public static class Functions
    {
        public static nint HasSpEffectId;
        public static nint GetEvent;
    }

    public static class Patches
    {
        public static nint NoLogo;
        public static nint MenuTutorialSkip;
        public static nint ShowSmallHintBox;
        public static nint ShowTutorialText;
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
        
        GameDataMan.Base = moduleBase + Version switch
        {
            Version1_2_0 => 0x3B47CF0,
            Version1_3_0 or Version1_4_0 => 0x3B48D30,
            Version1_5_0 => 0x3D5AA20,
            Version1_6_0 => 0x3D5AAC0,
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
            Version1_2_0 => 0x9F109F,
            Version1_3_0 or Version1_4_0 => 0x9F172F,
            Version1_5_0 or Version1_6_0 => 0xA00FFF,
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
        
        Hooks.FadeFallHeight = moduleBase + Version switch
        {
            Version1_2_0 => 0xB81165,
            Version1_3_0 or Version1_4_0 => 0xB81815,
            Version1_5_0 or Version1_6_0 => 0xB97F65,
            _ => 0
        };

        Hooks.DeferredFallCheck = moduleBase + Version switch
        {
            Version1_2_0 => 0xB812B3,
            Version1_3_0 or Version1_4_0 => 0xB81963,
            Version1_5_0 or Version1_6_0 => 0xB980B3,
            _ => 0
        };
        
        Hooks.ApplySpEffectDamage = moduleBase + Version switch
        {
            Version1_2_0 => 0xB4EE2D,
            Version1_3_0 or Version1_4_0 => 0xB4F4DD,
            Version1_5_0 or Version1_6_0 => 0xB65ADD,
            _ => 0
        };
        
        Hooks.SakuraDance = moduleBase + Version switch
        {
            Version1_2_0 => 0xB56BA3,
            Version1_3_0 or Version1_4_0 => 0xB57253,
            Version1_5_0 or Version1_6_0 => 0xB6D853,
            _ => 0
        };

        
        Hooks.SetEvent = moduleBase + Version switch
        {
            Version1_2_0 => 0x6C1B90,
            Version1_3_0 or Version1_4_0 => 0x6C1BF0,
            Version1_5_0 or Version1_6_0 => 0x6C4520,
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
        
        Patches.NoLogo = moduleBase + Version switch
        {
            Version1_2_0 => 0xDEBF2B,
            Version1_3_0 or Version1_4_0 => 0xDEC85B,
            Version1_5_0 => 0xE1B1AB,
            Version1_6_0 => 0xE1B51B,
            _ => 0
        };
        
        Patches.MenuTutorialSkip = moduleBase + Version switch
        {
            Version1_2_0 => 0xD73E22,
            Version1_3_0 or Version1_4_0 => 0xD74752,
            Version1_5_0 => 0xD9A2D2,
            Version1_6_0 => 0xD9A642,
            _ => 0
        };

        Patches.ShowSmallHintBox = moduleBase + Version switch
        {
            Version1_2_0 => 0x8FE263,
            Version1_3_0 or Version1_4_0 => 0x8FE763,
            Version1_5_0 or Version1_6_0 => 0x909FA3,
            _ => 0
        };

        Patches.ShowTutorialText = moduleBase + Version switch
        {
            Version1_2_0 => 0x8FE213,
            Version1_3_0 or Version1_4_0 => 0x8FE713,
            Version1_5_0 or Version1_6_0 => 0x909F53,
            _ => 0
        };


#if DEBUG
        _baseAddr = moduleBase;
        Console.WriteLine("--- Globals ---");
        PrintOffset("WorldChrMan", WorldChrMan.Base);
        PrintOffset("GameDataMan", GameDataMan.Base);
        PrintOffset("EventFlagMan", EventFlagMan.Base);
        PrintOffset("FallDmgRetAddr", FallDmgRetAddr);


        Console.WriteLine("\n--- Hooks ---");
        PrintOffset("Hit", Hooks.Hit);
        PrintOffset("LethalFall", Hooks.LethalFall);
        PrintOffset("FadeFall", Hooks.FadeFall);
        PrintOffset("ApplyHealthDelta", Hooks.ApplyHealthDelta);
        PrintOffset("PostHit", Hooks.PostHit);
        PrintOffset("StaggerIgnoreCheck", Hooks.StaggerIgnoreCheck);
        PrintOffset("AuxProc", Hooks.AuxProc);
        PrintOffset("CheckAuxAttacker", Hooks.CheckAuxAttacker);
        PrintOffset("HkbFireEvent", Hooks.HkbFireEvent);
        PrintOffset("FadeFallHeight", Hooks.FadeFallHeight);
        PrintOffset("DeferredFallCheck", Hooks.DeferredFallCheck);
        PrintOffset("ApplySpEffectDamage", Hooks.ApplySpEffectDamage);
        PrintOffset("SakuraDance", Hooks.SakuraDance);
        PrintOffset("SetEvent", Hooks.SetEvent);


        Console.WriteLine("\n--- Functions ---");
        PrintOffset("HasSpEffectId", Functions.HasSpEffectId);
        PrintOffset("GetEvent", Functions.GetEvent);
        
        Console.WriteLine("\n--- Patches ---");
        PrintOffset("NoLogo", Patches.NoLogo);
        PrintOffset("MenuTutorialSkip", Patches.MenuTutorialSkip);
        PrintOffset("ShowSmallHintBox", Patches.ShowSmallHintBox);
        PrintOffset("ShowTutorialText", Patches.ShowTutorialText);


        Console.WriteLine("\n====================================\n");
#endif
    }

#if DEBUG
    private static nint _baseAddr;
    private static void PrintOffset(string name, nint value)
    {
        var rel = value - _baseAddr;
        Console.WriteLine(rel <= 0
            ? $"  {name,-40} *** NOT SET ***"
            : $"  {name,-40} 0x{(long)value:X}  (0x{(long)rel:X})");
    }
#endif
}