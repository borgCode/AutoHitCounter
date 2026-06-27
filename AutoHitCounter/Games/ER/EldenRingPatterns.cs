// 

using AutoHitCounter.Memory;
using AutoHitCounter.Memory.Patterns;

namespace AutoHitCounter.Games.ER;

public static class EldenRingPatterns
{
    public static void QueueFallbackPatterns(AobScanner scanner)
    {
        scanner.Queue(nameof(WorldChrMan), WorldChrMan, addr => EldenRingOffsets.WorldChrMan.Base = addr);
        scanner.Queue(nameof(GameDataMan), GameDataMan, addr => EldenRingOffsets.GameDataMan.Base = addr);
        scanner.Queue(nameof(UserInputManager), UserInputManager, addr => EldenRingOffsets.UserInputManager.Base = addr);
        scanner.Queue(nameof(CSTrophy), CSTrophy, addr => EldenRingOffsets.CSTrophy.Base = addr);
        scanner.Queue(nameof(VirtualMemFlag), VirtualMemFlag, addr => EldenRingOffsets.VirtualMemFlag.Base = addr);
        
        scanner.Queue(nameof(Hit), Hit, addr => EldenRingOffsets.Hooks.Hit = addr);
        scanner.Queue(nameof(FallDamage), FallDamage, addr => EldenRingOffsets.Hooks.FallDamage = addr);
        scanner.Queue(nameof(KillBox), KillBox, addr => EldenRingOffsets.Hooks.KillBox = addr);
        scanner.Queue(nameof(AuxDamageAttacker), AuxDamageAttacker, addr => EldenRingOffsets.Hooks.AuxDamageAttacker = addr);
        scanner.Queue(nameof(AuxProc), AuxProc, addr => EldenRingOffsets.Hooks.AuxProc = addr);
        scanner.Queue(nameof(SpEffectTickDamage), SpEffectTickDamage, addr => EldenRingOffsets.Hooks.SpEffectTickDamage = addr);
        scanner.Queue(nameof(EndureStagger), EndureStagger, addr => EldenRingOffsets.Hooks.EndureStagger = addr);
        scanner.Queue(nameof(EnvKilling), EnvKilling, addr => EldenRingOffsets.Hooks.EnvKilling = addr);
        scanner.Queue(nameof(CheckStateInfo), CheckStateInfo, addr => EldenRingOffsets.Hooks.CheckStateInfo = addr);
        scanner.Queue(nameof(CheckDeflectTear), CheckDeflectTear, addr => EldenRingOffsets.Hooks.CheckDeflectTear = addr);
        scanner.Queue(nameof(KillChr), KillChr, addr => EldenRingOffsets.Hooks.KillChr = addr);
        scanner.Queue(nameof(HandleThrow), HandleThrow, addr => EldenRingOffsets.Hooks.HandleThrow = addr);
        scanner.Queue(nameof(ClearThrowState), ClearThrowState, addr => EldenRingOffsets.Hooks.ClearThrowState = addr);
        scanner.Queue(nameof(SetEvent), SetEvent, addr => EldenRingOffsets.Hooks.SetEvent = addr);
        scanner.Queue(nameof(StartNewGame), StartNewGame, addr => EldenRingOffsets.Hooks.StartNewGame = addr);
        
        
        scanner.Queue(nameof(ChrInsByHandle), ChrInsByHandle, addr => EldenRingOffsets.Functions.ChrInsByHandle = addr);
        scanner.Queue(nameof(HasSpEffectId), HasSpEffectId, addr => EldenRingOffsets.Functions.HasSpEffectId = addr);
        scanner.Queue(nameof(GetEvent), GetEvent, addr => EldenRingOffsets.Functions.GetEvent = addr);
        scanner.Queue(nameof(HasStateInfo), HasStateInfo, addr => EldenRingOffsets.Functions.HasStateInfo = addr);
        scanner.Queue(nameof(IsNoDeathEnabled), IsNoDeathEnabled, addr => EldenRingOffsets.Functions.IsNoDeathEnabled = addr);
        scanner.Queue(nameof(IsTorrent), IsTorrent, addr => EldenRingOffsets.Functions.IsTorrent = addr);
        scanner.Queue(nameof(EnvKillingOriginal), EnvKillingOriginal, addr => EldenRingOffsets.Functions.EnvKillingOriginal = addr);
        
        
        scanner.Queue(nameof(NoLogo), NoLogo, addr => EldenRingOffsets.Patches.NoLogo = addr);
        
    }

    public static readonly Pattern WorldChrMan = new(
        [
            0x41, 0x8B, 0xF0, 0x48, 0x8B, 0xDA, 0x48, 0x8B, 0xF9, 0x4C, 0x8B, 0xB5, 0xE8, 0x00, 0x00, 0x00, 0x4C,
            0x8B, 0xBD, 0xF0, 0x00, 0x00, 0x00, 0x45, 0x33, 0xE4
        ],
        "xxxxxxxxxxxxxxxxxxxxxxxxxx",
        0x1A,
        AddressingMode.Relative,
        3,
        7
    );

    public static readonly Pattern GameDataMan = new(
        [0x0F, 0x84, 0x27, 0x01, 0x00, 0x00, 0xF6, 0x41],
        "xxxxxxxx",
        -0x1B,
        AddressingMode.Relative,
        3,
        7
    );

    public static readonly Pattern UserInputManager = new(
        [0x0F, 0xB6, 0x80, 0x8E, 0x08, 0x00, 0x00, 0xB9],
        "xxxxxxxx",
        -0x11,
        AddressingMode.Relative,
        3,
        7
    );

    public static readonly Pattern CSTrophy = new(
        [0x84, 0xC0, 0x74, 0x4A, 0x48, 0x8B, 0x0D, 0x00, 0x00, 0x00, 0x00, 0xC7, 0x45, 0x00, 0x0D, 0x00, 0x00, 0x00, 0x48, 0x85, 0xC9, 0x75, 0x2E
        ],
        "xxxxxxx????xx?xxxxxxxxx",
        0x4,
        AddressingMode.Relative,
        3,
        7
    );

    public static readonly Pattern VirtualMemFlag = new(
        [0x48, 0x8B, 0x3D, 0x00, 0x00, 0x00, 0x00, 0x48, 0x85, 0xFF, 0x74, 0x53],
        "xxx????xxxxx",
        0,
        AddressingMode.Relative,
        3,
        7
    );

    public static readonly Pattern Hit = new Pattern(
        [
            0x48, 0x89, 0x5C, 0x24, 0x08, 0x55, 0x56, 0x57, 0x41, 0x54, 0x41, 0x55, 0x41, 0x56, 0x41, 0x57, 0x48, 0x83,
            0xEC, 0x60, 0x33, 0xF6, 0x45, 0x0F, 0xB6, 0xE9
        ],
        "xxxxxxxxxxxxxxxxxxxxxxxxxx",
        0,
        AddressingMode.Absolute
    );

    public static readonly Pattern FallDamage = new Pattern(
        [
            0xC6, 0x44, 0x24, 0x30, 0x01, 0xF3, 0x0F, 0x11, 0x7C, 0x24, 0x28, 0xF3, 0x0F, 0x11, 0x7C, 0x24, 0x20
        ],
        "xxxxxxxxxxxxxxxxx",
        0,
        AddressingMode.Absolute
    );

    public static readonly Pattern KillBox = new Pattern(
        [0xC6, 0x44, 0x24, 0x28, 0x01, 0x45, 0x33, 0xC0, 0x33, 0xD2, 0xF3, 0x0F, 0x11, 0x44, 0x24, 0x20],
        "xxxxxxxxxxxxxxxx",
        0,
        AddressingMode.Absolute
    );
    
    public static readonly Pattern AuxDamageAttacker = new Pattern(
        [
            0x48, 0x8B, 0x8B, 0x90, 0x01, 0x00, 0x00, 0x48, 0x8D, 0x54, 0x24, 0x40, 0x4C, 0x8B, 0x84, 0x24, 0x00, 0x00,
            0x00, 0x00
        ],
        "xxxxxxxxxxxxxxxx?xxx",
        0,
        AddressingMode.Absolute
    );
    
    public static readonly Pattern AuxProc = new Pattern(
        new byte[] { 0xD3, 0xE0, 0x09, 0x83, 0xB8, 0x00, 0x00, 0x00 },
        "xxxxxxxx",
        2,
        AddressingMode.Absolute
    );

    public static readonly Pattern SpEffectTickDamage = new Pattern(
        new byte[] { 0x41, 0xB0, 0x01, 0x49, 0x8B, 0xCE, 0xF3, 0x0F, 0x11, 0x44, 0x24, 0x20 },
        "xxxxxxxxxxxx",
        6,
        AddressingMode.Absolute
    );

    public static readonly Pattern EndureStagger = new Pattern(
  
        new byte[] { 0x45, 0x0F, 0x57, 0xC9, 0x84, 0xC0, 0x75, 0x17, 0x38, 0x83, 0x58, 0x02, 0x00, 0x00 },
        "xxxxxxxxxxxxxx",
        0,
        AddressingMode.Absolute
    );
    
    public static readonly Pattern EnvKilling = new Pattern(
        new byte[] { 0xE8, 0x00, 0x00, 0x00, 0x00, 0x41, 0x8B, 0x96, 0x28, 0x02, 0x00, 0x00, 0x49, 0x8B, 0xCF, 0xE8, 0x00, 0x00, 0x00, 0x00, 0x49, 0x8B, 0x07, 0x49, 0x8B, 0xCF, 0xFF, 0x50, 0x40 },
        "x????xxxxxxxxxxx????xxxxxxxxx",
        0,
        AddressingMode.Absolute
    );
    
    
    public static readonly Pattern CheckStateInfo = new Pattern(
        new byte[] { 0x0F, 0xB6, 0x81, 0x59, 0x02, 0x00, 0x00, 0x83, 0xE0, 0x01, 0x84, 0xC0 },
        "xxxxxxxxxxxx",
        0,
        AddressingMode.Absolute
    );
    
    public static readonly Pattern CheckDeflectTear = new Pattern(
        new byte[] { 0xF3, 0x0F, 0x10, 0x6D, 0xA0, 0x0F, 0x2F, 0x00, 0xF3, 0x0F, 0x10, 0x65, 0xB0, 0xF3, 0x0F, 0x10, 0x5D, 0xAC, 0xF3, 0x0F, 0x10, 0x55, 0xA8, 0xF3, 0x0F, 0x10, 0x4D, 0xA4 },
        "xxxxxxx?xxxxxxxxxxxxxxxxxxxx",
        0,
        AddressingMode.Absolute
    );
    
    public static readonly Pattern KillChr = new Pattern(
        new byte[] { 0x48, 0x8B, 0xD9, 0x48, 0x8B, 0x89, 0x90, 0x01, 0x00, 0x00, 0x0F, 0x57, 0xDB },
        "xxxxxxxxxxxxx",
        0,
        AddressingMode.Absolute
    );
    
    public static readonly Pattern HandleThrow = new Pattern(
        new byte[] { 0xBF, 0x01, 0x00, 0x00, 0x00, 0x48, 0x8B, 0x88, 0x90, 0x01, 0x00, 0x00, 0x48, 0x8B, 0x01, 0x48, 0x8B, 0xCD, 0x8B, 0x80, 0x38, 0x01, 0x00, 0x00, 0x89, 0x44, 0x24, 0x38, 0x40, 0x38, 0xB4, 0x24, 0xB0, 0x00, 0x00, 0x00 },
        "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
        0,
        AddressingMode.Absolute
    );

    public static readonly Pattern ClearThrowState = new Pattern(
        new byte[] { 0x40, 0x53, 0x48, 0x83, 0xEC, 0x30, 0xB2, 0x01, 0x48, 0x8B, 0xD9 },
        "xxxxxxxxxxx",
        0,
        AddressingMode.Absolute
    );


    public static readonly Pattern SetEvent = new(
        [
            0x48, 0x89, 0x5C, 0x24, 0x08, 0x44, 0x8B, 0x49, 0x1C, 0x44, 0x8B, 0xD2, 0x33, 0xD2, 0x41, 0x8B, 0xC2,
            0x41, 0xF7, 0xF1, 0x41, 0x8B, 0xD8, 0x4C, 0x8B, 0xD9, 0x4C, 0x8B, 0x41, 0x38, 0x44, 0x0F, 0xAF, 0xC8,
            0x49, 0x8B, 0xC8, 0x49, 0x8B, 0x50, 0x08, 0x45, 0x2B, 0xD1, 0x80, 0x7A, 0x19, 0x00, 0x75, 0x17, 0x39,
            0x42, 0x20, 0x73, 0x06, 0x48, 0x8B, 0x52, 0x10, 0xEB, 0x06, 0x48, 0x8B, 0xCA, 0x48, 0x8B, 0x12, 0x80,
            0x7A, 0x19, 0x00, 0x74, 0xE9
        ],
        "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
        0,
        AddressingMode.Absolute
    );
    
    public static readonly Pattern StartNewGame = new Pattern(
        new byte[] { 0xC6, 0x80, 0x00, 0x00, 0x00, 0x00, 0x01, 0xE8, 0x00, 0x00, 0x00, 0x00, 0xBA, 0x05, 0x00, 0x00, 0x00, 0x89, 0x83, 0xBC, 0x00, 0x00, 0x00, 0x48, 0x8B, 0xCB, 0xE8, 0x00, 0x00, 0x00, 0x00, 0xC6, 0x83, 0xE0, 0x03, 0x00, 0x00, 0x01, 0x48, 0x83, 0xC4, 0x20, 0x5B, 0xC3 },
        "xx??xxxx????xxxxxxxxxxxxxxx????xxxxxxxxxxxxx",

        12,
        AddressingMode.Absolute);
    
    public static readonly Pattern ChrInsByHandle = new(
        [0xE8, 0x00, 0x00, 0x00, 0x00, 0x48, 0x8B, 0x5C, 0x24, 0x30, 0x48, 0x85, 0xC0, 0x74, 0x15],
        "x????xxxxxxxxxx",
        0,
        AddressingMode.Relative,
        1,
        5
    );

    public static readonly Pattern HasSpEffectId = new Pattern(
        new byte[]
        {
            0x48, 0x8B, 0x49, 0x08, 0x48, 0x85, 0xC9, 0x74, 0x15, 0x0F, 0x1F, 0x80, 0x00, 0x00, 0x00, 0x00, 0x39, 0x51,
            0x08, 0x74, 0x0C
        },
        "xxxxxxxxxxxxxxxxxxxxx",
        0,
        AddressingMode.Absolute
    );
    
    public static readonly Pattern GetEvent = new(
        [
            0xE8, 0x00, 0x00, 0x00, 0x00, 0x85, 0xC0, 0x40, 0x0F, 0x95, 0xC6, 0x40, 0x84, 0xED, 0x48, 0x8B, 0x6C,
            0x24, 0x30
        ],
        "x????xxxxxxxxxxxxxx",
        0,
        AddressingMode.Relative,
        1,
        5,
        anchorOffset: 9
    );

    public static readonly Pattern HasStateInfo = new Pattern(
        new byte[]
        {
            0x48, 0x8B, 0x8B, 0x78, 0x01, 0x00, 0x00, 0xBA, 0x8F, 0x00, 0x00, 0x00, 0xE8, 0x00, 0x00, 0x00, 0x00
        },
        "xxxxxxxxxxxxx????",
        12,
        AddressingMode.Relative,
        1,
        5
    );
    
    public static readonly Pattern IsNoDeathEnabled = new Pattern(
        new byte[]
        {
            0x48, 0x8B, 0x8B, 0x78, 0x01, 0x00, 0x00, 0xBA, 0x8F, 0x00, 0x00, 0x00, 0xE8, 0x00, 0x00, 0x00, 0x00
        },
        "xxxxxxxxxxxxx????",
        -0x59,
        AddressingMode.Relative,
        1,
        5
    );
    
    public static readonly Pattern IsTorrent = new Pattern(
        new byte[]
        {
            0xE8, 0x00, 0x00, 0x00, 0x00, 0x84, 0xC0, 0x75, 0x39, 0x83, 0xFD, 0x02, 0x76, 0x34, 0x8D, 0x47, 0xFF, 0x83,
            0xF8, 0x02, 0x77, 0x0E, 0x8D, 0x87, 0x3C, 0x86, 0x01, 0x00
        },
        "x????xxxxxxxxxxxxxxxxxxxxxxx",
        0,
        AddressingMode.Relative,
        1,
        5
    );
    
    
    public static readonly Pattern EnvKillingOriginal = new Pattern(
        new byte[] { 0xE8, 0x00, 0x00, 0x00, 0x00, 0x41, 0x8B, 0x96, 0x28, 0x02, 0x00, 0x00, 0x49, 0x8B, 0xCF, 0xE8, 0x00, 0x00, 0x00, 0x00, 0x49, 0x8B, 0x07, 0x49, 0x8B, 0xCF, 0xFF, 0x50, 0x40 },
        "x????xxxxxxxxxxx????xxxxxxxxx",
        0,
        AddressingMode.Relative,
        1,
        5
    );
    
    public static readonly Pattern NoLogo = new(
        [0x48, 0x85, 0xD2, 0x74, 0x07, 0xC6, 0x82],
        "xxxxxxx",
        0x18,
        AddressingMode.Absolute,
        anchorOffset: 4
    );
}