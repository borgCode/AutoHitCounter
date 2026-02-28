// 

using System;
using System.Collections.Generic;
using AutoHitCounter.Interfaces;
using AutoHitCounter.Memory;

namespace AutoHitCounter.Services;

public abstract class EventServiceBase(
    IMemoryService memoryService,
    HookManager hookManager,
    Dictionary<uint, string> events,
    nint writeIdx,
    nint logBuffer)
    : IEventService
{
    private int _readIndex;

    public abstract void InstallHook();
    
    protected IMemoryService MemoryService => memoryService;
    protected HookManager HookManager => hookManager;
    
    public bool ShouldSplit()
    {
        var writeIndex = memoryService.Read<int>(writeIdx);
        if (writeIndex == _readIndex) return false;

        int entriesToRead = (writeIndex - _readIndex) & 511;
        int bytesToRead = entriesToRead * 5;

        if (bytesToRead > 0)
        {
            var dataBytes = memoryService.ReadBytes(logBuffer + (_readIndex * 5), bytesToRead);

            for (int i = 0; i < entriesToRead; i++)
            {
                var offset = i * 5;
                if (dataBytes[offset + 4] == 0) continue;

                var eventId = BitConverter.ToUInt32(dataBytes, offset);
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