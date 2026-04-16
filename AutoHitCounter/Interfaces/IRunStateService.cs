//

using System;
using System.Collections.Generic;
using AutoHitCounter.Models;
using AutoHitCounter.ViewModels;

namespace AutoHitCounter.Interfaces;

public interface IRunStateService
{
    void SaveRunState(Profile profile, IList<SplitViewModel> splits, SplitViewModel currentSplit, bool isRunComplete, TimeSpan inGameTime);
    void CancelPendingSave();
    void FlushRunState(Profile profile);
    RunSnapshot Capture(IList<SplitViewModel> splits, SplitViewModel currentSplit, bool isRunComplete, TimeSpan inGameTime);
    SplitViewModel RestoreSnapshot(IList<SplitViewModel> splits, RunSnapshot snapshot);
    SplitViewModel RestoreFromSavedRun(IList<SplitViewModel> splits, RunState state);
    void Save(string gameName, string profileName, RunSnapshot snapshot);
    bool TryGet(string gameName, string profileName, out RunSnapshot snapshot);
    void Invalidate(string gameName, string profileName);
    void InvalidateStale(string gameName, IEnumerable<string> validProfileNames);
    void RenameGame(string oldName, string newName);
}
