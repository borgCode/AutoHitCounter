// 

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

    public int Diff => NumOfHits - PersonalBest;
}