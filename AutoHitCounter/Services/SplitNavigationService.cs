// 

using System;
using System.Collections.ObjectModel;
using System.Linq;
using AutoHitCounter.Enums;
using AutoHitCounter.Interfaces;
using AutoHitCounter.ViewModels;

namespace AutoHitCounter.Services;

public class SplitNavigationService : ISplitNavigationService
{
    private ObservableCollection<SplitViewModel> _splits;
    
    public SplitViewModel CurrentSplit { get; private set; }
    public bool IsRunComplete { get; private set; }
    
    public event Action StateChanged;
    
    public void Load(ObservableCollection<SplitViewModel> splits) => _splits = splits;
    
    public void SetPosition(SplitViewModel split, bool isRunComplete)
    {
        if (CurrentSplit != null) CurrentSplit.IsCurrent = false;
        IsRunComplete = isRunComplete;
        CurrentSplit = split;
        if (CurrentSplit != null) CurrentSplit.IsCurrent = true;
    }
    
    public void  InitFresh()
    {
        if (CurrentSplit != null) CurrentSplit.IsCurrent = false;
        IsRunComplete = false;
        CurrentSplit = _splits?.FirstOrDefault(s => s.Type == SplitType.Child);
        if (CurrentSplit != null) CurrentSplit.IsCurrent = true;
    }
    
    public void Advance()
    {
        if (IsRunComplete || CurrentSplit == null) return;
        AdvanceInternal();
        StateChanged?.Invoke();
    }
    
    public void Previous()
    {
        if (CurrentSplit == null) return;
        if (!PreviousInternal()) return;
        StateChanged?.Invoke();
    }
    
    public void JumpTo(SplitViewModel target)
    {
        if (target == null || target.IsParent || _splits == null) return;
 
        var targetIndex = _splits.IndexOf(target);
        var currentIndex = CurrentSplit != null ? _splits.IndexOf(CurrentSplit) : -1;
        if (targetIndex < 0 || targetIndex == currentIndex) return;
 
        if (targetIndex > currentIndex)
        {
            while (CurrentSplit != target && !IsRunComplete)
            {
                if (!AdvanceInternal()) break;
            }
        }
        else
        {
            while (CurrentSplit != target)
            {
                if (!PreviousInternal()) break;
            }
        }
 
        StateChanged?.Invoke();
    }
    
    private bool AdvanceInternal()
    {
        if (IsRunComplete || CurrentSplit == null) return false;

        var currentIndex = _splits.IndexOf(CurrentSplit);
        if (currentIndex < 0) return false;

        var next = _splits.Skip(currentIndex + 1)
            .FirstOrDefault(s => s.Type == SplitType.Child);

        if (next == null)
        {
            CurrentSplit.IsCurrent = false;
            IsRunComplete = true;
            return true; // state changed (run completed)
        }

        CurrentSplit.IsCurrent = false;
        next.IsCurrent = true;
        CurrentSplit = next;
        return true;
    }
 
    private bool PreviousInternal()
    {
        if (CurrentSplit == null) return false;
 
        var currentIndex = _splits.IndexOf(CurrentSplit);
        if (currentIndex < 0) return false;
 
        var prev = _splits.Take(currentIndex)
            .LastOrDefault(s => s.Type == SplitType.Child);
        if (prev == null) return false;
 
        CurrentSplit.IsCurrent = false;
        prev.IsCurrent = true;
        CurrentSplit = prev;
        IsRunComplete = false;
        return true;
    }
}