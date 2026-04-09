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

    public static class UserInputManager
    {
        public static nint Base;

        public const int SteamInputEnum = 0x24B;
    }

    public static nint Float20;
    public static nint Float100;

    public static class Hooks
    {
        public static nint Hit;
        public static nint FallHeight;
        public static nint CheckAuxAttacker;
        public static nint AuxProc;
        public static nint HasJailerDrain;
        public static nint ApplyHealthDelta;
        public static nint KillBox;
        public static nint CheckStaggerIgnore;
        public static nint IsFallDmgDisabledHook;
        public static nint SetThrowState;
        public static nint ClearThrowState;
        public static nint SetEvent;
        public static nint StartNewGame;
    }

    public static class Functions
    {
        public static nint HasSpEffectId;
        public static nint OriginalLogoFunc;
        public static nint IsFallDamageDisabled;
    }

    public static class Patches
    {
        public static nint NoLogo;
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

        UserInputManager.Base = moduleBase + Version switch
        {
            Version1_3_2_0 => 0x48A9968,
            Version1_4_1_0 or Version1_4_2_0 or Version1_4_3_0 => 0x48AAC70,
            Version1_5_0_0 => 0x48AED80,
            Version1_5_1_0 => 0x48ADD80,
            Version1_6_0_0 => 0x48AEDF0,
            Version1_7_0_0 => 0x48B3670,
            Version1_8_0_0 => 0x49127F0,
            Version1_9_0_0 or Version1_10_0_0 => 0x4912930,
            Version1_11_0_0 => 0x4945EC8,
            Version1_12_0_0 => 0x4949058,
            Version1_13_0_0 => 0x494C878,
            Version1_14_0_0 or Version1_15_0_0 => 0x494E9D8,
            Version1_15_1_0 => 0x49644C8,
            Version1_15_2_0 => 0x49644B8,
            _ => 0
        };

        Float20 = moduleBase + Version switch
        {
            Version1_3_2_0 => 0x44BA1D0,
            Version1_4_1_0 or Version1_4_2_0 or Version1_4_3_0 => 0x44BB1D0,
            Version1_5_0_0 or Version1_6_0_0 => 0x44BF1E0,
            Version1_5_1_0 => 0x44BE1E0,
            Version1_7_0_0 => 0x44C31E0,
            Version1_8_0_0 or Version1_9_0_0 or Version1_10_0_0 => 0x451B920,
            Version1_11_0_0 => 0x454DEC0,
            Version1_12_0_0 => 0x4550ED0,
            Version1_13_0_0 => 0x4553ED0,
            Version1_14_0_0 or Version1_15_0_0 => 0x4555ED0,
            Version1_15_1_0 or Version1_15_2_0 => 0x456CED0,
            _ => 0
        };

        Float100 = moduleBase + Version switch
        {
            Version1_3_2_0 => 0x3CF0360,
            Version1_4_1_0 => 0x3CF1400,
            Version1_4_2_0 or Version1_4_3_0 => 0x3CF1680,
            Version1_5_0_0 => 0x3CF4960,
            Version1_5_1_0 => 0x3CF3BE0,
            Version1_6_0_0 => 0x3CF4B00,
            Version1_7_0_0 => 0x3CF7FD0,
            Version1_8_0_0 => 0x3D3C670,
            Version1_9_0_0 or Version1_10_0_0 => 0x3D3C330,
            Version1_11_0_0 => 0x3D65390,
            Version1_12_0_0 => 0x3D67910,
            Version1_13_0_0 => 0x3D6A010,
            Version1_14_0_0 => 0x3D6AF70,
            Version1_15_0_0 => 0x3D6AF90,
            Version1_15_1_0 => 0x3D7F010,
            Version1_15_2_0 => 0x3D7EFD0,
            _ => 0
        };


        Functions.OriginalLogoFunc = moduleBase + Version switch
        {
            Version1_3_2_0 => 0xB7B790,
            Version1_4_1_0 or Version1_4_2_0 or Version1_4_3_0 => 0xB7B860,
            Version1_5_0_0 => 0xB7C6B0,
            Version1_5_1_0 => 0xB7C4E0,
            Version1_6_0_0 => 0xB7CAB0,
            Version1_7_0_0 => 0xB7E140,
            Version1_8_0_0 => 0xB93180,
            Version1_9_0_0 => 0xB93740,
            Version1_10_0_0 => 0xB937B0,
            Version1_11_0_0 => 0xBA2D30,
            Version1_12_0_0 => 0xBA37B0,
            Version1_13_0_0 => 0xBA5360,
            Version1_14_0_0 => 0xBA5630,
            Version1_15_0_0 => 0xBA5730,
            Version1_15_1_0 => 0xBAFCE0,
            Version1_15_2_0 => 0xBAFE10,
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


        Hooks.FallHeight = moduleBase + Version switch
        {
            Version1_3_2_0 => 0x976E06,
            Version1_4_1_0 or Version1_4_2_0 or Version1_4_3_0 => 0x976D06,
            Version1_5_0_0 => 0x9777F6,
            Version1_5_1_0 => 0x977626,
            Version1_6_0_0 => 0x977BF6,
            Version1_7_0_0 => 0x978B06,
            Version1_8_0_0 => 0x985146,
            Version1_9_0_0 => 0x985706,
            Version1_10_0_0 => 0x985776,
            Version1_11_0_0 => 0x98D3B6,
            Version1_12_0_0 => 0x98DDD6,
            Version1_13_0_0 => 0x98F776,
            Version1_14_0_0 => 0x98FA46,
            Version1_15_0_0 => 0x98FB46,
            Version1_15_1_0 => 0x999C66,
            Version1_15_2_0 => 0x999D96,
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
        
        Hooks.IsFallDmgDisabledHook = moduleBase + Version switch
        {
            // WARNING: No match found for: Version1_3_2_0, Version1_4_1_0, Version1_4_2_0, Version1_4_3_0, Version1_5_0_0, Version1_5_1_0, Version1_6_0_0, Version1_7_0_0, Version1_8_0_0, Version1_9_0_0, Version1_10_0_0, Version1_11_0_0, Version1_12_0_0
            Version1_13_0_0 => 0x9BD858,
            Version1_14_0_0 => 0x9BDB28,
            Version1_15_0_0 => 0x9BDC28,
            Version1_15_1_0 => 0x9C7D18,
            Version1_15_2_0 => 0x9C7E48,
            _ => 0
        };
        
        Hooks.StartNewGame = moduleBase + Version switch
        {
            Version1_3_2_0 => 0xAACC5D,
            Version1_4_1_0 or Version1_4_2_0 or Version1_4_3_0 => 0xAACC9D,
            Version1_5_0_0 => 0xAAD90D,
            Version1_5_1_0 => 0xAAD73D,
            Version1_6_0_0 => 0xAADD0D,
            Version1_7_0_0 => 0xAAED5D,
            Version1_8_0_0 => 0xAC0B7D,
            Version1_9_0_0 => 0xAC113D,
            Version1_10_0_0 => 0xAC11AD,
            Version1_11_0_0 => 0xACEB9D,
            Version1_12_0_0 => 0xACF5ED,
            Version1_13_0_0 => 0xAD119D,
            Version1_14_0_0 => 0xAD146D,
            Version1_15_0_0 => 0xAD156D,
            Version1_15_1_0 => 0xADBD6D,
            Version1_15_2_0 => 0xADBE9D,
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
        
        Functions.IsFallDamageDisabled = moduleBase + Version switch
        {
            // WARNING: No match found for: Version1_3_2_0, Version1_4_1_0, Version1_4_2_0, Version1_4_3_0, Version1_5_0_0, Version1_5_1_0, Version1_6_0_0, Version1_7_0_0, Version1_8_0_0, Version1_9_0_0, Version1_10_0_0, Version1_11_0_0, Version1_12_0_0
            Version1_13_0_0 => 0x9CC420,
            Version1_14_0_0 => 0x9CC6F0,
            Version1_15_0_0 => 0x9CC7F0,
            Version1_15_1_0 => 0x9D68E0,
            Version1_15_2_0 => 0x9D6A10,
            _ => 0
        };
        
        Hooks.SetThrowState = moduleBase + Version switch
        {
            // WARNING: No match found for: Version1_3_2_0, Version1_4_1_0, Version1_4_2_0, Version1_4_3_0, Version1_5_0_0, Version1_5_1_0, Version1_6_0_0, Version1_7_0_0, Version1_8_0_0, Version1_9_0_0, Version1_10_0_0
            Version1_11_0_0 => 0x3F32D1,
            Version1_12_0_0 => 0x41E741,
            Version1_13_0_0 => 0x427B51,
            Version1_14_0_0 => 0x4127B1,
            Version1_15_0_0 => 0x4750C1,
            Version1_15_1_0 => 0x5182ECB,
            Version1_15_2_0 => 0x4EC31FA,
            _ => 0
        };
        
        Hooks.ClearThrowState = moduleBase + Version switch
        {
            Version1_3_2_0 => 0x9C77D5,
            Version1_4_1_0 or Version1_4_2_0 or Version1_4_3_0 => 0x9C76D5,
            Version1_5_0_0 => 0x9C8215,
            Version1_5_1_0 => 0x9C8045,
            Version1_6_0_0 => 0x9C8615,
            Version1_7_0_0 => 0x9C9525,
            Version1_8_0_0 => 0x9D5CD5,
            Version1_9_0_0 => 0x9D6295,
            Version1_10_0_0 => 0x9D6305,
            Version1_11_0_0 => 0x9DFF25,
            Version1_12_0_0 => 0x9E0975,
            Version1_13_0_0 => 0x9E2345,
            Version1_14_0_0 => 0x9E2615,
            Version1_15_0_0 => 0x9E2715,
            Version1_15_1_0 => 0x9EC825,
            Version1_15_2_0 => 0x9EC955,
            _ => 0
        };




        Patches.NoLogo = moduleBase + Version switch
        {
            Version1_3_2_0 => 0xBBAFDF,
            Version1_4_1_0 or Version1_4_2_0 or Version1_4_3_0 => 0xBBB0CF,
            Version1_5_0_0 => 0xBBBF2F,
            Version1_5_1_0 => 0xBBBD5F,
            Version1_6_0_0 => 0xBBC32F,
            Version1_7_0_0 => 0xBBEA5F,
            Version1_8_0_0 => 0xBD6ACF,
            Version1_9_0_0 => 0xBD708F,
            Version1_10_0_0 => 0xBD70FF,
            Version1_11_0_0 => 0xBE6F8F,
            Version1_12_0_0 => 0xBE7D9F,
            Version1_13_0_0 => 0xBE993F,
            Version1_14_0_0 => 0xBE9C0F,
            Version1_15_0_0 => 0xBE9D0F,
            Version1_15_1_0 => 0xBF42BF,
            Version1_15_2_0 => 0xBF43EF,
            _ => 0
        };


#if DEBUG
        _baseAddr = moduleBase;
        Console.WriteLine("--- Globals ---");
        PrintOffset("WorldChrMan.Base", WorldChrMan.Base);
        PrintOffset("GameDataMan", GameDataMan.Base);
        PrintOffset("UserInputManager", UserInputManager.Base);


        Console.WriteLine("\n--- Hooks ---");
        PrintOffset("Hit", Hooks.Hit);
        PrintOffset("FallHeight", Hooks.FallHeight);
        PrintOffset("CheckAuxAttacker", Hooks.CheckAuxAttacker);
        PrintOffset("AuxProc", Hooks.AuxProc);
        PrintOffset("HasJailerDrain", Hooks.HasJailerDrain);
        PrintOffset("ApplyHealthDelta", Hooks.ApplyHealthDelta);
        PrintOffset("KillBox", Hooks.KillBox);
        PrintOffset("CheckStaggerIgnore", Hooks.CheckStaggerIgnore);
        PrintOffset("IsFallDmgDisabledHook", Hooks.IsFallDmgDisabledHook);
        PrintOffset("SetThrowState", Hooks.SetThrowState);
        PrintOffset("ClearThrowState", Hooks.ClearThrowState);
        PrintOffset("SetEvent", Hooks.SetEvent);
        PrintOffset("StartNewGame", Hooks.StartNewGame);


        Console.WriteLine("\n--- Functions ---");
        PrintOffset("HasSpEffectId", Functions.HasSpEffectId);
        PrintOffset("OriginalLogoFunc", Functions.OriginalLogoFunc);
        PrintOffset("IsFallDamageDisabled", Functions.IsFallDamageDisabled);

        Console.WriteLine("\n--- Patches ---");
        PrintOffset("NoLogo", Patches.NoLogo);


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