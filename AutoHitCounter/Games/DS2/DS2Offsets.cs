// 

using System;
using AutoHitCounter.Utilities;
using static AutoHitCounter.Games.DS2.DS2Version;

namespace AutoHitCounter.Games.DS2;

public static class DS2Offsets
{
    
    private static DS2Version? _version;

    public static DS2Version Version => _version
                                               ?? Scholar1_0_3;
    
    public static bool IsScholar => Version is Scholar1_0_2 or Scholar1_0_3;

    public static void Initialize(long fileSize, nint moduleBase)
    {
        _version = fileSize switch
        {
            32340760 => Vanilla1_0_11,
            29588960 => Vanilla1_0_12,
            31605096 => Scholar1_0_2,
            28200992 => Scholar1_0_3,
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
    
    public static class GameManagerImp
    {
        public static nint Base;

        
        public static int GameDataManager => Version switch
        {
            Vanilla1_0_11 or Vanilla1_0_12 => 0x60,
            Scholar1_0_2 or Scholar1_0_3 => 0xA8,
            _ => 0x0
        };
        

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

    public static nint MapId;


    public static class Hooks
    {
        public static nint Hit;
        public static nint GeneralApplyDamage;
        public static nint KillBox;
        public static nint CountAuxHit;
        public static nint LightPoiseStagger;
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
            Vanilla1_0_11 => 0x11493F4,
            Vanilla1_0_12 => 0x1150414,
            Scholar1_0_2 => 0x160B8D0,
            Scholar1_0_3 => 0x16148F0,
            _ => 0
        };
        
        MapId = moduleBase + Version switch
        {
            Vanilla1_0_11 => 0x10CD2D0,
            Vanilla1_0_12 => 0x10D42D8,
            Scholar1_0_2 => 0x15641B4,
            Scholar1_0_3 => 0x156D1C4,
            _ => 0
        };

        
        
        Hooks.Hit = moduleBase+ Version switch
        {
            Vanilla1_0_11 => 0x1C5500,
            Vanilla1_0_12 => 0x1C6A70,
            Scholar1_0_2 => 0x133BB0,
            Scholar1_0_3 => 0x136220,
            _ => 0
        };
        
        Hooks.GeneralApplyDamage = moduleBase + Version switch
        {
            Vanilla1_0_11 => 0x1F33D1,
            Vanilla1_0_12 => 0x1F5AD1,
            Scholar1_0_2 => 0x16727A,
            Scholar1_0_3 => 0x16A39A,
            _ => 0
        };
        
        Hooks.KillBox = moduleBase + Version switch
        {
            Vanilla1_0_11 => 0x1F3073,
            Vanilla1_0_12 => 0x1F5773,
            Scholar1_0_2 => 0x167440,
            Scholar1_0_3 => 0x16A560,
            _ => 0
        };
        
        Hooks.CountAuxHit = moduleBase + Version switch
        {
            Vanilla1_0_11 => 0x1D5D26,
            Vanilla1_0_12 => 0x1D72C6,
            Scholar1_0_2 => 0x143D20,
            Scholar1_0_3 => 0x146430,
            _ => 0
        };

        Hooks.LightPoiseStagger = moduleBase + Version switch
        {
            Vanilla1_0_11 => 0x1D55B4,
            Vanilla1_0_12 => 0x1D6B54,
            Scholar1_0_2 => 0x1432A7,
            Scholar1_0_3 => 0x145997,
            _ => 0
        };
        
        Hooks.SetEvent = moduleBase + Version switch
        {
            Scholar1_0_2 => 0x46DED0,
            Scholar1_0_3 => 0x4750C0,
            _ => 0
        };
        
        Hooks.IgtNewGame = moduleBase + Version switch
        {
            Scholar1_0_2 => 0xFC35F,
            Scholar1_0_3 => 0xFC41F,
            _ => 0
        };
        
        Hooks.IgtStop = moduleBase + Version switch
        {
            Scholar1_0_2 => 0x1BC25F,
            Scholar1_0_3 => 0x1BF9CF,
            _ => 0
        };
        
        Hooks.IgtLoadGame = moduleBase + Version switch
        {
            Scholar1_0_2 => 0xFCDBD,
            Scholar1_0_3 => 0xFCE7D,
            _ => 0
        };
        
        Functions.RequestSave = moduleBase + Version switch
        {
            Scholar1_0_2 => 0x2E1080,
            Scholar1_0_3 => 0x2E7410,
            _ => 0
        };



        
        
        
#if DEBUG
        _baseAddr = moduleBase;
        Console.WriteLine("--- Base Pointers ---");
        PrintOffset("GameManagerImp.Base", GameManagerImp.Base);
        PrintOffset("MapId", MapId);
        
        Console.WriteLine("\n--- Hooks ---");
        PrintOffset("Hit", Hooks.Hit);
        PrintOffset("GeneralApplyDamage", Hooks.GeneralApplyDamage);
        PrintOffset("KillBox", Hooks.KillBox);
        PrintOffset("CountAuxHit", Hooks.CountAuxHit);
        PrintOffset("LightPoiseStagger", Hooks.LightPoiseStagger);
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