// 

using System;
using AutoHitCounter.Utilities;
using static AutoHitCounter.Games.DS2S.DS2ScholarVersion;

namespace AutoHitCounter.Games.DS2S;

public static class DS2ScholarOffsets
{
    
    private static DS2ScholarVersion? _version;

    public static DS2ScholarVersion Version => _version
                                               ?? Version1_0_3;

    public static void Initialize(string fileVersion, nint moduleBase)
    {
        _version = fileVersion switch
        {
            var v when v.StartsWith("1.2.0.") => Version1_0_2,
            var v when v.StartsWith("1.2.1.") => Version1_0_3,
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
    
    public static class GameManagerImp
    {
        public static nint Base;

        public const int PlayerCtrl = 0xD0;
    }
    
    
    
    private static void InitializeBaseAddresses(nint moduleBase)
    {
        
        GameManagerImp.Base = moduleBase + Version switch
        {
            Version1_0_2 => 0x160B8D0,
            Version1_0_3 => 0x16148F0,
            _ => 0
        };
        
        
        
        _baseAddr = moduleBase;
        
#if DEBUG
        
        Console.WriteLine("--- Base Pointers ---");

            
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