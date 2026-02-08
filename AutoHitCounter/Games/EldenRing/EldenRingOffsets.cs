// 

using AutoHitCounter.Utilities;
using static AutoHitCounter.Games.EldenRing.EldenRingVersion;

namespace AutoHitCounter.Games.EldenRing;

public static class EldenRingOffsets
{
    private static EldenRingVersion? _version;

    public static EldenRingVersion Version => _version
                                              ?? Version2_6_1;

    public static void Initialize(string fileVersion, nint moduleBase)
    {
        _version = fileVersion switch
        {
            var v when v.StartsWith("1.2.0.") => Version1_2_0,
            var v when v.StartsWith("1.2.1.") => Version1_2_1,
            var v when v.StartsWith("1.2.2.") => Version1_2_2,
            var v when v.StartsWith("1.2.3.") => Version1_2_3,
            var v when v.StartsWith("1.3.0.") => Version1_3_0,
            var v when v.StartsWith("1.3.1.") => Version1_3_1,
            var v when v.StartsWith("1.3.2.") => Version1_3_2,
            var v when v.StartsWith("1.4.0.") => Version1_4_0,
            var v when v.StartsWith("1.4.1.") => Version1_4_1,
            var v when v.StartsWith("1.5.0.") => Version1_5_0,
            var v when v.StartsWith("1.6.0.") => Version1_6_0,
            var v when v.StartsWith("1.7.0.") => Version1_7_0,
            var v when v.StartsWith("1.8.0.") => Version1_8_0,
            var v when v.StartsWith("1.8.1.") => Version1_8_1,
            var v when v.StartsWith("1.9.0.") => Version1_9_0,
            var v when v.StartsWith("1.9.1.") => Version1_9_1,
            var v when v.StartsWith("2.0.0.") => Version2_0_0,
            var v when v.StartsWith("2.0.1.") => Version2_0_1,
            var v when v.StartsWith("2.2.0.") => Version2_2_0,
            var v when v.StartsWith("2.2.3.") => Version2_2_3,
            var v when v.StartsWith("2.3.0.") => Version2_3_0,
            var v when v.StartsWith("2.4.0.") => Version2_4_0,
            var v when v.StartsWith("2.5.0.") => Version2_5_0,
            var v when v.StartsWith("2.6.0.") => Version2_6_0,
            var v when v.StartsWith("2.6.1.") => Version2_6_1,
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
        
        public static int PlayerIns => Version switch
        {
            Version1_2_0 or Version1_2_1 or Version1_2_2 or Version1_2_3 or Version1_3_0 or Version1_3_1
                or Version1_3_2
                or Version1_4_0 or Version1_4_1 or Version1_5_0 or Version1_6_0 => 0x18468,
            _ => 0x1E508,
        };
    }

    public static class Hooks
    {
        public static nint Hit;
        public static nint SetEvent;
    }

    public static class Functions
    {
        public static nint ChrInsByHandle;
    }

    private static void InitializeBaseAddresses(nint moduleBase)
    {
        WorldChrMan.Base = moduleBase + Version switch
        {
            Version1_2_0 => 0x3C50268,
            Version1_2_1 => 0x3C50288,
            Version1_2_2 => 0x3C502A8,
            Version1_2_3 => 0x3C532C8,
            Version1_3_0 or Version1_3_1 or Version1_3_2 => 0x3C64E38,
            Version1_4_0 or Version1_4_1 => 0x3C080E8,
            Version1_5_0 => 0x3C1FE98,
            Version1_6_0 => 0x3C310B8,
            Version1_7_0 => 0x3C4BA78,
            Version1_8_0 or Version1_8_1 => 0x3CD9998,
            Version1_9_0 or Version1_9_1 or Version2_0_0 or Version2_0_1 => 0x3CDCDD8,
            Version2_2_0 or Version2_4_0 or Version2_5_0
                or Version2_6_0 or Version2_6_1 => 0x3D65F88,
            Version2_2_3 or Version2_3_0 => 0x3D65FA8,
            _ => 0
        };

        Hooks.Hit = moduleBase + Version switch
        {
            Version1_2_0 => 0x440250,
            Version1_2_1 or Version1_2_2 => 0x4402C0,
            Version1_2_3 => 0x4403E0,
            Version1_3_0 or Version1_3_1 or Version1_3_2 => 0x441040,
            Version1_4_0 => 0x443860,
            Version1_4_1 => 0x443770,
            Version1_5_0 => 0x443BB0,
            Version1_6_0 => 0x444C10,
            Version1_7_0 => 0x444D60,
            Version1_8_0 or Version1_8_1 => 0x4466F0,
            Version1_9_0 or Version1_9_1 => 0x446830,
            Version2_0_0 or Version2_0_1 => 0x4469D0,
            Version2_2_0 or Version2_2_3 => 0x4497C0,
            Version2_3_0 => 0x4498D0,
            Version2_4_0 or Version2_5_0 => 0x449910,
            Version2_6_0 or Version2_6_1 => 0x4498E0,
            _ => 0
        };

        Hooks.SetEvent = moduleBase + Version switch
        {
            Version1_2_0 => 0x5D9E40,
            Version1_2_1 or Version1_2_2 => 0x5D9EB0,
            Version1_2_3 => 0x5D9FD0,
            Version1_3_0 or Version1_3_1 => 0x5DB060,
            Version1_3_2 => 0x5DB040,
            Version1_4_0 => 0x5DDD40,
            Version1_4_1 => 0x5DDC50,
            Version1_5_0 => 0x5DE730,
            Version1_6_0 => 0x5DFED0,
            Version1_7_0 => 0x5E0D50,
            Version1_8_0 or Version1_8_1 => 0x5ED450,
            Version1_9_0 => 0x5EE170,
            Version1_9_1 => 0x5EE1D0,
            Version2_0_0 or Version2_0_1 => 0x5EE410,
            Version2_2_0 or Version2_2_3 => 0x5F9970,
            Version2_3_0 => 0x5F9AF0,
            Version2_4_0 or Version2_5_0 => 0x5F9B50,
            Version2_6_0 or Version2_6_1 => 0x5F9CD0,
            _ => 0
        };

        Functions.ChrInsByHandle = moduleBase + Version switch
        {
            Version1_2_0 => 0x4F7580,
            Version1_2_1 or Version1_2_2 => 0x4F75F0,
            Version1_2_3 => 0x4F7710,
            Version1_3_0 or Version1_3_1 or Version1_3_2 => 0x4F8620,
            Version1_4_0 => 0x4FB430,
            Version1_4_1 => 0x4FB340,
            Version1_5_0 => 0x4FB6D0,
            Version1_6_0 => 0x4FC840,
            Version1_7_0 => 0x4FC7F0,
            Version1_8_0 or Version1_8_1 => 0x503B80,
            Version1_9_0 => 0x503EA0,
            Version1_9_1 => 0x503F00,
            Version2_0_0 or Version2_0_1 => 0x504140,
            Version2_2_0 or Version2_2_3 => 0x507BC0,
            Version2_3_0 => 0x507D40,
            Version2_4_0 or Version2_5_0 => 0x507D80,
            Version2_6_0 or Version2_6_1 => 0x507D50,
            _ => 0
        };
    }
}