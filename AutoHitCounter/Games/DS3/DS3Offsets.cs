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
                $@"Unknown patch version: {_version}, please report it on GitHub",
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

        
        
        _baseAddr = moduleBase;
        
#if DEBUG
        
        Console.WriteLine("--- Base Pointers ---");
        PrintOffset("WorldChrMan.Base", WorldChrMan.Base);
            
        Console.WriteLine("\n--- Hooks ---");

           
            
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