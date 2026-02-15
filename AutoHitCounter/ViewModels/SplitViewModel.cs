// 

using AutoHitCounter.Enums;

namespace AutoHitCounter.ViewModels;

public class SplitViewModel : BaseViewModel
{
    private string _name;

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    private int _numOfHits;

    public int NumOfHits
    {
        get => _numOfHits;
        set
        {
            if (SetProperty(ref _numOfHits, value))
                OnPropertyChanged(nameof(Diff));
        }
    }

    private int _personalBest;

    public int PersonalBest
    {
        get => _personalBest;
        set => SetProperty(ref _personalBest, value);
    }

    private bool _isCurrent;

    public bool IsCurrent
    {
        get => _isCurrent;
        set => SetProperty(ref _isCurrent, value);
    }

    private bool _isAuto;

    public bool IsAuto
    {
        get => _isAuto;
        set => SetProperty(ref _isAuto, value);
    }
    
    private SplitType _type = SplitType.Child;

    public SplitType Type
    {
        get => _type;
        set
        {
            if (SetProperty(ref _type, value))
                OnPropertyChanged(nameof(IsParent));
        }
    }

    private string _groupId;

    public string GroupId
    {
        get => _groupId;
        set => SetProperty(ref _groupId, value);
    }

    private bool _isExpanded;

    public bool IsExpanded
    {
        get => _isExpanded;
        set => SetProperty(ref _isExpanded, value);
    }
    
    private string _notes;

    public string Notes
    {
        get => _notes;
        set => SetProperty(ref _notes, value);
    }

    private bool _isEditingNotes;

    public bool IsEditingNotes
    {
        get => _isEditingNotes;
        set => SetProperty(ref _isEditingNotes, value);
    }

    public bool IsParent => Type == SplitType.Parent;
    public int Diff => NumOfHits - PersonalBest;
}