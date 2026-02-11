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
    
    public int Diff => NumOfHits - PersonalBest;
    
}