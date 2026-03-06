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
    private Dictionary<uint, string> _events = events;

    public abstract void InstallHook();

    protected IMemoryService MemoryService => memoryService;
    protected HookManager HookManager => hookManager;
    
    public void UpdateEvents(Dictionary<uint, string> events)
    {
        _events = events;
    }

    public bool ShouldSplit()
    {
        var writeIndex = memoryService.Read<int>(writeIdx);
        if (writeIndex == _readIndex) return false;

        int entriesToRead = (writeIndex - _readIndex) & 511;
        var dataBytes = ReadWrapping(entriesToRead);

        for (int i = 0; i < entriesToRead; i++)
        {
            var offset = i * 5;
            if (dataBytes[offset + 4] == 0) continue;

            var eventId = BitConverter.ToUInt32(dataBytes, offset);
            if (_events.ContainsKey(eventId))
            {
                _readIndex = writeIndex;
#if DEBUG
                Console.WriteLine($@"Event {eventId} set to true, splitting {_events[eventId]}");
#endif
                return true;
            }
        }

        _readIndex = writeIndex;
        return false;
    }

    private byte[] ReadWrapping(int entriesToRead)
    {
        int tail = 512 - _readIndex;
        if (entriesToRead <= tail)
            return memoryService.ReadBytes(logBuffer + (_readIndex * 5), entriesToRead * 5);

        var part1 = memoryService.ReadBytes(logBuffer + (_readIndex * 5), tail * 5);
        var part2 = memoryService.ReadBytes(logBuffer, (entriesToRead - tail) * 5);
        var result = new byte[entriesToRead * 5];
        part1.CopyTo(result, 0);
        part2.CopyTo(result, part1.Length);
        return result;
    }
}