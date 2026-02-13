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

    public static void Initialize(long fileVersion, nint moduleBase)
    {
        _version = fileVersion switch
        {
            31605096 => Version1_0_2,
            28200992 => Version1_0_3,
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
    }


    public static class Hooks
    {
        public static nint Hit;
        public static nint FallDamage;
        public static nint KillBox;
        public static nint SetEvent;
    }
    
    
    
    private static void InitializeBaseAddresses(nint moduleBase)
    {
        
        GameManagerImp.Base = moduleBase + Version switch
        {
            Version1_0_2 => 0x160B8D0,
            Version1_0_3 => 0x16148F0,
            _ => 0
        };
        
        
        Hooks.Hit = moduleBase + Version switch
        {
            Version1_0_2 => 0x1327AB,
            Version1_0_3 => 0x134E1B,
            _ => 0
        };
        
        Hooks.FallDamage = moduleBase + Version switch
        {
            Version1_0_2 => 0x16727A,
            Version1_0_3 => 0x16A39A,
            _ => 0
        };
        
        Hooks.KillBox = moduleBase + Version switch
        {
            Version1_0_2 => 0x167440,
            Version1_0_3 => 0x16A560,
            _ => 0
        };


        Hooks.SetEvent = moduleBase + Version switch
        {
            Version1_0_2 => 0x46DEC0,
            Version1_0_3 => 0x4750B0,
            _ => 0
        };

        
        
        
        _baseAddr = moduleBase;
        
#if DEBUG
        
        Console.WriteLine("--- Base Pointers ---");
        PrintOffset("GameManagerImp.Base", GameManagerImp.Base);
        
        Console.WriteLine("\n--- Hooks ---");
        PrintOffset("Hit", Hooks.Hit);
        PrintOffset("FallDamage", Hooks.FallDamage);
        PrintOffset("KillBox", Hooks.KillBox);
        
        
        PrintOffset("SetEvent", Hooks.SetEvent);
           
            
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