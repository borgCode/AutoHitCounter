//

using System;

namespace AutoHitCounter.Games.BB;

public static class BBOffsets
{
    public static class WorldChrMan
    {
        public static nint Base;

        public const int PlayerIns = 0x60;
    }

    public static class GameDataMan
    {
        public static nint Base;

        public const int Igt = 0x94;
    }

    public static class Hooks
    {
        public static nint ProcessHit;
        public static nint ApplySpEffectAndProcessStatus;
        public static nint LandingHpCheck;
        public static nint SpEffectHpCheck;
        public static nint KillBox;
        public static nint SetEvent;
        public static nint EmevdSetEvent;
        public static nint StartNewGame;
    }

    public static class Functions
    {
        public static nint FindInventoryItem;
        public static nint ProcessHit;
    }

    public static class Patches
    {
        public static nint NoLogo;
    }

    public static void Initialize(nint ebootBase)
    {
        WorldChrMan.Base = ebootBase + 0x553E878;
        GameDataMan.Base = ebootBase + 0x553B130;

        Hooks.ProcessHit = ebootBase + 0x1A2A6AC;
        Hooks.ApplySpEffectAndProcessStatus = ebootBase + 0x18C69D0;
        Hooks.LandingHpCheck = ebootBase + 0x1A35DE7;
        Hooks.SpEffectHpCheck = ebootBase + 0x1A2FDA5;
        Hooks.KillBox = ebootBase + 0x18C2230;
        Hooks.SetEvent = ebootBase + 0x13CFCC0;
        Hooks.EmevdSetEvent = ebootBase + 0x12ED340;
        Hooks.StartNewGame = ebootBase + 0x195B462;

        Functions.FindInventoryItem = ebootBase + 0x14D9E80;
        Functions.ProcessHit = ebootBase + 0x1A2A770;

        Patches.NoLogo = ebootBase + 0x1BB5538;
    }

    private static nint _printBaseAddr;

    public static void Print(nint ebootBase)
    {
        _printBaseAddr = ebootBase;

        Console.WriteLine("--- Globals ---");
        PrintOffset("WorldChrMan.Base", WorldChrMan.Base);
        PrintOffset("GameDataMan.Base", GameDataMan.Base);

        Console.WriteLine("\n--- Hooks ---");
        PrintOffset("ProcessHit", Hooks.ProcessHit);
        PrintOffset("ApplySpEffectAndProcessStatus", Hooks.ApplySpEffectAndProcessStatus);
        PrintOffset("LandingHpCheck", Hooks.LandingHpCheck);
        PrintOffset("SpEffectHpCheck", Hooks.SpEffectHpCheck);
        PrintOffset("KillBox", Hooks.KillBox);
        PrintOffset("SetEvent", Hooks.SetEvent);
        PrintOffset("EmevdSetEvent", Hooks.EmevdSetEvent);
        PrintOffset("StartNewGame", Hooks.StartNewGame);

        Console.WriteLine("\n--- Functions ---");
        PrintOffset("FindInventoryItem", Functions.FindInventoryItem);
        PrintOffset("ProcessHit", Functions.ProcessHit);

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
