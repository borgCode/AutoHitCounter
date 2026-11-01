// 

using System;
using System.Diagnostics;
using System.IO;
using AutoHitCounter.Interfaces;
using AutoHitCounter.Memory;
using static AutoHitCounter.Games.DS2.DS2Version;

namespace AutoHitCounter.Games.DS2;

public static class DS2Offsets
{
    private static DS2Version? _version;

    private static readonly string ScholarFallbackAddressPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "AutoHitCounter",
        "fallback_addresses_ds2_scholar.txt");

    private static readonly string VanillaFallbackAddressPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "AutoHitCounter",
        "fallback_addresses_ds2_vanilla.txt");

    public static DS2Version Version => _version
                                        ?? Scholar1_0_3;

    public static bool IsScholar { get; private set; }

    public static bool IsAobFallback { get; private set; }

    public static Version? DetectedAppVersion { get; private set; }

    public static void Initialize(FileVersionInfo versionInfo, IMemoryService memoryService)
    {
        IsScholar = !memoryService.IsWow64;
        IsAobFallback = false;
        DetectedAppVersion = null;
        var moduleBase = memoryService.BaseAddress;
        _version = (IsScholar, versionInfo.FileVersion) switch
        {
            (false, "1,0,11,0") => Vanilla1_0_11,
            (false, "1,0,12,0") => Vanilla1_0_12,
            (true, "1,0,2,0") => Scholar1_0_2,
            (true, "1,0,3,0") => Scholar1_0_3,
            _ => null
        };

        if (!_version.HasValue)
        {
            IsAobFallback = true;
            DetectedAppVersion = new Version(versionInfo.FileMajorPart, versionInfo.FileMinorPart,
                versionInfo.FileBuildPart);
            InitializeFallbackAddresses(memoryService);
            return;
        }


        InitializeBaseAddresses(moduleBase);
    }

    public static class GameManagerImp
    {
        public static nint Base;

        public static int GameDataManager => IsScholar ? 0xA8 : 0x60;

        public static int SaveDataManager => IsScholar ? 0xD8 : 0x6C;

        public static class SaveDataManagerOffsets
        {
            public const int SaveSlotStart = 0x0;
            public const int CurrentSaveSlotIdx = 0x1368;
        }

        public static class SaveSlotOffsets
        {
            public const int PlayTime = 0x1CC;
        }

        public static int PlayerCtrl => IsScholar ? 0xD0 : 0x74;
    }

    public static nint MapId;

    public static class KatanaMainApp
    {
        public static IntPtr Base;

        private static readonly int[] VanillaDoubleClickPtrChain = { 0x3C, 0x0, 0x4, 0x39 };
        private static readonly int[] ScholarDoubleClickPtrChain = { 0x60, 0x0, 0x8, 0x6D };

        public static int[] DoubleClickPtrChain => IsScholar ? ScholarDoubleClickPtrChain : VanillaDoubleClickPtrChain;
    }

    public static class Hooks
    {
        public static nint Hit;
        public static nint GeneralApplyDamage;
        public static nint KillBox;
        public static nint CountAuxHit;
        public static nint ClearWetPoisonBit;
        public static nint StaggerCheck;
        public static nint SetEvent;
        public static nint IgtNewGame;
        public static nint IgtStop;
        public static nint IgtLoadGame;
        public static nint NoBabyJump;
        public static nint CreditSkip;
    }

    private static void InitializeFallbackAddresses(IMemoryService memoryService)
    {
        var scanner = new AobScanner(memoryService);
        if (IsScholar) DS2Patterns.QueueScholarPatterns(scanner);
        else DS2Patterns.QueueVanillaPatterns(scanner);
        scanner.Run(IsScholar ? ScholarFallbackAddressPath : VanillaFallbackAddressPath);
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

        KatanaMainApp.Base = moduleBase + Version switch
        {
            Vanilla1_0_11 => 0x11936C4,
            Vanilla1_0_12 => 0x119A6E4,
            Scholar1_0_2 => 0x166C1D8,
            Scholar1_0_3 => 0x16751F8,
            _ => 0
        };


        Hooks.Hit = moduleBase + Version switch
        {
            Vanilla1_0_11 => 0x1C5500,
            Vanilla1_0_12 => 0x1C6A70,
            Scholar1_0_2 => 0x133BB0,
            Scholar1_0_3 => 0x136220,
            _ => 0
        };

        Hooks.GeneralApplyDamage = moduleBase + Version switch
        {
            Vanilla1_0_11 => 0x1F3395,
            Vanilla1_0_12 => 0x1F5A95,
            Scholar1_0_2 => 0x167234,
            Scholar1_0_3 => 0x16A354,
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


        Hooks.ClearWetPoisonBit = moduleBase + Version switch
        {
            Vanilla1_0_11 => 0x28B93B,
            Vanilla1_0_12 => 0x28EC0B,
            Scholar1_0_2 => 0x21F540,
            Scholar1_0_3 => 0x223050,
            _ => 0
        };

        Hooks.StaggerCheck = moduleBase + Version switch
        {
            Vanilla1_0_11 => 0x1CA788,
            Vanilla1_0_12 => 0x1CBCF8,
            Scholar1_0_2 => 0x1327BD,
            Scholar1_0_3 => 0x134E2D,
            _ => 0
        };
        Hooks.SetEvent = moduleBase + Version switch
        {
            Vanilla1_0_11 => 0x47884B,
            Vanilla1_0_12 => 0x47FAEB,
            Scholar1_0_2 => 0x46DED0,
            Scholar1_0_3 => 0x4750C0,
            _ => 0
        };

        Hooks.IgtNewGame = moduleBase + Version switch
        {
            Vanilla1_0_11 => 0x191827,
            Vanilla1_0_12 => 0x191A37,
            Scholar1_0_2 => 0xFC35F,
            Scholar1_0_3 => 0xFC41F,
            _ => 0
        };

        Hooks.IgtStop = moduleBase + Version switch
        {
            Vanilla1_0_11 => 0x23D3F8,
            Vanilla1_0_12 => 0x240228,
            Scholar1_0_2 => 0x1BC157,
            Scholar1_0_3 => 0x1BF8C7,
            _ => 0
        };

        Hooks.IgtLoadGame = moduleBase + Version switch
        {
            Vanilla1_0_11 => 0x193335,
            Vanilla1_0_12 => 0x193545,
            Scholar1_0_2 => 0xFCDBD,
            Scholar1_0_3 => 0xFCE7D,
            _ => 0
        };

        Hooks.NoBabyJump = moduleBase + Version switch
        {
            Vanilla1_0_11 => 0x3A09C9,
            Vanilla1_0_12 => 0x3A7369,
            Scholar1_0_2 => 0x37B4C7,
            Scholar1_0_3 => 0x381E27,
            _ => 0
        };

        Hooks.CreditSkip = moduleBase + Version switch
        {
            Vanilla1_0_11 => 0x11BD53,
            Vanilla1_0_12 => 0x11BE23,
            Scholar1_0_2 => 0x599D4,
            Scholar1_0_3 => 0x59A64,
            _ => 0
        };
    }

    private static nint _printBaseAddr;

    public static void Print(nint moduleBase)
    {
        _printBaseAddr = moduleBase;
        Console.WriteLine("--- Base Pointers ---");
        PrintOffset("GameManagerImp.Base", GameManagerImp.Base);
        PrintOffset("MapId", MapId);
        PrintOffset("KatanaMainApp", KatanaMainApp.Base);

        Console.WriteLine("\n--- Hooks ---");
        PrintOffset("Hit", Hooks.Hit);
        PrintOffset("GeneralApplyDamage", Hooks.GeneralApplyDamage);
        PrintOffset("KillBox", Hooks.KillBox);
        PrintOffset("CountAuxHit", Hooks.CountAuxHit);
        PrintOffset("ClearWetPoisonBit", Hooks.ClearWetPoisonBit);
        PrintOffset("StaggerCheck", Hooks.StaggerCheck);
        PrintOffset("SetEvent", Hooks.SetEvent);
        PrintOffset("IgtStart", Hooks.IgtNewGame);
        PrintOffset("IgtStop", Hooks.IgtStop);
        PrintOffset("IgtLoadGame", Hooks.IgtLoadGame);
        PrintOffset("NoBabyJump", Hooks.NoBabyJump);
        PrintOffset("CreditSkip", Hooks.CreditSkip);


        Console.WriteLine("\n====================================\n");
    }

    private static void PrintOffset(string name, nint value)
    {
        var rel = value - _printBaseAddr;
        Console.WriteLine(rel <= 0
            ? $"  {name,-40} *** NOT SET ***"
            : $"  {name,-40} 0x{(long)value:X}  (0x{(long)rel:X})");
    }
}