// 

using System;
using System.Windows.Threading;
using AutoHitCounter.Enums;
using AutoHitCounter.Interfaces;
using AutoHitCounter.Memory;

namespace AutoHitCounter.Services;

public class TickService : ITickService
{
    private readonly IMemoryService _memoryService;
    private readonly IStateService _stateService;
    private readonly DispatcherTimer _mainTimer;
    
    private Action? _gameTick;
    private DateTime? _attachedTime;
    private bool _hasAllocatedMemory;

    public TickService(IMemoryService memoryService, IStateService stateService) 
    {
        _memoryService = memoryService;
        _stateService = stateService;
        _mainTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(100)
        };
        _mainTimer.Tick += MainTick;
        _mainTimer.Start();
    }
    
    public void RegisterGameTick(Action tick) => _gameTick = tick;
    public void UnregisterGameTick() => _gameTick = null;
    
    
    private void MainTick(object sender, EventArgs e)
    {
        if (_memoryService.IsAttached)
        {
            if (!_attachedTime.HasValue)
            {
                _attachedTime = DateTime.Now;
                return;
            }
            
            if ((DateTime.Now - _attachedTime.Value).TotalSeconds < 2)
                return;
            
            if (!_hasAllocatedMemory)
            {
                _memoryService.AllocCodeCave();
#if DEBUG
                Console.WriteLine($@"Code cave: 0x{(long)CodeCaveOffsets.Base:X}");
#endif
                _stateService.Publish(State.Attached);
                _hasAllocatedMemory = true;
            }
            
            _gameTick?.Invoke();
        }
    }

    
}