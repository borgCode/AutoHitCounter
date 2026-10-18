// 

using System;
using System.IO;
using AutoHitCounter.Interfaces;
using AutoHitCounter.Memory;
using static AutoHitCounter.Games.NR.NRVersion;

namespace AutoHitCounter.Games.NR;

public class NROffsets
{
    private static NRVersion? _version;
    
    private static readonly string FallbackAddressPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "AutoHitCounter",
        "fallback_addresses_nightreign.txt");
    
    public static NRVersion Version => _version
                                       ?? Version1_3_3;
    
    public static bool IsAobFallback { get; private set; }

    public static string DetectedFileVersion { get; private set; }

    public static void Initialize(string fileVersion, IMemoryService memoryService)
    {
        var moduleBase = memoryService.BaseAddress;
        IsAobFallback = false;
        DetectedFileVersion = null;

        _version = fileVersion switch
        {
            null => null,
            var v when v.StartsWith("1.1.5") => Version1_1_5,
            var v when v.StartsWith("1.2.0") => Version1_2_0,
            var v when v.StartsWith("1.2.1") => Version1_2_1,
            var v when v.StartsWith("1.2.2") => Version1_2_2,
            var v when v.StartsWith("1.2.3") => Version1_2_3,
            var v when v.StartsWith("1.2.4") => Version1_2_4,
            var v when v.StartsWith("1.3.0") => Version1_3_0,
            var v when v.StartsWith("1.3.1") => Version1_3_1,
            var v when v.StartsWith("1.3.2") => Version1_3_2,
            var v when v.StartsWith("1.3.3") => Version1_3_3,
            _ => null
        };

        if (!_version.HasValue)
        {
            IsAobFallback = true;
            DetectedFileVersion = FormatFileVersion(fileVersion);
            InitializeFallbackAddresses(memoryService);
            return;
        }


        InitializeBaseAddresses(moduleBase);
    }
    
    public static class WorldChrMan
    {
        public static nint Base;

        public const int PlayerIns = 0x174E8;
        
        public static class ChrIns
        {
            public const int Modules = 0x1B8;


            public const int Health = 0x140;
        }
        
    }
    
    
    
    
    public static class Hooks
    {
        public static nint ProcessHit;
        public static nint SetHealth;
        public static nint ApplySpEffectAndProcessStatus;
        public static nint ClearThrowState;
        public static nint SetEvent;
        public static nint CalcSpEffectMultipliers;
    }
    
    public static class Functions
    {
        public static nint ProcessHitOriginal;
        public static nint ApplySpEffectAndProcessStatusOriginal;
        public static nint CalcSpEffectMultipliersOriginal;
        
    }
    
    public static class Patches
    {
        public static nint NoLogo;
        
    }
    
    
    private static string FormatFileVersion(string fileVersion)
    {
        if (string.IsNullOrWhiteSpace(fileVersion)) return null;

        return System.Version.TryParse(fileVersion.Trim(), out var parsed)
            ? $"{parsed.Major}.{parsed.Minor}.{Math.Max(parsed.Build, 0)}"
            : fileVersion.Trim();
    }

    private static void InitializeFallbackAddresses(IMemoryService memoryService)
    {
        var scanner = new AobScanner(memoryService);
        NRPatterns.QueueFallbackPatterns(scanner);
        scanner.Run(FallbackAddressPath);
    }

    private static void InitializeBaseAddresses(nint moduleBase)
    {
        WorldChrMan.Base = moduleBase + Version switch
        {
            Version1_1_5 => 0x3B04378,
            Version1_2_0 or Version1_2_1 => 0x3B7B3B8,
            Version1_2_2 => 0x3B8E3D8,
            Version1_2_3 or Version1_2_4 => 0x3B9A8D8,
            Version1_3_0 => 0x3C06D88,
            Version1_3_1 => 0x3C0BE28,
            Version1_3_2 or Version1_3_3 => 0x3C0F0A8,
            _ => 0
        };
        
        
        
        
        
        Hooks.ProcessHit = moduleBase + Version switch
        {
            Version1_3_2 or Version1_3_3 => 0x4366D6,
            _ => 0
        };
        
        Hooks.SetHealth = moduleBase + Version switch
        {
            Version1_1_5 => 0x412C30,
            Version1_2_0 => 0x416710,
            Version1_2_1 => 0x416730,
            Version1_2_2 => 0x4168A0,
            Version1_2_3 or Version1_2_4 => 0x4168E0,
            Version1_3_0 => 0x425A70,
            Version1_3_1 => 0x425D10,
            Version1_3_2 or Version1_3_3 => 0x426B00,
            _ => 0
        };

        Hooks.ApplySpEffectAndProcessStatus = moduleBase + Version switch
        {
            Version1_3_2 or Version1_3_3 => 0x3C8885,
            _ => 0
        };

        Hooks.ClearThrowState = moduleBase + Version switch
        {
            Version1_1_5 => 0x472780,
            Version1_2_0 => 0x4763F0,
            Version1_2_1 => 0x476580,
            Version1_2_2 => 0x476A00,
            Version1_2_3 or Version1_2_4 => 0x476AB0,
            Version1_3_0 => 0x48FF70,
            Version1_3_1 => 0x490BF0,
            Version1_3_2 or Version1_3_3 => 0x491BE0,
            _ => 0
        };
        
        Hooks.SetEvent = moduleBase + Version switch
        {
            Version1_1_5 => 0x5E43D0,
            Version1_2_0 => 0x5EBAC0,
            Version1_2_1 => 0x5EBC60,
            Version1_2_2 => 0x5EF480,
            Version1_2_3 or Version1_2_4 => 0x5EF560,
            Version1_3_0 => 0x60AB70,
            Version1_3_1 => 0x60BCD0,
            Version1_3_2 or Version1_3_3 => 0x60D330,
            _ => 0
        };
        
        Hooks.CalcSpEffectMultipliers = moduleBase + Version switch
        {
            // WARNING: No match found for: Version1_1_5
            Version1_2_0 => 0x4291B0,
            Version1_2_1 => 0x4291D0,
            Version1_2_2 => 0x429340,
            Version1_2_3 or Version1_2_4 => 0x4293C0,
            Version1_3_0 => 0x4393E3,
            Version1_3_1 => 0x439683,
            Version1_3_2 or Version1_3_3 => 0x43A653,
            _ => 0
        };
        
        
        Functions.ProcessHitOriginal = moduleBase + Version switch
        {
            Version1_3_0 => 0x43BFF0,
            Version1_3_1 => 0x43C290,
            Version1_3_2 or Version1_3_3 => 0x43D260,
            _ => 0
        };
        
        Functions.ApplySpEffectAndProcessStatusOriginal = moduleBase + Version switch
        {
            Version1_3_2 or Version1_3_3 => 0x3DB410,
            _ => 0
        };
        
        Functions.CalcSpEffectMultipliersOriginal = moduleBase + Version switch
        {
            // WARNING: No match found for: Version1_1_5, Version1_2_0, Version1_2_1, Version1_2_2, Version1_2_3, Version1_2_4, Version1_3_0, Version1_3_1
            Version1_3_2 or Version1_3_3 => 0x503800,
            _ => 0
        };






        Patches.NoLogo = moduleBase + Version switch
        {
            Version1_1_5 => 0xBBC4CB,
            Version1_2_0 => 0xBFDCDB,
            Version1_2_1 => 0xBFE4CB,
            Version1_2_2 => 0xC025AB,
            Version1_2_3 => 0xC03F5B,
            Version1_2_4 => 0xC03F4B,
            Version1_3_0 => 0xC4409B,
            Version1_3_1 => 0xC46A9B,
            Version1_3_2 or Version1_3_3 => 0xC497CB,
            _ => 0
        };


    }
    
    private static nint _printBaseAddr;

    public static void Print(nint moduleBase)
    {
        _printBaseAddr = moduleBase;
        
        Console.WriteLine("--- Globals ---");
        PrintOffset("WorldChrMan.Base", WorldChrMan.Base);
        
        Console.WriteLine("\n--- Hooks ---");
        PrintOffset("ProcessHit", Hooks.ProcessHit);
        PrintOffset("SetHealth", Hooks.SetHealth);
        PrintOffset("ApplySpEffectAndProcessStatus", Hooks.ApplySpEffectAndProcessStatus);
        PrintOffset("ClearThrowState", Hooks.ClearThrowState);
        PrintOffset("CalcSpEffectMultipliers", Hooks.CalcSpEffectMultipliers);
        
        Console.WriteLine("\n--- Functions ---");
        PrintOffset("ProcessHitOriginal", Functions.ProcessHitOriginal);
        PrintOffset("ApplySpEffectAndProcessStatusOriginal", Functions.ApplySpEffectAndProcessStatusOriginal);
        PrintOffset("CalcSpEffectMultipliersOriginal", Functions.CalcSpEffectMultipliersOriginal);
        
        Console.WriteLine("\n--- Patches ---");
        PrintOffset("NoLogo", Patches.NoLogo);
    }
    
    private static void PrintOffset(string name, nint value)
    {
        var rel = value - _printBaseAddr;
        Console.WriteLine(rel <= 0
            ? $"  {name,-40} *** NOT SET ***"
            : $"  {name,-40} 0x{(long)value:X}  (0x{(long)rel:X})");
    }
}