// 

using System;
using AutoHitCounter.Utilities;
using static AutoHitCounter.Games.DS3.DS3Version;

namespace AutoHitCounter.Games.DS3;

public static class DS3Offsets
{
    private static DS3Version? _version;

    public static DS3Version Version => _version
                                        ?? Version1_15_0_0;

    public static void Initialize(string fileVersion, nint moduleBase)
    {
        _version = fileVersion switch
        {
            var v when v.StartsWith("1.3.2.") => Version1_3_2_0,
            var v when v.StartsWith("1.4.1.") => Version1_4_1_0,
            var v when v.StartsWith("1.4.2.") => Version1_4_2_0,
            var v when v.StartsWith("1.4.3.") => Version1_4_3_0,
            var v when v.StartsWith("1.5.0.") => Version1_5_0_0,
            var v when v.StartsWith("1.5.1.") => Version1_5_1_0,
            var v when v.StartsWith("1.6.0.") => Version1_6_0_0,
            var v when v.StartsWith("1.7.0.") => Version1_7_0_0,
            var v when v.StartsWith("1.8.0.") => Version1_8_0_0,
            var v when v.StartsWith("1.9.0.") => Version1_9_0_0,
            var v when v.StartsWith("1.10.0.") => Version1_10_0_0,
            var v when v.StartsWith("1.11.0.") => Version1_11_0_0,
            var v when v.StartsWith("1.12.0.") => Version1_12_0_0,
            var v when v.StartsWith("1.13.0.") => Version1_13_0_0,
            var v when v.StartsWith("1.14.0.") => Version1_14_0_0,
            var v when v.StartsWith("1.15.0.") => Version1_15_0_0,
            var v when v.StartsWith("1.15.1.") => Version1_15_1_0,
            var v when v.StartsWith("1.15.2.") => Version1_15_2_0,
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

        public const int PlayerIns = 0x80;
    }

    public static class GameDataMan
    {
        public static nint Base;

        public const int Igt = 0xA4;
    }

    public static nint FallDamageKillFloor;

    public static class Hooks
    {
        public static nint Hit;
        public static nint LethalFall;
        public static nint CheckAuxAttacker;
        public static nint AuxProc;
        public static nint HasJailerDrain;
        public static nint ApplyHealthDelta;
        public static nint KillBox;
        public static nint CheckStaggerIgnore;
        public static nint SetEvent;
    }

    public static class Functions
    {
        public static nint HasSpEffectId;
    }

    private static void InitializeBaseAddresses(nint moduleBase)
    {
        WorldChrMan.Base = moduleBase + Version switch
        {
            Version1_3_2_0 => 0x46C4AA8,
            Version1_4_1_0 or Version1_4_2_0 or Version1_4_3_0 => 0x46C5DC8,
            Version1_5_0_0 => 0x46C9EC8,
            Version1_5_1_0 => 0x46C8EC8,
            Version1_6_0_0 => 0x46C9F28,
            Version1_7_0_0 => 0x46CE768,
            Version1_8_0_0 => 0x472CF58,
            Version1_9_0_0 or Version1_10_0_0 => 0x472D098,
            Version1_11_0_0 => 0x4760398,
            Version1_12_0_0 => 0x4763518,
            Version1_13_0_0 => 0x4766D18,
            Version1_14_0_0 or Version1_15_0_0 => 0x4768E78,
            Version1_15_1_0 or Version1_15_2_0 => 0x477FDB8,
            _ => 0
        };

        GameDataMan.Base = moduleBase + Version switch
        {
            // WARNING: No match found for: Version1_3_2_0, Version1_4_1_0, Version1_4_2_0, Version1_4_3_0, Version1_5_0_0, Version1_5_1_0, Version1_6_0_0, Version1_7_0_0
            Version1_8_0_0 => 0x4704268,
            Version1_9_0_0 or Version1_10_0_0 => 0x47043A8,
            Version1_11_0_0 => 0x4737698,
            Version1_12_0_0 => 0x473A818,
            Version1_13_0_0 => 0x473E018,
            Version1_14_0_0 or Version1_15_0_0 => 0x4740178,
            Version1_15_1_0 or Version1_15_2_0 => 0x47572B8,
            _ => 0
        };


        FallDamageKillFloor = moduleBase + Version switch
        {
            Version1_3_2_0 => 0x3CF01A0,
            Version1_4_1_0 => 0x3CF1240,
            Version1_4_2_0 or Version1_4_3_0 => 0x3CF14C0,
            Version1_5_0_0 => 0x3CF47A0,
            Version1_5_1_0 => 0x3CF3A20,
            Version1_6_0_0 => 0x3CF4940,
            Version1_7_0_0 => 0x3CF7E10,
            Version1_8_0_0 => 0x3D3C4B0,
            Version1_9_0_0 or Version1_10_0_0 => 0x3D3C170,
            Version1_11_0_0 => 0x3D651D0,
            Version1_12_0_0 => 0x3D67750,
            Version1_13_0_0 => 0x3D69E50,
            Version1_14_0_0 => 0x3D6ADB0,
            Version1_15_0_0 => 0x3D6ADD0,
            Version1_15_1_0 => 0x3D7EE50,
            Version1_15_2_0 => 0x3D7EE10,
            _ => 0
        };


        Hooks.Hit = moduleBase + Version switch
        {
            Version1_3_2_0 => 0x99F498,
            Version1_4_1_0 or Version1_4_2_0 or Version1_4_3_0 => 0x99F398,
            Version1_5_0_0 => 0x99FE88,
            Version1_5_1_0 => 0x99FCB8,
            Version1_6_0_0 => 0x9A0288,
            Version1_7_0_0 => 0x9A1198,
            Version1_8_0_0 => 0x9AD8A8,
            Version1_9_0_0 => 0x9ADE68,
            Version1_10_0_0 => 0x9ADED8,
            Version1_11_0_0 => 0x4518542,
            Version1_12_0_0 => 0x1B8C72D,
            Version1_13_0_0 => 0xC883DD,
            Version1_14_0_0 => 0x1B949A1,
            Version1_15_0_0 => 0xC88A3D,
            Version1_15_1_0 => 0x51BCD98,
            Version1_15_2_0 => 0x9C2BF8,
            _ => 0
        };

        Hooks.LethalFall = moduleBase + Version switch
        {
            Version1_3_2_0 => 0x626746,
            Version1_4_1_0 or Version1_4_2_0 or Version1_4_3_0 => 0x626D16,
            Version1_5_0_0 => 0x627186,
            Version1_5_1_0 => 0x626FB6,
            Version1_6_0_0 => 0x627586,
            Version1_7_0_0 => 0x628496,
            Version1_8_0_0 or Version1_9_0_0 or Version1_10_0_0 => 0x62EA06,
            Version1_11_0_0 => 0x632056,
            Version1_12_0_0 => 0x6326B6,
            Version1_13_0_0 => 0x6327B6,
            Version1_14_0_0 or Version1_15_0_0 => 0x6327E6,
            Version1_15_1_0 => 0x634C06,
            Version1_15_2_0 => 0x634BF6,
            _ => 0
        };

        Hooks.CheckAuxAttacker = moduleBase + Version switch
        {
            Version1_3_2_0 => 0x96A570,
            Version1_4_1_0 or Version1_4_2_0 or Version1_4_3_0 => 0x96A470,
            Version1_5_0_0 => 0x96AF40,
            Version1_5_1_0 => 0x96AD70,
            Version1_6_0_0 => 0x96B340,
            Version1_7_0_0 => 0x96C250,
            Version1_8_0_0 => 0x978860,
            Version1_9_0_0 => 0x978E20,
            Version1_10_0_0 => 0x978E80,
            Version1_11_0_0 => 0x980A20,
            Version1_12_0_0 => 0x981440,
            Version1_13_0_0 => 0x982DE0,
            Version1_14_0_0 => 0x9830B0,
            Version1_15_0_0 => 0x983100,
            Version1_15_1_0 => 0x98D220,
            Version1_15_2_0 => 0x98D350,
            _ => 0
        };

        Hooks.AuxProc = moduleBase + Version switch
        {
            Version1_3_2_0 => 0x9C1E15,
            Version1_4_1_0 or Version1_4_2_0 or Version1_4_3_0 => 0x9C1D15,
            Version1_5_0_0 => 0x9C2805,
            Version1_5_1_0 => 0x9C2635,
            Version1_6_0_0 => 0x9C2C05,
            Version1_7_0_0 => 0x9C3B15,
            Version1_8_0_0 => 0x9D02C5,
            Version1_9_0_0 => 0x9D0885,
            Version1_10_0_0 => 0x9D08F5,
            Version1_11_0_0 => 0x9DA515,
            Version1_12_0_0 => 0x9DAF65,
            Version1_13_0_0 => 0x9DC905,
            Version1_14_0_0 => 0x9DCBD5,
            Version1_15_0_0 => 0x9DCCD5,
            Version1_15_1_0 => 0x9E6DC5,
            Version1_15_2_0 => 0x9E6EF5,
            _ => 0
        };

        Hooks.HasJailerDrain = moduleBase + Version switch
        {
            Version1_3_2_0 => 0x9DDE3B,
            Version1_4_1_0 or Version1_4_2_0 or Version1_4_3_0 => 0x9DDD3B,
            Version1_5_0_0 => 0x9DE99B,
            Version1_5_1_0 => 0x9DE7CB,
            Version1_6_0_0 => 0x9DED9B,
            Version1_7_0_0 => 0x9DFCAB,
            Version1_8_0_0 => 0x9EC5FB,
            Version1_9_0_0 => 0x9ECBBB,
            Version1_10_0_0 => 0x9ECC2B,
            Version1_11_0_0 => 0x9F695B,
            Version1_12_0_0 => 0x9F73AB,
            Version1_13_0_0 => 0x9F8D8B,
            Version1_14_0_0 => 0x9F905B,
            Version1_15_0_0 => 0x9F915B,
            Version1_15_1_0 => 0xA034BB,
            Version1_15_2_0 => 0xA035EB,
            _ => 0
        };

        Hooks.ApplyHealthDelta = moduleBase + Version switch
        {
            Version1_3_2_0 => 0x9BE7FA,
            Version1_4_1_0 => 0x1A6C759,
            Version1_4_2_0 => 0x19D5BEA,
            Version1_4_3_0 => 0x4484681,
            Version1_5_0_0 => 0x448882C,
            Version1_5_1_0 => 0x4487A39,
            Version1_6_0_0 => 0x4488B90,
            Version1_7_0_0 => 0x448D363,
            Version1_8_0_0 => 0x4BE57F9,
            Version1_9_0_0 => 0x1A5848D,
            Version1_10_0_0 => 0x1A5850B,
            Version1_11_0_0 => 0x1011EF0,
            Version1_12_0_0 => 0x9D8F21,
            Version1_13_0_0 => 0x9DA8AC,
            Version1_14_0_0 => 0xC79C18,
            Version1_15_0_0 => 0x9B1639,
            Version1_15_1_0 => 0x125AB60,
            Version1_15_2_0 => 0x1B9274C,
            _ => 0
        };

        Hooks.KillBox = moduleBase + Version switch
        {
            Version1_3_2_0 => 0x9A351A,
            Version1_4_1_0 or Version1_4_2_0 or Version1_4_3_0 => 0x9A341A,
            Version1_5_0_0 => 0x9A3F0A,
            Version1_5_1_0 => 0x9A3D3A,
            Version1_6_0_0 => 0x9A430A,
            Version1_7_0_0 => 0x9A521A,
            Version1_8_0_0 => 0x9B191A,
            Version1_9_0_0 => 0x9B1EDA,
            Version1_10_0_0 => 0x9B1F4A,
            Version1_11_0_0 => 0x9BB8AA,
            Version1_12_0_0 => 0x9BC2FA,
            Version1_13_0_0 => 0x9BDC9A,
            Version1_14_0_0 => 0x9BDF6A,
            Version1_15_0_0 => 0x9BE06A,
            Version1_15_1_0 => 0x9C815A,
            Version1_15_2_0 => 0x9C828A,
            _ => 0
        };

        Hooks.CheckStaggerIgnore = moduleBase + Version switch
        {
            Version1_3_2_0 => 0x99CB05,
            Version1_4_1_0 or Version1_4_2_0 or Version1_4_3_0 => 0x99CA05,
            Version1_5_0_0 => 0x99D4F5,
            Version1_5_1_0 => 0x99D325,
            Version1_6_0_0 => 0x99D8F5,
            Version1_7_0_0 => 0x99E805,
            Version1_8_0_0 => 0x9AAE72,
            Version1_9_0_0 => 0x9AB432,
            Version1_10_0_0 => 0x9AB4A2,
            Version1_11_0_0 => 0xF8530F,
            Version1_12_0_0 => 0xFC512F,
            Version1_13_0_0 => 0xF4D88F,
            Version1_14_0_0 => 0x1020D0F,
            Version1_15_0_0 => 0xF1604F,
            Version1_15_1_0 => 0x53D0365,
            Version1_15_2_0 => 0x4F28E0B,

            _ => 0
        };


        Hooks.SetEvent = moduleBase + Version switch
        {
            Version1_3_2_0 => 0x4BFB80,
            Version1_4_1_0 or Version1_4_2_0 or Version1_4_3_0 => 0x4BFC10,
            Version1_5_0_0 or Version1_5_1_0 => 0x4BFEF0,
            Version1_6_0_0 => 0x4C04C0,
            Version1_7_0_0 => 0x4C13D0,
            Version1_8_0_0 => 0x4C43D0,
            Version1_9_0_0 or Version1_10_0_0 => 0x4C43E0,
            Version1_11_0_0 => 0x4C4DE0,
            Version1_12_0_0 => 0x4C4F40,
            Version1_13_0_0 or Version1_14_0_0 or Version1_15_0_0 => 0x4C5060,
            Version1_15_1_0 => 0x4C5E30,
            Version1_15_2_0 => 0x4C5E20,
            _ => 0
        };

        Functions.HasSpEffectId = moduleBase + Version switch
        {
            Version1_3_2_0 => 0x86FAF0,
            Version1_4_1_0 or Version1_4_2_0 or Version1_4_3_0 => 0x86F840,
            Version1_5_0_0 => 0x86FEA0,
            Version1_5_1_0 => 0x86FCD0,
            Version1_6_0_0 => 0x8702A0,
            Version1_7_0_0 => 0x8711B0,
            Version1_8_0_0 or Version1_9_0_0 => 0x87ADA0,
            Version1_10_0_0 => 0x87AD90,
            Version1_11_0_0 => 0x880FE0,
            Version1_12_0_0 => 0x8817D0,
            Version1_13_0_0 => 0x883090,
            Version1_14_0_0 => 0x883180,
            Version1_15_0_0 => 0x8831C0,
            Version1_15_1_0 => 0x88B850,
            Version1_15_2_0 => 0x88BC00,
            _ => 0
        };


#if DEBUG
        _baseAddr = moduleBase;
        Console.WriteLine("--- Globals ---");
        PrintOffset("WorldChrMan.Base", WorldChrMan.Base);

        PrintOffset("FallDamageKillingFloor", FallDamageKillFloor);

        Console.WriteLine("\n--- Hooks ---");
        PrintOffset("Hooks.Hit", Hooks.Hit);
        PrintOffset("Hooks.LethalFall", Hooks.LethalFall);
        PrintOffset("Hooks.CheckAuxAttacker", Hooks.CheckAuxAttacker);
        PrintOffset("Hooks.AuxProc", Hooks.AuxProc);
        PrintOffset("Hooks.HasJailerDrain", Hooks.HasJailerDrain);
        PrintOffset("Hooks.ApplyHealthDelta", Hooks.ApplyHealthDelta);
        PrintOffset("Hooks.KillBox", Hooks.KillBox);
        PrintOffset("Hooks.SetEvent", Hooks.SetEvent);


        Console.WriteLine("\n--- Functions ---");
        PrintOffset("Functions.HasSpEffectId", Functions.HasSpEffectId);


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