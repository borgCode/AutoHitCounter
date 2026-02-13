// 

using System;
using System.Collections.Generic;
using AutoHitCounter.Interfaces;
using AutoHitCounter.Memory;

namespace AutoHitCounter.Services;

public abstract class EventServiceBase(
    IMemoryService memoryService,
    HookManager hookManager,
    Dictionary<uint, string> events)
    : IEventService
{
    private int _readIndex;

    private readonly nint _writeIndexAddr = CodeCaveOffsets.Base + CodeCaveOffsets.EventLogWriteIdx;
    private readonly nint _bufferAddr = CodeCaveOffsets.Base + CodeCaveOffsets.EventLogBuffer;

    public abstract void InstallHook();
    
    protected IMemoryService MemoryService => memoryService;
    protected HookManager HookManager => hookManager;
    
    public bool ShouldSplit()
    {
        var writeIndex = memoryService.Read<int>(_writeIndexAddr);
        if (writeIndex == _readIndex) return false;

        int entriesToRead = (writeIndex - _readIndex) & 511;
        int bytesToRead = entriesToRead * 5;

        if (bytesToRead > 0)
        {
            var dataBytes = memoryService.ReadBytes(_bufferAddr + (_readIndex * 5), bytesToRead);

            for (int i = 0; i < entriesToRead; i++)
            {
                var offset = i * 5;
                if (dataBytes[offset + 4] == 0) continue;

                var eventId = BitConverter.ToUInt32(dataBytes, offset);
                Console.WriteLine(eventId);
                if (events.ContainsKey(eventId))
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