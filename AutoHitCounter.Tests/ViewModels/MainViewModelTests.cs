//

using System;
using System.Collections.Generic;
using AutoHitCounter.Enums;
using AutoHitCounter.Interfaces;
using AutoHitCounter.Models;
using AutoHitCounter.ViewModels;
using NSubstitute;
using Xunit;

namespace AutoHitCounter.Tests.ViewModels;

public class MainViewModelTests
{
    private readonly IHotkeyManager _hotkeyManager = Substitute.For<IHotkeyManager>();
    private readonly IGameModuleFactory _gameModuleFactory = Substitute.For<IGameModuleFactory>();
    private readonly IProfileService _profileService = Substitute.For<IProfileService>();
    private readonly IStateService _stateService = Substitute.For<IStateService>();
    private readonly IOverlayServerService _overlayServerService = Substitute.For<IOverlayServerService>();
    private readonly ISplitNavigationService _splitNav = Substitute.For<ISplitNavigationService>();
    private readonly IExternalIntegrationService _externalIntegration = Substitute.For<IExternalIntegrationService>();
    private readonly IGameSessionOrchestrator _orchestrator = Substitute.For<IGameSessionOrchestrator>();
    private readonly IRunStateService _runStateService = Substitute.For<IRunStateService>();
    private readonly ICustomGameService _customGameService = Substitute.For<ICustomGameService>();

    private readonly MainViewModel _sut;

    public MainViewModelTests()
    {
        _gameModuleFactory.GetRegisteredGames().Returns(new List<Game>());
        _customGameService.Load().Returns(new List<Game>());
        _profileService.GetProfiles(Arg.Any<string>()).Returns(new List<Profile>());

        _sut = new MainViewModel(
            _hotkeyManager, _gameModuleFactory, _profileService, _stateService,
            null, null, _overlayServerService, _splitNav, _externalIntegration,
            _orchestrator, _runStateService, _customGameService);
    }

    private SplitViewModel AddChildSplit(string name = "Boss", int hits = 0, int pb = 0)
    {
        var split = new SplitViewModel { Name = name, Type = SplitType.Child, NumOfHits = hits, PersonalBest = pb };
        _sut.Splits.Add(split);
        return split;
    }

    private void SetCurrentSplit(SplitViewModel split)
    {
        _splitNav.CurrentSplit.Returns(split);
        _splitNav.IsRunComplete.Returns(false);
    }

    private Profile SetupProfileWithSplits(params (string Name, SplitType Type)[] splits)
    {
        var profile = new Profile
        {
            Name = "TestProfile",
            Splits = new List<SplitEntry>()
        };
        foreach (var (name, type) in splits)
            profile.Splits.Add(new SplitEntry { Name = name, Type = type });

        _sut.ActiveProfile = profile;
        return profile;
    }

    #region IncrementHit

    [Fact]
    public void IncrementHit_IncrementsCurrentSplitHits()
    {
        var split = AddChildSplit(hits: 0);
        SetCurrentSplit(split);

        _sut.IncrementHitCommand.Execute(null);

        Assert.Equal(1, split.NumOfHits);
    }

    [Fact]
    public void IncrementHit_CallsSaveRunState()
    {
        var split = AddChildSplit();
        SetCurrentSplit(split);

        _sut.IncrementHitCommand.Execute(null);

        _runStateService.Received(1).SaveRunState(
            Arg.Any<Profile>(), Arg.Any<IList<SplitViewModel>>(),
            Arg.Any<SplitViewModel>(), Arg.Any<bool>(), Arg.Any<TimeSpan>());
    }

    [Fact]
    public void IncrementHit_BroadcastsOverlayState()
    {
        var split = AddChildSplit();
        SetCurrentSplit(split);
        _overlayServerService.ClearReceivedCalls();

        _sut.IncrementHitCommand.Execute(null);

        _overlayServerService.Received(1).BroadcastState(Arg.Any<OverlayState>());
    }

    [Fact]
    public void IncrementHit_WhenRunComplete_DoesNothing()
    {
        var split = AddChildSplit(hits: 0);
        _splitNav.CurrentSplit.Returns(split);
        _splitNav.IsRunComplete.Returns(true);

        _sut.IncrementHitCommand.Execute(null);

        Assert.Equal(0, split.NumOfHits);
    }

    [Fact]
    public void IncrementHit_WhenCurrentSplitNull_DoesNothing()
    {
        _splitNav.CurrentSplit.Returns((SplitViewModel)null);
        _splitNav.IsRunComplete.Returns(false);

        _sut.IncrementHitCommand.Execute(null);

        _runStateService.DidNotReceive().SaveRunState(
            Arg.Any<Profile>(), Arg.Any<IList<SplitViewModel>>(),
            Arg.Any<SplitViewModel>(), Arg.Any<bool>(), Arg.Any<TimeSpan>());
    }

    [Fact]
    public void IncrementHit_WhenPracticeMode_DoesNothing()
    {
        var split = AddChildSplit(hits: 0);
        SetCurrentSplit(split);
        _sut.IsPracticeMode = true;

        _sut.IncrementHitCommand.Execute(null);

        Assert.Equal(0, split.NumOfHits);
    }

    #endregion

    #region DecrementHit

    [Fact]
    public void DecrementHit_DecrementsCurrentSplitHits()
    {
        var split = AddChildSplit(hits: 2);
        SetCurrentSplit(split);

        _sut.DecrementHitCommand.Execute(null);

        Assert.Equal(1, split.NumOfHits);
    }

    [Fact]
    public void DecrementHit_DoesNotGoBelowZero()
    {
        var split = AddChildSplit(hits: 0);
        SetCurrentSplit(split);

        _sut.DecrementHitCommand.Execute(null);

        Assert.Equal(0, split.NumOfHits);
    }

    [Fact]
    public void DecrementHit_WhenRunComplete_DoesNothing()
    {
        var split = AddChildSplit(hits: 2);
        _splitNav.CurrentSplit.Returns(split);
        _splitNav.IsRunComplete.Returns(true);

        _sut.DecrementHitCommand.Execute(null);

        Assert.Equal(2, split.NumOfHits);
    }

    [Fact]
    public void DecrementHit_WhenCurrentSplitNull_DoesNothing()
    {
        _splitNav.CurrentSplit.Returns((SplitViewModel)null);
        _splitNav.IsRunComplete.Returns(false);

        _sut.DecrementHitCommand.Execute(null);

        _runStateService.DidNotReceive().SaveRunState(
            Arg.Any<Profile>(), Arg.Any<IList<SplitViewModel>>(),
            Arg.Any<SplitViewModel>(), Arg.Any<bool>(), Arg.Any<TimeSpan>());
    }

    #endregion

    #region ResetRun

    [Fact]
    public void Reset_CancelsPendingSave()
    {
        SetupProfileWithSplits(("Boss", SplitType.Child));
        _runStateService.ClearReceivedCalls();

        _sut.ResetCommand.Execute(null);

        _runStateService.Received().CancelPendingSave();
    }

    [Fact]
    public void Reset_IncrementsAttemptCount()
    {
        var profile = SetupProfileWithSplits(("Boss", SplitType.Child));
        profile.AttemptCount = 5;

        _sut.ResetCommand.Execute(null);

        Assert.Equal(6, profile.AttemptCount);
    }

    [Fact]
    public void Reset_ClearsSavedRun()
    {
        var profile = SetupProfileWithSplits(("Boss", SplitType.Child));
        profile.SavedRun = new RunState { CurrentSplitIndex = 0, HitCounts = new[] { 1 } };

        _sut.ResetCommand.Execute(null);

        Assert.Null(profile.SavedRun);
    }

    [Fact]
    public void Reset_SavesProfile()
    {
        var profile = SetupProfileWithSplits(("Boss", SplitType.Child));
        _profileService.ClearReceivedCalls();

        _sut.ResetCommand.Execute(null);

        _profileService.Received().SaveProfile(profile);
    }

    [Fact]
    public void Reset_InvalidatesRunState()
    {
        var profile = SetupProfileWithSplits(("Boss", SplitType.Child));
        _runStateService.ClearReceivedCalls();

        _sut.ResetCommand.Execute(null);

        _runStateService.Received().Invalidate(Arg.Any<string>(), profile.Name);
    }

    [Fact]
    public void Reset_CallsInitFresh()
    {
        SetupProfileWithSplits(("Boss", SplitType.Child));
        _splitNav.ClearReceivedCalls();

        _sut.ResetCommand.Execute(null);

        _splitNav.Received().InitFresh();
    }

    [Fact]
    public void Reset_ResetsIgtToZero()
    {
        SetupProfileWithSplits(("Boss", SplitType.Child));

        _sut.ResetCommand.Execute(null);

        Assert.Equal(TimeSpan.Zero, _sut.InGameTime);
        Assert.Equal("0:00:00", _sut.InGameTimeFormatted);
    }

    [Fact]
    public void Reset_CallsOrchestratorManualReset()
    {
        SetupProfileWithSplits(("Boss", SplitType.Child));
        _orchestrator.ClearReceivedCalls();

        _sut.ResetCommand.Execute(null);

        _orchestrator.Received().ManualReset();
    }

    [Fact]
    public void Reset_BroadcastsStateAndIgt()
    {
        SetupProfileWithSplits(("Boss", SplitType.Child));
        _overlayServerService.ClearReceivedCalls();

        _sut.ResetCommand.Execute(null);

        _overlayServerService.Received().BroadcastState(Arg.Any<OverlayState>());
        _overlayServerService.Received().BroadcastIgt("0:00:00");
    }

    #endregion

    #region ActiveEvents derivation

    private Profile SetupProfileWithEventSplits(params (string Name, SplitType Type, uint? EventId)[] splits)
    {
        var profile = new Profile { Name = "TestProfile", Splits = new List<SplitEntry>() };
        foreach (var (name, type, eventId) in splits)
            profile.Splits.Add(new SplitEntry { Name = name, Type = type, EventId = eventId });
        _sut.ActiveProfile = profile;
        return profile;
    }

    private Dictionary<uint, (string Name, int Required, int Hit)> CaptureLastEventsDict()
    {
        Dictionary<uint, (string Name, int Required, int Hit)> captured = null;
        foreach (var call in _orchestrator.ReceivedCalls())
        {
            if (call.GetMethodInfo().Name != nameof(_orchestrator.UpdateEvents)) continue;
            captured = (Dictionary<uint, (string Name, int Required, int Hit)>)call.GetArguments()[0];
        }
        return captured;
    }

    [Fact]
    public void GetActiveEvents_FreshLoad_SeedsAllHitsToZero()
    {
        SetupProfileWithEventSplits(
            ("Margit1", SplitType.Child, 10000850u),
            ("Margit2", SplitType.Child, 10000850u),
            ("Godrick", SplitType.Child, 10000800u));

        var dict = CaptureLastEventsDict();

        Assert.Equal(2, dict[10000850u].Required);
        Assert.Equal(0, dict[10000850u].Hit);
        Assert.Equal(1, dict[10000800u].Required);
        Assert.Equal(0, dict[10000800u].Hit);
    }

    [Fact]
    public void GetActiveEvents_MidRun_SeedsHitFromCompletedSplits()
    {
        SetupProfileWithEventSplits(
            ("Margit1", SplitType.Child, 10000850u),
            ("Margit2", SplitType.Child, 10000850u),
            ("Margit3", SplitType.Child, 10000850u),
            ("Margit4", SplitType.Child, 10000850u));

        _splitNav.CurrentSplit.Returns(_sut.Splits[2]);
        _splitNav.IsRunComplete.Returns(false);
        _orchestrator.ClearReceivedCalls();

        _splitNav.StateChanged += Raise.Event<Action>();

        var dict = CaptureLastEventsDict();
        Assert.Equal(4, dict[10000850u].Required);
        Assert.Equal(2, dict[10000850u].Hit);
    }

    [Fact]
    public void GetActiveEvents_RunComplete_SeedsHitToRequired()
    {
        SetupProfileWithEventSplits(
            ("Margit1", SplitType.Child, 10000850u),
            ("Margit2", SplitType.Child, 10000850u));

        _splitNav.CurrentSplit.Returns(_sut.Splits[1]);
        _splitNav.IsRunComplete.Returns(true);
        _orchestrator.ClearReceivedCalls();

        _splitNav.StateChanged += Raise.Event<Action>();

        var dict = CaptureLastEventsDict();
        Assert.Equal(2, dict[10000850u].Required);
        Assert.Equal(2, dict[10000850u].Hit);
    }

    [Fact]
    public void GetActiveEvents_OnlyCountsMatchingEventIdBeforeCutoff()
    {
        SetupProfileWithEventSplits(
            ("Margit", SplitType.Child, 10000850u),
            ("Godrick", SplitType.Child, 10000800u),
            ("Margit2", SplitType.Child, 10000850u),
            ("Rennala", SplitType.Child, 10000900u));

        _splitNav.CurrentSplit.Returns(_sut.Splits[3]);
        _splitNav.IsRunComplete.Returns(false);
        _orchestrator.ClearReceivedCalls();

        _splitNav.StateChanged += Raise.Event<Action>();

        var dict = CaptureLastEventsDict();
        Assert.Equal(2, dict[10000850u].Hit);
        Assert.Equal(1, dict[10000800u].Hit);
        Assert.Equal(0, dict[10000900u].Hit);
    }

    [Fact]
    public void GetActiveEvents_IgnoresSplitsWithoutEventId()
    {
        SetupProfileWithEventSplits(
            ("Manual", SplitType.Child, (uint?)null),
            ("Margit", SplitType.Child, 10000850u));

        var dict = CaptureLastEventsDict();
        Assert.Single(dict);
        Assert.True(dict.ContainsKey(10000850u));
    }

    #endregion

    #region SplitNav state change propagation

    [Fact]
    public void OnSplitStateChanged_CallsUpdateEventsOnOrchestrator()
    {
        SetupProfileWithEventSplits(("Margit", SplitType.Child, 10000850u));
        _orchestrator.ClearReceivedCalls();

        _splitNav.StateChanged += Raise.Event<Action>();

        _orchestrator.Received().UpdateEvents(
            Arg.Any<Dictionary<uint, (string Name, int Required, int Hit)>>());
    }

    [Fact]
    public void OnSplitStateChanged_WhenActiveGameDiffersFromSelected_DoesNotUpdateEvents()
    {
        SetupProfileWithEventSplits(("Margit", SplitType.Child, 10000850u));
        _orchestrator.ActiveGame.Returns(new Game { GameName = "Other" });
        _orchestrator.ClearReceivedCalls();

        _splitNav.StateChanged += Raise.Event<Action>();

        _orchestrator.DidNotReceive().UpdateEvents(
            Arg.Any<Dictionary<uint, (string Name, int Required, int Hit)>>());
    }

    [Fact]
    public void OnSplitStateChanged_AfterJumpingForward_SeedsEventHitFromNewPosition()
    {
        SetupProfileWithEventSplits(
            ("Margit1", SplitType.Child, 10000850u),
            ("Margit2", SplitType.Child, 10000850u),
            ("Margit3", SplitType.Child, 10000850u));

        _splitNav.CurrentSplit.Returns(_sut.Splits[2]);
        _orchestrator.ClearReceivedCalls();

        _splitNav.StateChanged += Raise.Event<Action>();

        var dict = CaptureLastEventsDict();
        Assert.Equal(2, dict[10000850u].Hit);
    }

    #endregion

    #region SetPb

    [Fact]
    public void SetPb_CopiesHitsToPersonalBest()
    {
        SetupProfileWithSplits(
            ("Boss1", SplitType.Child),
            ("Boss2", SplitType.Child),
            ("Boss3", SplitType.Child));

        _sut.Splits[0].NumOfHits = 1;
        _sut.Splits[1].NumOfHits = 2;
        _sut.Splits[2].NumOfHits = 3;

        _sut.SetPbCommand.Execute(null);

        Assert.Equal(1, _sut.Splits[0].PersonalBest);
        Assert.Equal(2, _sut.Splits[1].PersonalBest);
        Assert.Equal(3, _sut.Splits[2].PersonalBest);
    }

    [Fact]
    public void SetPb_SkipsParentSplits()
    {
        SetupProfileWithSplits(
            ("Group", SplitType.Parent),
            ("Boss1", SplitType.Child));

        _sut.Splits[0].NumOfHits = 99;
        _sut.Splits[1].NumOfHits = 5;

        _sut.SetPbCommand.Execute(null);

        Assert.Equal(0, _sut.Splits[0].PersonalBest);
        Assert.Equal(5, _sut.Splits[1].PersonalBest);
    }

    [Fact]
    public void SetPb_UpdatesProfileSplits()
    {
        var profile = SetupProfileWithSplits(
            ("Boss1", SplitType.Child),
            ("Boss2", SplitType.Child));

        _sut.Splits[0].NumOfHits = 3;
        _sut.Splits[1].NumOfHits = 7;

        _sut.SetPbCommand.Execute(null);

        Assert.Equal(3, profile.Splits[0].PersonalBest);
        Assert.Equal(7, profile.Splits[1].PersonalBest);
    }

    [Fact]
    public void SetPb_SavesProfile()
    {
        var profile = SetupProfileWithSplits(("Boss", SplitType.Child));
        _profileService.ClearReceivedCalls();

        _sut.SetPbCommand.Execute(null);

        _profileService.Received().SaveProfile(profile);
    }

    [Fact]
    public void SetPb_BroadcastsOverlayState()
    {
        SetupProfileWithSplits(("Boss", SplitType.Child));
        _overlayServerService.ClearReceivedCalls();

        _sut.SetPbCommand.Execute(null);

        _overlayServerService.Received().BroadcastState(Arg.Any<OverlayState>());
    }

    #endregion
}
