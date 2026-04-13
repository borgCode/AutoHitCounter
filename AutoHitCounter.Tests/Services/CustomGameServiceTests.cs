//

using System.Collections.Generic;
using System.Linq;
using AutoHitCounter.Enums;
using AutoHitCounter.Interfaces;
using AutoHitCounter.Models;
using AutoHitCounter.Services;
using NSubstitute;
using Xunit;

namespace AutoHitCounter.Tests.Services;

public class CustomGameServiceTests
{
    private readonly FakeCustomGamesStore _store = new();
    private readonly IProfileService _profileService = Substitute.For<IProfileService>();
    private readonly RunStateService _runStateService;
    private readonly CustomGameService _sut;

    public CustomGameServiceTests()
    {
        _runStateService = new RunStateService(_profileService);
        _sut = new CustomGameService(_store, _profileService, _runStateService);
    }

    #region Load

    [Fact]
    public void Load_EmptyRaw_ReturnsEmpty()
    {
        _store.Raw = "";

        Assert.Empty(_sut.Load());
    }

    [Fact]
    public void Load_NullRaw_ReturnsEmpty()
    {
        _store.Raw = null;

        Assert.Empty(_sut.Load());
    }

    [Fact]
    public void Load_WhitespaceRaw_ReturnsEmpty()
    {
        _store.Raw = "   ";

        Assert.Empty(_sut.Load());
    }

    [Fact]
    public void Load_SingleName_ReturnsOneGame()
    {
        _store.Raw = "Bloodborne";

        var games = _sut.Load();

        var game = Assert.Single(games);
        Assert.Equal("Bloodborne", game.GameName);
    }

    [Fact]
    public void Load_ReturnedGame_HasManualTitleAndIsManualTrueAndNullProcessName()
    {
        _store.Raw = "Bloodborne";

        var game = _sut.Load().Single();

        Assert.Equal(GameTitle.Manual, game.Title);
        Assert.True(game.IsManual);
        Assert.Null(game.ProcessName);
    }

    [Fact]
    public void Load_MultipleNames_ReturnsAllInOrder()
    {
        _store.Raw = "Alpha,Beta,Gamma";

        var names = _sut.Load().Select(g => g.GameName).ToArray();

        Assert.Equal(new[] { "Alpha", "Beta", "Gamma" }, names);
    }

    [Fact]
    public void Load_TrimsWhitespaceAroundNames()
    {
        _store.Raw = "  Alpha , Beta  ,Gamma ";

        var names = _sut.Load().Select(g => g.GameName).ToArray();

        Assert.Equal(new[] { "Alpha", "Beta", "Gamma" }, names);
    }

    [Fact]
    public void Load_SkipsEmptyEntries()
    {
        _store.Raw = "Alpha,,Beta,  ,Gamma";

        var names = _sut.Load().Select(g => g.GameName).ToArray();

        Assert.Equal(new[] { "Alpha", "Beta", "Gamma" }, names);
    }

    [Fact]
    public void Load_TrailingComma_DoesNotAddEmptyGame()
    {
        _store.Raw = "Alpha,";

        var names = _sut.Load().Select(g => g.GameName).ToArray();

        Assert.Equal(new[] { "Alpha" }, names);
    }

    #endregion

    #region Add

    [Fact]
    public void Add_EmptyStore_WritesJustTheName()
    {
        _store.Raw = "";

        _sut.Add("Bloodborne");

        Assert.Equal("Bloodborne", _store.Raw);
    }

    [Fact]
    public void Add_NullStore_WritesJustTheName()
    {
        _store.Raw = null;

        _sut.Add("Bloodborne");

        Assert.Equal("Bloodborne", _store.Raw);
    }

    [Fact]
    public void Add_NonEmptyStore_AppendsWithComma()
    {
        _store.Raw = "Alpha";

        _sut.Add("Beta");

        Assert.Equal("Alpha,Beta", _store.Raw);
    }

    [Fact]
    public void Add_MultipleExistingEntries_AppendsToEnd()
    {
        _store.Raw = "Alpha,Beta";

        _sut.Add("Gamma");

        Assert.Equal("Alpha,Beta,Gamma", _store.Raw);
    }

    [Fact]
    public void Add_PersistsToStore()
    {
        _store.Raw = "";

        _sut.Add("Bloodborne");

        Assert.True(_store.SaveCalled);
    }

    [Fact]
    public void Add_ReturnedGame_HasManualTitleAndCorrectName()
    {
        _store.Raw = "";

        var game = _sut.Add("Bloodborne");

        Assert.Equal("Bloodborne", game.GameName);
        Assert.Equal(GameTitle.Manual, game.Title);
        Assert.True(game.IsManual);
        Assert.Null(game.ProcessName);
    }

    [Fact]
    public void Add_ThenLoad_RoundTripsName()
    {
        _store.Raw = "";

        _sut.Add("Bloodborne");
        var loaded = _sut.Load().Select(g => g.GameName).ToArray();

        Assert.Equal(new[] { "Bloodborne" }, loaded);
    }

    #endregion

    #region Rename

    [Fact]
    public void Rename_ReplacesNameInCsv()
    {
        _store.Raw = "Alpha,Beta,Gamma";

        _sut.Rename("Beta", "Delta");

        Assert.Equal("Alpha,Delta,Gamma", _store.Raw);
    }

    [Fact]
    public void Rename_OnlyRenamesMatchingEntry()
    {
        _store.Raw = "Beta,BetaOther,Beta";

        _sut.Rename("Beta", "Delta");

        Assert.Equal("Delta,BetaOther,Delta", _store.Raw);
    }

    [Fact]
    public void Rename_NameNotInCsv_LeavesCsvUnchanged()
    {
        _store.Raw = "Alpha,Beta";

        _sut.Rename("Missing", "Delta");

        Assert.Equal("Alpha,Beta", _store.Raw);
    }

    [Fact]
    public void Rename_NormalizesWhitespaceAroundOtherEntries()
    {
        _store.Raw = "  Alpha ,Beta, Gamma ";

        _sut.Rename("Beta", "Delta");

        Assert.Equal("Alpha,Delta,Gamma", _store.Raw);
    }

    [Fact]
    public void Rename_PersistsToStore()
    {
        _store.Raw = "Alpha";

        _sut.Rename("Alpha", "Beta");

        Assert.True(_store.SaveCalled);
    }

    [Fact]
    public void Rename_CallsProfileServiceRenameGame()
    {
        _store.Raw = "Alpha";

        _sut.Rename("Alpha", "Beta");

        _profileService.Received(1).RenameGame("Alpha", "Beta");
    }

    [Fact]
    public void Rename_RenamesRunStateSnapshots()
    {
        _store.Raw = "Alpha";
        _runStateService.Save("Alpha", "P1", new Models.RunSnapshot(0, new[] { 5 }, false, System.TimeSpan.Zero));

        _sut.Rename("Alpha", "Beta");

        Assert.False(_runStateService.TryGet("Alpha", "P1", out _));
        Assert.True(_runStateService.TryGet("Beta", "P1", out var snapshot));
        Assert.Equal(new[] { 5 }, snapshot.HitCounts);
    }

    #endregion

    #region Delete

    [Fact]
    public void Delete_RemovesNameFromCsv()
    {
        _store.Raw = "Alpha,Beta,Gamma";
        _profileService.GetProfiles("Beta").Returns(new List<Profile>());

        _sut.Delete("Beta");

        Assert.Equal("Alpha,Gamma", _store.Raw);
    }

    [Fact]
    public void Delete_OnlyRemaining_LeavesEmptyCsv()
    {
        _store.Raw = "Alpha";
        _profileService.GetProfiles("Alpha").Returns(new List<Profile>());

        _sut.Delete("Alpha");

        Assert.Equal("", _store.Raw);
    }

    [Fact]
    public void Delete_NameNotInCsv_LeavesOtherEntriesUnchanged()
    {
        _store.Raw = "Alpha,Beta";
        _profileService.GetProfiles("Missing").Returns(new List<Profile>());

        _sut.Delete("Missing");

        Assert.Equal("Alpha,Beta", _store.Raw);
    }

    [Fact]
    public void Delete_NormalizesWhitespaceAroundRemainingEntries()
    {
        _store.Raw = "  Alpha , Beta, Gamma ";
        _profileService.GetProfiles("Beta").Returns(new List<Profile>());

        _sut.Delete("Beta");

        Assert.Equal("Alpha,Gamma", _store.Raw);
    }

    [Fact]
    public void Delete_DuplicateEntries_RemovesAll()
    {
        _store.Raw = "Alpha,Beta,Alpha";
        _profileService.GetProfiles("Alpha").Returns(new List<Profile>());

        _sut.Delete("Alpha");

        Assert.Equal("Beta", _store.Raw);
    }

    [Fact]
    public void Delete_PersistsToStore()
    {
        _store.Raw = "Alpha";
        _profileService.GetProfiles("Alpha").Returns(new List<Profile>());

        _sut.Delete("Alpha");

        Assert.True(_store.SaveCalled);
    }

    [Fact]
    public void Delete_DeletesEachProfileForTheGame()
    {
        _store.Raw = "Alpha";
        _profileService.GetProfiles("Alpha").Returns(new List<Profile>
        {
            new() { Name = "P1", GameName = "Alpha" },
            new() { Name = "P2", GameName = "Alpha" },
        });

        _sut.Delete("Alpha");

        _profileService.Received(1).DeleteProfile("Alpha", "P1");
        _profileService.Received(1).DeleteProfile("Alpha", "P2");
    }

    [Fact]
    public void Delete_NoProfiles_StillUpdatesCsv()
    {
        _store.Raw = "Alpha,Beta";
        _profileService.GetProfiles("Alpha").Returns(new List<Profile>());

        _sut.Delete("Alpha");

        Assert.Equal("Beta", _store.Raw);
        _profileService.DidNotReceive().DeleteProfile(Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public void Delete_ClearsRunStateSnapshotsForGame()
    {
        _store.Raw = "Alpha,Beta";
        _profileService.GetProfiles("Alpha").Returns(new List<Profile>());
        _runStateService.Save("Alpha", "P1", new RunSnapshot(0, new[] { 1 }, false, System.TimeSpan.Zero));
        _runStateService.Save("Alpha", "P2", new RunSnapshot(0, new[] { 2 }, false, System.TimeSpan.Zero));
        _runStateService.Save("Beta", "P1", new RunSnapshot(0, new[] { 9 }, false, System.TimeSpan.Zero));

        _sut.Delete("Alpha");

        Assert.False(_runStateService.TryGet("Alpha", "P1", out _));
        Assert.False(_runStateService.TryGet("Alpha", "P2", out _));
        Assert.True(_runStateService.TryGet("Beta", "P1", out _));
    }

    #endregion

    #region IsValidName

    [Theory]
    [InlineData("Bloodborne")]
    [InlineData("Foo Bar")]
    [InlineData("X")]
    public void IsValidName_AcceptsNonEmptyNamesWithoutComma(string name)
    {
        Assert.True(CustomGameService.IsValidName(name));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void IsValidName_RejectsNullOrWhitespace(string name)
    {
        Assert.False(CustomGameService.IsValidName(name));
    }

    [Theory]
    [InlineData("Foo,Bar")]
    [InlineData("Foo, Bar")]
    [InlineData(",Foo")]
    [InlineData("Foo,")]
    public void IsValidName_RejectsNamesContainingComma(string name)
    {
        Assert.False(CustomGameService.IsValidName(name));
    }

    #endregion

    private class FakeCustomGamesStore : ICustomGamesStore
    {
        public string Raw { get; set; } = "";
        public bool SaveCalled { get; private set; }
        public void Save() => SaveCalled = true;
    }
}
