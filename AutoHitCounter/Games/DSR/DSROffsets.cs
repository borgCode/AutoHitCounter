// 

using System;
using AutoHitCounter.Utilities;
using static AutoHitCounter.Games.DSR.DSRVersion;

namespace AutoHitCounter.Games.DSR;

public static class DSROffsets
{
    private static DSRVersion? _version;

    public static DSRVersion Version => _version
                                        ?? Version1_0_3_0;

    public static void Initialize(long fileSize, nint moduleBase)
    {
        _version = fileSize switch
        {
            74186240 => Version1_0_1_0,
            75245056 => Version1_0_1_1,
            56756736 => Version1_0_1_2,
            57067008 => Version1_0_3_0,
            50286344 => Version1_0_3_1,
            _ => null
        };

        if (!_version.HasValue)
        {
            MsgBox.Show(
                $@"Unknown patch version (file size: {fileSize}), please report it on GitHub",
                "Unknown patch version");
            return;
        }


        InitializeBaseAddresses(moduleBase);
    }

    public static class WorldChrMan
    {
        public static nint Base;
    }

    private static void InitializeBaseAddresses(nint moduleBase)
    {
        WorldChrMan.Base = moduleBase + Version switch
        {
            Version1_0_1_0 => 0x1CEE830,
            Version1_0_1_1 => 0x1C7E820,
            Version1_0_1_2 => 0x1D01FC0,
            Version1_0_3_0 => 0x1D151B0,
            Version1_0_3_1 => 0x1C77E50,
            _ => 0
        };


#if DEBUG
        _baseAddr = moduleBase;
        Console.WriteLine("--- Globals ---");
        PrintOffset("WorldChrMan.Base", WorldChrMan.Base);

        // PrintOffset("FallDamageKillingFloor", FallDamageKillFloor);

        Console.WriteLine("\n--- Hooks ---");
        // PrintOffset("Hooks.Hit", Hooks.Hit);
        // PrintOffset("Hooks.LethalFall", Hooks.LethalFall);
        // PrintOffset("Hooks.CheckAuxAttacker", Hooks.CheckAuxAttacker);
        // PrintOffset("Hooks.AuxProc", Hooks.AuxProc);
        // PrintOffset("Hooks.HasJailerDrain", Hooks.HasJailerDrain);
        // PrintOffset("Hooks.ApplyHealthDelta", Hooks.ApplyHealthDelta);
        // PrintOffset("Hooks.KillBox", Hooks.KillBox);
        // PrintOffset("Hooks.SetEvent", Hooks.SetEvent);


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

