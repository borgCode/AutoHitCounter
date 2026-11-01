// 

using AutoHitCounter.Memory;
using AutoHitCounter.Memory.Patterns;

namespace AutoHitCounter.Games.DS2;

public static class DS2Patterns
{
    public static void QueueScholarPatterns(AobScanner scanner)
    {
        scanner.Queue(nameof(Scholar.GameManagerImp), Scholar.GameManagerImp, addr => DS2Offsets.GameManagerImp.Base = addr);
        scanner.Queue(nameof(Scholar.MapId), Scholar.MapId, addr => DS2Offsets.MapId = addr);
        scanner.Queue(nameof(Scholar.KatanaMainApp), Scholar.KatanaMainApp, addr => DS2Offsets.KatanaMainApp.Base = addr);

        scanner.Queue(nameof(Scholar.Hit), Scholar.Hit, addr => DS2Offsets.Hooks.Hit = addr);
        scanner.Queue(nameof(Scholar.GeneralApplyDamage), Scholar.GeneralApplyDamage, addr => DS2Offsets.Hooks.GeneralApplyDamage = addr);
        scanner.Queue(nameof(Scholar.KillBox), Scholar.KillBox, addr => DS2Offsets.Hooks.KillBox = addr);
        scanner.Queue(nameof(Scholar.CountAuxHit), Scholar.CountAuxHit, addr => DS2Offsets.Hooks.CountAuxHit = addr);
        scanner.Queue(nameof(Scholar.ClearWetPoisonBit), Scholar.ClearWetPoisonBit, addr => DS2Offsets.Hooks.ClearWetPoisonBit = addr);
        scanner.Queue(nameof(Scholar.StaggerCheck), Scholar.StaggerCheck, addr => DS2Offsets.Hooks.StaggerCheck = addr);
        scanner.Queue(nameof(Scholar.SetEvent), Scholar.SetEvent, addr => DS2Offsets.Hooks.SetEvent = addr);
        scanner.Queue(nameof(Scholar.IgtNewGame), Scholar.IgtNewGame, addr => DS2Offsets.Hooks.IgtNewGame = addr);
        scanner.Queue(nameof(Scholar.IgtStop), Scholar.IgtStop, addr => DS2Offsets.Hooks.IgtStop = addr);
        scanner.Queue(nameof(Scholar.IgtLoadGame), Scholar.IgtLoadGame, addr => DS2Offsets.Hooks.IgtLoadGame = addr);
        scanner.Queue(nameof(Scholar.NoBabyJump), Scholar.NoBabyJump, addr => DS2Offsets.Hooks.NoBabyJump = addr);
        scanner.Queue(nameof(Scholar.CreditSkip), Scholar.CreditSkip, addr => DS2Offsets.Hooks.CreditSkip = addr);
    }

    public static void QueueVanillaPatterns(AobScanner scanner)
    {
        scanner.Queue(nameof(Vanilla.GameManagerImp), Vanilla.GameManagerImp, addr => DS2Offsets.GameManagerImp.Base = addr);
        scanner.Queue(nameof(Vanilla.MapId), Vanilla.MapId, addr => DS2Offsets.MapId = addr);
        scanner.Queue(nameof(Vanilla.KatanaMainApp), Vanilla.KatanaMainApp, addr => DS2Offsets.KatanaMainApp.Base = addr);

        scanner.Queue(nameof(Vanilla.Hit), Vanilla.Hit, addr => DS2Offsets.Hooks.Hit = addr);
        scanner.Queue(nameof(Vanilla.GeneralApplyDamage), Vanilla.GeneralApplyDamage, addr => DS2Offsets.Hooks.GeneralApplyDamage = addr);
        scanner.Queue(nameof(Vanilla.KillBox), Vanilla.KillBox, addr => DS2Offsets.Hooks.KillBox = addr);
        scanner.Queue(nameof(Vanilla.CountAuxHit), Vanilla.CountAuxHit, addr => DS2Offsets.Hooks.CountAuxHit = addr);
        scanner.Queue(nameof(Vanilla.ClearWetPoisonBit), Vanilla.ClearWetPoisonBit, addr => DS2Offsets.Hooks.ClearWetPoisonBit = addr);
        scanner.Queue(nameof(Vanilla.StaggerCheck), Vanilla.StaggerCheck, addr => DS2Offsets.Hooks.StaggerCheck = addr);
        scanner.Queue(nameof(Vanilla.SetEvent), Vanilla.SetEvent, addr => DS2Offsets.Hooks.SetEvent = addr);
        scanner.Queue(nameof(Vanilla.IgtNewGame), Vanilla.IgtNewGame, addr => DS2Offsets.Hooks.IgtNewGame = addr);
        scanner.Queue(nameof(Vanilla.IgtStop), Vanilla.IgtStop, addr => DS2Offsets.Hooks.IgtStop = addr);
        scanner.Queue(nameof(Vanilla.IgtLoadGame), Vanilla.IgtLoadGame, addr => DS2Offsets.Hooks.IgtLoadGame = addr);
        scanner.Queue(nameof(Vanilla.NoBabyJump), Vanilla.NoBabyJump, addr => DS2Offsets.Hooks.NoBabyJump = addr);
        scanner.Queue(nameof(Vanilla.CreditSkip), Vanilla.CreditSkip, addr => DS2Offsets.Hooks.CreditSkip = addr);
    }

    public static class Scholar
    {
        public static readonly Pattern GameManagerImp = new(
            [0x89, 0x59, 0x0C, 0x88],
            "xxxx",
            0x5,
            AddressingMode.Relative,
            3,
            7
        );

        public static readonly Pattern MapId = new(
            [0x8B, 0x44, 0x24, 0x40, 0x89, 0x05],
            "xxxxxx",
            0x4,
            AddressingMode.Relative,
            2,
            6
        );

        public static readonly Pattern KatanaMainApp = new(
            [0x48, 0x8B, 0x0D, 0x00, 0x00, 0x00, 0x00, 0x48, 0x85, 0xC9, 0x74, 0x09, 0x48, 0x8B, 0x49, 0x60],
            "xxx????xxxxxxxxx",
            0,
            AddressingMode.Relative,
            3,
            7
        );

        public static readonly Pattern Hit = new(
            [
                0x48, 0x89, 0x5C, 0x24, 0x10, 0x48, 0x89, 0x6C, 0x24, 0x18, 0x48, 0x89, 0x74, 0x24, 0x20, 0x57, 0x41, 0x56,
                0x41, 0x57, 0x48, 0x83, 0xEC, 0x30, 0x48, 0x8B, 0x41, 0x08, 0x48, 0x8B, 0x19, 0x4C, 0x8B, 0xF9, 0x0F, 0x29,
                0x74, 0x24, 0x20
            ],
            "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
            0,
            AddressingMode.Absolute
        );

        public static readonly Pattern GeneralApplyDamage = new(
            [
                0x8B, 0x8B, 0x68, 0x01, 0x00, 0x00, 0x48, 0x89, 0x7C, 0x24, 0x40, 0x48, 0x8D, 0xBB, 0x6C, 0x01, 0x00, 0x00,
                0x8B, 0x17, 0x8D, 0x04, 0x32, 0x3B, 0xC1, 0x7D, 0x08, 0x2B, 0xCE
            ],
            "xxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
            0,
            AddressingMode.Absolute
        );

        public static readonly Pattern KillBox = new(
            [0x4C, 0x8B, 0x01, 0x89, 0x54, 0x24, 0x10, 0x49, 0x8D, 0x88, 0x70, 0x01, 0x00, 0x00],
            "xxxxxxxxxxxxxx",
            0,
            AddressingMode.Absolute
        );

        public static readonly Pattern CountAuxHit = new(
            [
                0x4C, 0x8B, 0x01, 0x48, 0x63, 0xC2, 0x48, 0x8D, 0x14, 0x40, 0xF3, 0x41, 0x0F, 0x10, 0x9C, 0x90, 0xC8, 0x01,
                0x00, 0x00
            ],
            "xxxxxxxxxxxxxxxxxxxx",
            0,
            AddressingMode.Absolute
        );

        public static readonly Pattern ClearWetPoisonBit = new(
            [0x20, 0x84, 0x2A, 0x20, 0x01, 0x00, 0x00, 0x48, 0x8D, 0x4C, 0x24, 0x40],
            "xxxxxxxxxxxx",
            0,
            AddressingMode.Absolute
        );

        public static readonly Pattern StaggerCheck = new(
            [
                0x41, 0x88, 0x4E, 0x17, 0x48, 0x8B, 0x47, 0x08, 0x41, 0x0F, 0xB6, 0x94, 0x24, 0xCF, 0x00, 0x00, 0x00, 0x48,
                0x8B, 0x48, 0x40
            ],
            "xxxxxxxxxxxxxxxxxxxxx",
            0,
            AddressingMode.Absolute
        );

        public static readonly Pattern SetEvent = new(
            [0xB8, 0x59, 0x17, 0xB7, 0xD1, 0x44, 0x8B, 0xD7, 0x48, 0x8B, 0xF1, 0xF7, 0xE2, 0x44, 0x8B, 0xCA],
            "xxxxxxxxxxxxxxxx",
            0,
            AddressingMode.Absolute
        );

        public static readonly Pattern IgtNewGame = new(
            [
                0x48, 0x89, 0x83, 0x60, 0x13, 0x00, 0x00, 0x48, 0x8B, 0x0D, 0x00, 0x00, 0x00, 0x00, 0x48, 0x8B, 0x49, 0x50,
                0xE8, 0x00, 0x00, 0x00, 0x00
            ],
            "xxxxxxxxxx????xxxxx????",
            0,
            AddressingMode.Absolute
        );

        public static readonly Pattern IgtStop = new(
            [0x24, 0xFD, 0xC7, 0x86, 0xAC, 0x24, 0x00, 0x00, 0x1F, 0x00, 0x00, 0x00, 0x88, 0x9E, 0xB0, 0x24, 0x00, 0x00],
            "xxxxxxxxxxxxxxxxxx",
            0,
            AddressingMode.Absolute
        );

        public static readonly Pattern IgtLoadGame = new(
            [0x48, 0x8B, 0x44, 0x24, 0x38, 0x48, 0x89, 0x87, 0x60, 0x13, 0x00, 0x00, 0x33, 0xC0],
            "xxxxxxxxxxxxxx",
            0x5,
            AddressingMode.Absolute
        );

        public static readonly Pattern NoBabyJump = new(
            [0x74, 0x12, 0xF3, 0x0F, 0x10, 0x83],
            "xxxxxx",
            -0x11,
            AddressingMode.Absolute
        );

        public static readonly Pattern CreditSkip = new(
            [0x48, 0x81, 0xEC, 0x20, 0x02, 0x00, 0x00, 0x8B, 0x41],
            "xxxxxxxxx",
            0,
            AddressingMode.Absolute
        );
    }

    public static class Vanilla
    {
        public static readonly Pattern GameManagerImp = new(
            [0xA1, 0x00, 0x00, 0x00, 0x00, 0x56, 0x8B, 0xB0, 0xCC, 0x0C, 0x00, 0x00],
            "x????xxxxxxx",
            0,
            AddressingMode.Direct32,
            1
        );

        public static readonly Pattern MapId = new(
            [0x8B, 0x15, 0x00, 0x00, 0x00, 0x00, 0x8B, 0xF2],
            "xx????xx",
            0,
            AddressingMode.Direct32,
            2
        );

        public static readonly Pattern KatanaMainApp = new(
            [0x8B, 0x0D, 0x00, 0x00, 0x00, 0x00, 0x8B, 0x49, 0x4C, 0x89, 0x55],
            "xx????xxxxx",
            0,
            AddressingMode.Direct32,
            2
        );

        public static readonly Pattern Hit = new(
            [
                0x55, 0x8B, 0xEC, 0x83, 0xEC, 0x08, 0x53, 0x56, 0x8B, 0x75, 0x0C, 0x57, 0x8B, 0xF9, 0x8B, 0x47, 0x10, 0x8B,
                0x88, 0x94, 0x00, 0x00, 0x00, 0x8B, 0x98, 0xB8, 0x01, 0x00, 0x00
            ],
            "xxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
            0,
            AddressingMode.Absolute
        );

        public static readonly Pattern GeneralApplyDamage = new(
            [
                0x8B, 0x86, 0xFC, 0x00, 0x00, 0x00, 0x53, 0x57, 0x8D, 0xBE, 0x00, 0x01, 0x00, 0x00, 0x8D, 0x1C, 0x11, 0x3B,
                0xD8
            ],
            "xxxxxxxxxxxxxxxxxxx",
            0,
            AddressingMode.Absolute
        );

        public static readonly Pattern KillBox = new(
            [
                0x8B, 0x01, 0x8B, 0x4D, 0x08, 0x89, 0x4D, 0x08, 0x8D, 0x90, 0x04, 0x01, 0x00, 0x00, 0x3B, 0x88, 0x00, 0x01,
                0x00, 0x00
            ],
            "xxxxxxxxxxxxxxxxxxxx",
            0,
            AddressingMode.Absolute
        );

        public static readonly Pattern CountAuxHit = new(
            [
                0xF3, 0x0F, 0x10, 0x55, 0x0C, 0x8B, 0x09, 0x0F, 0x5A, 0xE2, 0x8D, 0x04, 0x40, 0xF3, 0x0F, 0x10, 0x8C, 0x81,
                0x5C, 0x01, 0x00, 0x00, 0xF3, 0x0F, 0x10, 0x84, 0x81, 0x58, 0x01, 0x00, 0x00
            ],
            "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
            0,
            AddressingMode.Absolute
        );

        public static readonly Pattern ClearWetPoisonBit = new(
            [0x8D, 0x94, 0x1A, 0x10, 0x01, 0x00, 0x00, 0xF6, 0xD0, 0x20, 0x02],
            "xxxxxxxxxxx",
            0,
            AddressingMode.Absolute
        );

        public static readonly Pattern StaggerCheck = new(
            [0x88, 0x4F, 0x17, 0x8B, 0x56, 0x10, 0x8B, 0x42, 0x24, 0x8B, 0x8D, 0x18, 0xFE, 0xFF, 0xFF],
            "xxxxxxxxxxxxxxx",
            0,
            AddressingMode.Absolute
        );

        public static readonly Pattern SetEvent = new(
            [
                0xB8, 0x59, 0x17, 0xB7, 0xD1, 0xF7, 0xE6, 0x57, 0x8B, 0xF9, 0x8B, 0xCA, 0xC1, 0xE9, 0x0D, 0x8B, 0xC1, 0x69,
                0xC0, 0x10, 0x27, 0x00, 0x00, 0x2B, 0xF0, 0x8B, 0xC1, 0x69, 0xC0, 0x89, 0x00, 0x00, 0x00, 0x33, 0xD2, 0xBB,
                0x1F, 0x00, 0x00, 0x00, 0xF7, 0xF3, 0x89, 0x7D, 0xF8, 0x8B, 0x54, 0x97, 0x10, 0x85, 0xD2
            ],
            "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
            0,
            AddressingMode.Absolute
        );

        public static readonly Pattern IgtNewGame = new(
            [0x8B, 0x4D, 0xF4, 0x8B, 0x55, 0xF8, 0x89, 0x8B, 0x60, 0x13, 0x00, 0x00, 0x89, 0x93, 0x64, 0x13, 0x00, 0x00],
            "xxxxxxxxxxxxxxxxxx",
            0x6,
            AddressingMode.Absolute
        );

        public static readonly Pattern IgtStop = new(
            [0x24, 0xFD, 0x0C, 0x01, 0x88, 0x86, 0xF1, 0x0D, 0x00, 0x00],
            "xxxxxxxxxx",
            0,
            AddressingMode.Absolute
        );

        public static readonly Pattern IgtLoadGame = new(
            [
                0x89, 0x8F, 0x60, 0x13, 0x00, 0x00, 0x8B, 0x4D, 0xFC, 0x89, 0x97, 0x64, 0x13, 0x00, 0x00, 0x5F, 0x5E, 0x33,
                0xCD, 0x33, 0xC0
            ],
            "xxxxxxxxxxxxxxxxxxxxx",
            0,
            AddressingMode.Absolute
        );

        public static readonly Pattern NoBabyJump = new(
            [0x74, 0x30, 0xF3, 0x0F, 0x10, 0x80, 0x84],
            "xxxxxxx",
            -0x1C,
            AddressingMode.Absolute
        );

        public static readonly Pattern CreditSkip = new(
            [0x81, 0xEC, 0xFC, 0x01, 0x00, 0x00, 0x53, 0x8B, 0xD9, 0x8B, 0x43, 0x14],
            "xxxxxxxxxxxx",
            0,
            AddressingMode.Absolute
        );
    }
}
