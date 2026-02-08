// 

using System;
using System.Windows.Threading;
using AutoHitCounter.Interfaces;
using AutoHitCounter.Memory;

namespace AutoHitCounter.Games.EldenRing;

public class EldenRingModule : IGameModule
{
    private readonly IMemoryService _memoryService;
    private readonly IStateService _stateService;
    private readonly EldenRingAsm _eldenRingAsm;
    private readonly DispatcherTimer _timer;

    public EldenRingModule(IMemoryService memoryService, IStateService stateService, HookManager hookManager)
    {
        _memoryService = memoryService;
        _stateService = stateService;
        _eldenRingAsm = new EldenRingAsm(memoryService, hookManager);
        
        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(64)
        };
        _timer.Tick += Tick;
    }

    public void Initialize()
    {
        InitializeOffsets();
        _eldenRingAsm.InstallEventHook();
        _eldenRingAsm.InstallHitHooks();
    }

    public event Action<int> OnHit;

    public event Action OnBossKilled;

    private void InitializeOffsets()
    {
        if (_memoryService.TargetProcess == null) return;
        var module = _memoryService.TargetProcess.MainModule;
        var fileVersion = module?.FileVersionInfo.FileVersion;
        var moduleBase = _memoryService.BaseAddress;
        EldenRingOffsets.Initialize(fileVersion, moduleBase);
    }

    private void Tick(object sender, EventArgs e)
    {
        throw new NotImplementedException();
    }
}