//

using System;
using System.Collections.Generic;
using AutoHitCounter.Interfaces;
using AutoHitCounter.Models;
using AutoHitCounter.Services;
using NSubstitute;
using Xunit;

namespace AutoHitCounter.Tests.Services;

public class GameSessionOrchestratorTests
{
    private readonly IMemoryService _memoryService = Substitute.For<IMemoryService>();
    private readonly IHotkeyManager _hotkeyManager = Substitute.For<IHotkeyManager>();
    private readonly IGameModuleFactory _factory = Substitute.For<IGameModuleFactory>();
    private readonly StateService _stateService = new();
    private readonly GameSessionOrchestrator _sut;

    public GameSessionOrchestratorTests()
    {
        _sut = new GameSessionOrchestrator(_memoryService, _hotkeyManager, _factory, _stateService);
        _sut.Initialize(
            Substitute.For<IHitRulesProvider>(),
            () => new Dictionary<uint, (string Name, int Required, int Hit)>());
    }

    [Fact]
    public void Track_CreatesModuleViaFactory()
    {
        var game = new Game { GameName = "DS3", ProcessName = "darksoulsiii", IsManual = false };
        _factory.CreateModule(Arg.Any<Game>(), Arg.Any<Dictionary<uint, (string, int, int)>>(), Arg.Any<IHitRulesProvider>())
            .Returns(Substitute.For<IGameModule>());

        _sut.Track(game);

        _factory.Received(1).CreateModule(game, Arg.Any<Dictionary<uint, (string, int, int)>>(), Arg.Any<IHitRulesProvider>());
    }

    [Fact]
    public void Track_WhenModuleFiresOnHit_HitReceivedIsForwarded()
    {
        var module = Substitute.For<IGameModule>();
        _factory.CreateModule(Arg.Any<Game>(), Arg.Any<Dictionary<uint, (string, int, int)>>(), Arg.Any<IHitRulesProvider>())
            .Returns(module);
        _sut.Track(new Game { GameName = "DS3", ProcessName = "darksoulsiii", IsManual = false });

        var fired = false;
        _sut.HitReceived += () => fired = true;
        module.OnHit += Raise.Event<Action>();

        Assert.True(fired);
    }
}
