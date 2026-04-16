//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using AutoHitCounter.Enums;
using AutoHitCounter.Interfaces;
using AutoHitCounter.Models;
using AutoHitCounter.ViewModels;

namespace AutoHitCounter.Services;

public class RunStateService : IRunStateService
{
    private readonly IProfileService _profileService;
    private readonly Timer _saveDebounce;
    private Profile _pendingProfile;

    public RunStateService(IProfileService profileService)
    {
        _profileService = profileService;
        _saveDebounce = new Timer(300) { AutoReset = false };
        _saveDebounce.Elapsed += (_, _) =>
        {
            if (_pendingProfile?.SavedRun != null)
                _profileService.SaveProfile(_pendingProfile);
        };
    }

    public void SaveRunState(Profile profile, IList<SplitViewModel> splits, SplitViewModel currentSplit, bool isRunComplete, TimeSpan inGameTime)
    {
        if (profile == null) return;

        var children = splits.Where(s => s.Type == SplitType.Child).ToList();
        profile.SavedRun = new RunState
        {
            CurrentSplitIndex = currentSplit != null ? splits.IndexOf(currentSplit) : -1,
            HitCounts = children.Select(s => s.NumOfHits).ToArray(),
            IsRunComplete = isRunComplete,
            IgtMilliseconds = (long)inGameTime.TotalMilliseconds
        };

        _pendingProfile = profile;
        _saveDebounce.Stop();
        _saveDebounce.Start();
    }

    public void CancelPendingSave()
    {
        _saveDebounce.Stop();
    }

    public void FlushRunState(Profile profile)
    {
        _saveDebounce.Stop();
        if (profile?.SavedRun != null)
            _profileService.SaveProfile(profile);
    }

    public RunSnapshot Capture(IList<SplitViewModel> splits, SplitViewModel currentSplit, bool isRunComplete, TimeSpan inGameTime)
    {
        var children = splits.Where(s => s.Type == SplitType.Child).ToList();
        var hits = children.Select(s => s.NumOfHits).ToArray();
        var index = currentSplit != null ? splits.IndexOf(currentSplit) : -1;
        return new RunSnapshot(index, hits, isRunComplete, inGameTime);
    }

    public SplitViewModel RestoreSnapshot(IList<SplitViewModel> splits, RunSnapshot snapshot)
    {
        RestoreHits(splits, snapshot.HitCounts);
        return FindSplitToRestore(splits, snapshot.CurrentSplitIndex);
    }

    public SplitViewModel RestoreFromSavedRun(IList<SplitViewModel> splits, RunState state)
    {
        RestoreHits(splits, state.HitCounts);
        return FindSplitToRestore(splits, state.CurrentSplitIndex);
    }

    public void Save(string gameName, string profileName, RunSnapshot snapshot)
    {
        _snapshots[Key(gameName, profileName)] = snapshot;
    }

    public bool TryGet(string gameName, string profileName, out RunSnapshot snapshot)
    {
        return _snapshots.TryGetValue(Key(gameName, profileName), out snapshot);
    }

    public void Invalidate(string gameName, string profileName)
    {
        if (gameName == null || profileName == null) return;
        _snapshots.Remove(Key(gameName, profileName));
    }

    public void InvalidateStale(string gameName, IEnumerable<string> validProfileNames)
    {
        var validKeys = new HashSet<string>(validProfileNames.Select(p => Key(gameName, p)));
        var stale = _snapshots.Keys
            .Where(k => k.StartsWith($"{gameName}|") && !validKeys.Contains(k))
            .ToList();
        foreach (var key in stale)
            _snapshots.Remove(key);
    }

    public void RenameGame(string oldName, string newName)
    {
        var toRename = _snapshots.Keys.Where(k => k.StartsWith($"{oldName}|")).ToList();
        foreach (var key in toRename)
        {
            var snapshot = _snapshots[key];
            _snapshots.Remove(key);
            _snapshots[key.Replace($"{oldName}|", $"{newName}|")] = snapshot;
        }
    }

    private readonly Dictionary<string, RunSnapshot> _snapshots = new();

    private static string Key(string gameName, string profileName) => $"{gameName}|{profileName}";

    private static void RestoreHits(IList<SplitViewModel> splits, int[] hitCounts)
    {
        var children = splits.Where(s => s.Type == SplitType.Child).ToList();
        for (int i = 0; i < children.Count && i < hitCounts.Length; i++)
            children[i].NumOfHits = hitCounts[i];
    }

    private static SplitViewModel FindSplitToRestore(IList<SplitViewModel> splits, int currentSplitIndex)
    {
        return currentSplitIndex >= 0 && currentSplitIndex < splits.Count
            ? splits[currentSplitIndex]
            : splits.FirstOrDefault(s => s.Type == SplitType.Child);
    }
}