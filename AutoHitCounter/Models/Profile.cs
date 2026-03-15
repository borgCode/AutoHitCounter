// 

using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AutoHitCounter.Models;

public class Profile : INotifyPropertyChanged
{
    private string _name;
    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            OnPropertyChanged();
        }
    }

    public string GameName { get; set; }
    public List<SplitEntry> Splits { get; set; }
    public Dictionary<string, bool> GameSettings { get; set; } = new();

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    
    public int AttemptCount { get; set; }
    public int DistancePb { get; set; } = -1;
    public RunState SavedRun { get; set; }
}