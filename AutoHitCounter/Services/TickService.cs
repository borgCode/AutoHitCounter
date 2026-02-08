// 

using System;
using System.Windows.Threading;
using AutoHitCounter.Interfaces;

namespace AutoHitCounter.Services;

public class TickService : ITickService
{
    private readonly IMemoryService _memoryService;
    private readonly DispatcherTimer _mainTimer;

    public TickService(IMemoryService memoryService)
    {
        _memoryService = memoryService;
        _mainTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(25)
        };
        _mainTimer.Tick += MainTick;
    }
    
    public void Start() => _mainTimer.Start();

    public void Stop() => _mainTimer.Stop();
    
    private void MainTick(object sender, EventArgs e)
    {
        if (_memoryService.IsAttached)
        {
            
        }
    }
}