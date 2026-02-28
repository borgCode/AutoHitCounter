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

    public static nint FallDmgRetAddr;

    public static class Hooks
    {
        public static nint DidSuccessfulDeflect;
        public static nint Hit;
        public static nint LethalFall;
        public static nint FadeFall;
        public static nint ApplyHealthDelta;
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
        
        FallDmgRetAddr = moduleBase + Version switch
        {
            Version1_2_0 => 0xB811C7,
            Version1_3_0 or Version1_4_0 => 0xB81877,
            Version1_5_0 or Version1_6_0 => 0xB97FC7,
            _ => 0
        };
        
        Hooks.DidSuccessfulDeflect = moduleBase + Version switch
        {
            Version1_2_0 => 0xB56619,
            Version1_3_0 or Version1_4_0 => 0xB56CC9,
            Version1_5_0 or Version1_6_0 => 0xB6D2C9,
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



#if DEBUG
        _baseAddr = moduleBase;
        Console.WriteLine("--- Globals ---");
        PrintOffset("WorldChrMan.Base", WorldChrMan.Base);
        PrintOffset("FallDmgRetAddr", FallDmgRetAddr);
        


        Console.WriteLine("\n--- Hooks ---");
        PrintOffset("Hooks.DidSuccessfulDeflect", Hooks.DidSuccessfulDeflect);
        PrintOffset("Hooks.Hit", Hooks.Hit);
        PrintOffset("Hooks.LethalFall", Hooks.LethalFall);
        PrintOffset("Hooks.FadeFall", Hooks.FadeFall);
        PrintOffset("Hooks.ApplyHealthDelta", Hooks.ApplyHealthDelta);


        Console.WriteLine("\n--- Functions ---");


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