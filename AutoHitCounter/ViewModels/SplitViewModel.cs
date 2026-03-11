//

using System.Windows.Media;
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
            {
                OnPropertyChanged(nameof(Diff));
                OnPropertyChanged(nameof(HitsBrush));
                OnPropertyChanged(nameof(DiffBrush));
            }
        }
    }

    private int _personalBest;

    public int PersonalBest
    {
        get => _personalBest;
        set
        {
            if (SetProperty(ref _personalBest, value))
                OnPropertyChanged(nameof(Diff));
            OnPropertyChanged(nameof(DiffBrush));
        }
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

    public bool IsParent => Type == SplitType.Parent;
    public int Diff => NumOfHits - PersonalBest;

    public Brush HitsBrush => NumOfHits > 0
        ? new SolidColorBrush(Color.FromRgb(0xc8, 0x84, 0x3a))
        : new SolidColorBrush(Color.FromRgb(0x99, 0x99, 0x99));

    public Brush DiffBrush
    {
        get
        {
            if (Diff > 0) return new SolidColorBrush(Color.FromRgb(0xb8, 0x55, 0x55));
            if (Diff < 0) return new SolidColorBrush(Color.FromRgb(0x5a, 0x90, 0x68));
            return new SolidColorBrush(Color.FromRgb(0xAA, 0xAA, 0xAA));
        }
    }

    private bool _isEditing;

    public bool IsEditing
    {
        get => _isEditing;
        set => SetProperty(ref _isEditing, value);
    }

    public bool IsEditingPb
    {
        get => _isEditingPb;
        set
        {
            _isEditingPb = value;
            OnPropertyChanged();
        }
    }

    private bool _isEditingPb;

    public void RefreshLayout()
    {
        OnPropertyChanged(nameof(IsEditingPb));
        OnPropertyChanged(nameof(PersonalBest));
    }
}