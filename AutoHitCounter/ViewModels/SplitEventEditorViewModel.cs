using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using AutoHitCounter.Core;
using AutoHitCounter.Models;

namespace AutoHitCounter.ViewModels;

public class SplitEventEditorViewModel : BaseViewModel
{
    private readonly Dictionary<uint, string> _allEvents;

    public SplitEventEditorViewModel(SplitEntry split, Dictionary<uint, string> allEvents)
    {
        _allEvents = allEvents;
        SplitName = split.Label;


        ConfirmCommand = new DelegateCommand(Confirm, () => IsValidEventId);
        ClearCommand = new DelegateCommand(Clear);


        EventIdText = split.EventId?.ToString() ?? string.Empty;

        foreach (var kvp in allEvents)
            AllEvents.Add(new SplitEntry { EventId = kvp.Key, Name = kvp.Value });

        FilteredEvents = new ObservableCollection<SplitEntry>(AllEvents);
    }

    public string SplitName { get; }
    public ObservableCollection<SplitEntry> AllEvents { get; } = new();
    public ObservableCollection<SplitEntry> FilteredEvents { get; }
    public uint? ResultEventId { get; private set; }
    public bool Confirmed { get; private set; }

    public DelegateCommand ConfirmCommand { get; }
    public DelegateCommand ClearCommand { get; }

    private string _eventIdText;

    public string EventIdText
    {
        get => _eventIdText;
        set
        {
            if (SetProperty(ref _eventIdText, value))
            {
                OnPropertyChanged(nameof(IsValidEventId));
                ConfirmCommand.RaiseCanExecuteChanged();
            }
        }
    }

    private SplitEntry _selectedEvent;

    public SplitEntry SelectedEvent
    {
        get => _selectedEvent;
        set
        {
            if (SetProperty(ref _selectedEvent, value) && value != null)
                EventIdText = value.EventId?.ToString() ?? string.Empty;
        }
    }

    private string _searchText;

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
                FilterEvents();
        }
    }

    public bool IsValidEventId => string.IsNullOrEmpty(EventIdText) ||
                                  uint.TryParse(EventIdText, out _);

    private void Confirm()
    {
        if (!string.IsNullOrEmpty(EventIdText) &&
            uint.TryParse(EventIdText, out var id))
            ResultEventId = id;
        else
            ResultEventId = null;

        Confirmed = true;
        RequestClose?.Invoke();
    }

    private void Clear()
    {
        EventIdText = string.Empty;
        SelectedEvent = null;
        ResultEventId = null;
        Confirmed = true;
        RequestClose?.Invoke();
    }


    // fuzzy search
    private static readonly Dictionary<string, string[]> Aliases = new(StringComparer.OrdinalIgnoreCase)
    {
        // Elden Ring
        { "godfrey", ["Godfrey, the First Elden Lord (Golden Shade)", "Hoarah Loux, Warrior"] },
        { "hoarah loux", ["Godfrey, the First Elden Lord (Golden Shade)", "Hoarah Loux, Warrior"] },
        { "bofa", ["Beastman of Farum Azula"] },
        { "moose", ["Regal Ancestor Spirit", "Ancestor Spirit"] },
        { "bbh", ["Bell Bearing Hunter (Warmaster's Shack)", "Bell Bearing Hunter (Church of Vows)",
                "Bell Bearing Hunter (Hermit Merchant's Shack)", "Bell Bearing Hunter (Isolated Merchant's Shack)"] },
        { "bbk", ["Black Blade Kindred (Greyoll's Dragonbarrow)", "Black Blade Kindred (Forbidden Lands)"] },
        { "death bird", ["Deathbird (Stormhill)", "Deathbird (Weeping Peninsula)", "Deathbird (Liurnia of the Lakes)",
                "Deathbird (Capital Outskirts)", "Death Rite Bird (Academy Gate Town)", "Death Rite Bird (Caelid)",
                "Death Rite Bird (Mountaintops of the Giants)", "Death Rite Bird (Consecrated Snowfield)", "Death Rite Bird (Charo's Hidden Grave)"] },
        { "dragon", [ "Ancient Dragon Lansseax", "Borealis the Freezing Fog", "Decaying Ekzykes", "Dragonkin Soldier (Lake of Rot)", "Dragonkin Soldier (Siofra River)",
                "Dragonkin Soldier of Nokstella", "Dragonlord Placidusax", "Flying Dragon Agheel", "Flying Dragon Greyll", "Glintstone Dragon Adula",
                "Glintstone Dragon Smarag", "Lichdragon Fortissax", "Ancient Dragon Senessax", "Ghostflame Dragon (Gravesite Plain)", "Ghostflame Dragon (Scadu Altus)",
                "Ghostflame Dragon (Cerulean Coast)", "Jagged Peak Drake", "Jagged Peak Drake (Duo Encounter)", "Bayle the Dread"] },
        { "ancient dragon man", ["Ancient Dragon-Man",] },
        
        // Sekiro
        { "corrupted monk", ["Fake Monk", "True Monk", "Fake Monk (Gauntlet)", "True Monk (Gauntlet)",] },
        { "SSI", ["Isshin, the Sword Saint / Isshin, the Sword Saint (Gauntlet) / Inner Isshin",] },
        { "roberto", ["Armored Warrior",] },
        { "orin", ["O'Rin of the Water",] },
        { "hape", ["Headless Ape", "Headless Ape (Gauntlet)",] },
        { "gape", ["Guardian Ape", "Guardian Ape (Gauntlet)",] },
    };

    private void FilterEvents()
    {
        FilteredEvents.Clear();

        HashSet<string> aliasMatches = [];
        if (!string.IsNullOrEmpty(_searchText))
        {
            foreach (var kvp in Aliases)
            {
                if (kvp.Key.IndexOf(_searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    foreach (var name in kvp.Value)
                        aliasMatches.Add(name);
                }
            }
        }

        foreach (var e in AllEvents)
        {
            var matchesDirect = string.IsNullOrEmpty(_searchText) ||
                                e.Name.IndexOf(_searchText, StringComparison.OrdinalIgnoreCase) >= 0;

            var matchesAlias = aliasMatches.Contains(e.Name);

            if (matchesDirect || matchesAlias)
                FilteredEvents.Add(e);
        }
    }

    public System.Action RequestClose { get; set; }
}