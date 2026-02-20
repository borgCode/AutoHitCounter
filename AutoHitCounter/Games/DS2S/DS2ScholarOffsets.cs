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

        public const int GameDataManager = 0xA8;
        public const int SaveDataManager = 0xD8;

        public static class SaveDataManagerOffsets
        {
            public const int SaveSlotStart = 0x0;
            public const int CurrentSaveSlotIdx = 0x1368;
        }

        public static class SaveSlotOffsets
        {
            public const int PlayTime = 0x1CC;
        }

    }


    public static class Hooks
    {
        public static nint Hit;
        public static nint FallDamage;
        public static nint KillBox;
        public static nint SetEvent;
        public static nint IgtNewGame;
        public static nint IgtStop;
        public static nint IgtLoadGame;
    }

    public static class Functions
    {
        public static nint RequestSave;
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
            Version1_0_2 => 0x46DED0,
            Version1_0_3 => 0x4750C0,
            _ => 0
        };
        
        Hooks.IgtNewGame = moduleBase + Version switch
        {
            Version1_0_2 => 0xFC35F,
            Version1_0_3 => 0xFC41F,
            _ => 0
        };
        
        Hooks.IgtStop = moduleBase + Version switch
        {
            Version1_0_2 => 0x1BC25F,
            Version1_0_3 => 0x1BF9CF,
            _ => 0
        };
        
        Hooks.IgtLoadGame = moduleBase + Version switch
        {
            Version1_0_2 => 0xFCDBD,
            Version1_0_3 => 0xFCE7D,
            _ => 0
        };
        
        Functions.RequestSave = moduleBase + Version switch
        {
            Version1_0_2 => 0x2E1080,
            Version1_0_3 => 0x2E7410,
            _ => 0
        };



        
        
        
#if DEBUG
        _baseAddr = moduleBase;
        Console.WriteLine("--- Base Pointers ---");
        PrintOffset("GameManagerImp.Base", GameManagerImp.Base);
        
        Console.WriteLine("\n--- Hooks ---");
        PrintOffset("Hit", Hooks.Hit);
        PrintOffset("FallDamage", Hooks.FallDamage);
        PrintOffset("KillBox", Hooks.KillBox);
        PrintOffset("SetEvent", Hooks.SetEvent);
        PrintOffset("IgtStart", Hooks.IgtNewGame);
        PrintOffset("IgtStop", Hooks.IgtStop);
        PrintOffset("IgtLoadGame", Hooks.IgtLoadGame);
           
            
        Console.WriteLine("\n--- Functions ---");
        PrintOffset("RequestSave", Functions.RequestSave);
  
            

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