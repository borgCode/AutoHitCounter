// 

using System;
using System.Collections.Generic;
using AutoHitCounter.Interfaces;
using AutoHitCounter.Memory;
using AutoHitCounter.Utilities;

namespace AutoHitCounter.Games.EldenRing;

public class EldenRingEventService(
    IMemoryService memoryService,
    HookManager hookManager,
    Dictionary<uint, string> eldenRingEvents)
    : IEventService
{
    private int _readIndex = 0;

    private readonly nint _writeIndexAddr = CodeCaveOffsets.Base + CodeCaveOffsets.EventLogWriteIdx;
    private readonly nint _bufferAddr = CodeCaveOffsets.Base + CodeCaveOffsets.EventLogBuffer;

    public void InstallHook()
    {
        var code = CodeCaveOffsets.Base + CodeCaveOffsets.EventLogCode;

        var bytes = AsmLoader.GetAsmBytes("EldenRingEventLog");
        var writeIndex = CodeCaveOffsets.Base + CodeCaveOffsets.EventLogWriteIdx;
        var buffer = CodeCaveOffsets.Base + CodeCaveOffsets.EventLogBuffer;
        var hookLoc = EldenRingOffsets.Hooks.SetEvent;

        AsmHelper.WriteRelativeOffsets(bytes, [
            (code + 0x8, writeIndex, 6, 0x8 + 2),
            (code + 0x13, buffer, 7, 0x13 + 3),
            (code + 0x2B, writeIndex, 6, 0x2B + 2),
            (code + 0x34, hookLoc + 0x5, 5, 0x34 + 1)
        ]);

        memoryService.WriteBytes(code, bytes);
        hookManager.InstallHook(code, hookLoc, [0x48, 0x89, 0x5C, 0x24, 0x08]);
    }

    public bool ShouldSplit()
    {
        var writeIndex = memoryService.Read<int>(_writeIndexAddr);
        if (writeIndex == _readIndex) return false;

        int entriesToRead = (writeIndex - _readIndex) & 511;
        int bytesToRead = entriesToRead * 5;

        if (bytesToRead > 0)
        {
            var dataBytes = memoryService.ReadBytes(_bufferAddr + (_readIndex * 5), bytesToRead);
            Console.WriteLine(bytesToRead);

            for (int i = 0; i < entriesToRead; i++)
            {
                var offset = i * 5;
                if (dataBytes[offset + 4] == 0) continue;

                var eventId = BitConverter.ToUInt32(dataBytes, offset);
                if (eldenRingEvents.ContainsKey(eventId))
                {
                    _readIndex = writeIndex;
                    return true;
                }
            }

            _readIndex = writeIndex;
        }

        return false;
    }
}