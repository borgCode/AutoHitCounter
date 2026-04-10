//

using System.Windows;
using System.Windows.Media;
using AutoHitCounter.Enums;
using AutoHitCounter.Services;

namespace AutoHitCounter.ViewModels;

public class SplitViewModel : BaseViewModel
{
    public SplitViewModel()
    {
        ThemeService.ThemeChanged += OnThemeChanged;
    }

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

    private bool _isDistancePb;

    public bool IsDistancePb
    {
        get => _isDistancePb;
        set => SetProperty(ref _isDistancePb, value);
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
        ? GetBrush("HitsActiveBrush")
        : GetBrush("HitsInactiveBrush");

    public Brush DiffBrush
    {
        get
        {
            if (Diff > 0) return GetBrush("DiffPositiveBrush");
            if (Diff < 0) return GetBrush("DiffNegativeBrush");
            return GetBrush("DiffNeutralBrush");
        }
    }

    private static Brush GetBrush(string key)
    {
        if (Application.Current.Resources[key] is SolidColorBrush brush)
            return brush;
        return new SolidColorBrush(Colors.White);
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

    private void OnThemeChanged()
    {
        OnPropertyChanged(nameof(HitsBrush));
        OnPropertyChanged(nameof(DiffBrush));
    }

    public override void Dispose()
    {
        ThemeService.ThemeChanged -= OnThemeChanged;
    }
}