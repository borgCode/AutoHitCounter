//

using System;
using System.Collections.ObjectModel;
using AutoHitCounter.ViewModels;

namespace AutoHitCounter.Interfaces;

public interface ISplitNavigationService
{
    SplitViewModel CurrentSplit { get; }
    bool IsRunComplete { get; }
    event Action StateChanged;
    void Load(ObservableCollection<SplitViewModel> splits);
    void SetPosition(SplitViewModel split, bool isRunComplete);
    void InitFresh();
    void Advance();
    void Previous();
    void JumpTo(SplitViewModel target);
}
