// 

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AutoHitCounter.Core;
using AutoHitCounter.Enums;
using AutoHitCounter.Interfaces;
using AutoHitCounter.Models;
using AutoHitCounter.Services;
using AutoHitCounter.Utilities;
using AutoHitCounter.Views.Controls;

namespace AutoHitCounter.ViewModels;

public class ProfileEditorViewModel : BaseViewModel, IReorderHandler
{
    private readonly IProfileService _profileService;
    private readonly Dictionary<uint, string> _allEvents;
    private readonly string _gameName;
    private readonly GameTitle _gameTitle;

    public bool IsManualGame { get; }

    public ProfileEditorViewModel(
        Dictionary<uint, string> allEvents,
        IProfileService profileService,
        string gameName,
        GameTitle gameTitle,
        Profile activeProfile,
        bool isManualGame = false)
    {
        IsManualGame = isManualGame;
        _allEvents = allEvents;
        _profileService = profileService;
        _gameName = gameName;
        _gameTitle = gameTitle;

        Profiles = new ObservableCollection<Profile>(profileService.GetProfiles(gameName));

        foreach (var kvp in allEvents)
            AllEvents.Add(new SplitEntry { EventId = kvp.Key, Name = kvp.Value });

        AddCommand =
            new DelegateCommand<SplitEntry>(Add, e => AllowDuplicates || Splits.All(s => s.EventId != e.EventId));
        RemoveCommand = new DelegateCommand<SplitEntry>(Remove);
        MoveUpCommand = new DelegateCommand<SplitEntry>(MoveUp, e => CanMoveUp());
        MoveDownCommand = new DelegateCommand<SplitEntry>(MoveDown, e => CanMoveDown());
        AddManualSplitCommand = new DelegateCommand(AddManualSplit);
        AddGroupCommand = new DelegateCommand(AddGroup);
        RenameSplitCommand = new DelegateCommand<SplitEntry>(RenameSplit);
        NewProfileCommand = new DelegateCommand(NewProfile);
        RenameProfileCommand = new DelegateCommand(RenameProfile, () => SelectedProfile != null);
        SaveCommand = new DelegateCommand(Save, () => SelectedProfile != null);
        DeleteCommand = new DelegateCommand(Delete, () => SelectedProfile != null);
        AddSelectedCommand = new DelegateCommand(AddSelected, () => SelectedTemplates.Any());
        RemoveSelectedCommand = new DelegateCommand(RemoveSelected, () => SelectedSplits.Any());

        ClearAutoSplitEventsCommand = new DelegateCommand(ClearEventIds, () => SelectedProfile != null);

        Splits.CollectionChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(SplitCount));
            OnPropertyChanged(nameof(HasGroups));
            MoveUpCommand.RaiseCanExecuteChange();
            MoveDownCommand.RaiseCanExecuteChange();
        };

        SelectedTemplates.CollectionChanged += (_, _) => AddSelectedCommand.RaiseCanExecuteChanged();
        SelectedSplits.CollectionChanged += (_, _) => RemoveSelectedCommand.RaiseCanExecuteChanged();

        if (activeProfile != null)
        {
            SelectedProfile = Profiles.FirstOrDefault(p => p.Name == activeProfile.Name);
        }

        EditSplitEventCommand = new DelegateCommand(EditSplitEvent,
            () => SelectedSplit?.Type == SplitType.Child);

        _hideAdded = SettingsManager.Default.HideAdded;
        _allowDuplicates = SettingsManager.Default.AllowDuplicates;
        FilterEvents();
    }

    #region Commands

    public DelegateCommand<SplitEntry> AddCommand { get; }
    public DelegateCommand<SplitEntry> RemoveCommand { get; }
    public DelegateCommand<SplitEntry> MoveUpCommand { get; }
    public DelegateCommand<SplitEntry> MoveDownCommand { get; }
    public DelegateCommand AddManualSplitCommand { get; }
    public DelegateCommand AddGroupCommand { get; }
    public DelegateCommand<SplitEntry> RenameSplitCommand { get; }
    public DelegateCommand NewProfileCommand { get; }
    public DelegateCommand RenameProfileCommand { get; }
    public DelegateCommand SaveCommand { get; }
    public DelegateCommand DeleteCommand { get; }
    public DelegateCommand EditSplitEventCommand { get; private set; }

    public DelegateCommand AddSelectedCommand { get; private set; }
    public DelegateCommand RemoveSelectedCommand { get; private set; }

    public DelegateCommand ClearAutoSplitEventsCommand { get; private set; }

    #endregion

    #region Properties

    public ObservableCollection<SplitEntry> AllEvents { get; } = new();
    public ObservableCollection<SplitEntry> Splits { get; } = new();
    public ObservableCollection<Profile> Profiles { get; }
    public ObservableCollection<SplitEntry> SelectedTemplates { get; } = new();
    public ObservableCollection<SplitEntry> SelectedSplits { get; } = new();

    private Profile _selectedProfile;

    public Profile SelectedProfile
    {
        get => _selectedProfile;
        set
        {
            if (SetProperty(ref _selectedProfile, value))
                LoadProfile(value);
            SaveCommand.RaiseCanExecuteChanged();
            DeleteCommand.RaiseCanExecuteChanged();
            RenameProfileCommand.RaiseCanExecuteChanged();
        }
    }

    private SplitEntry _selectedSplit;

    public SplitEntry SelectedSplit
    {
        get => _selectedSplit;
        set
        {
            if (SetProperty(ref _selectedSplit, value))
            {
                MoveUpCommand.RaiseCanExecuteChange();
                MoveDownCommand.RaiseCanExecuteChange();
                EditSplitEventCommand.RaiseCanExecuteChanged();
            }
        }
    }

    private string _searchText = string.Empty;

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
                FilterEvents();
        }
    }

    public string GameName => _gameName;

    public void ImportProfile(Profile profile)
    {
        _profileService.SaveProfile(profile);
        var existing = Profiles.FirstOrDefault(p => p.Name == profile.Name);
        if (existing != null)
            Profiles[Profiles.IndexOf(existing)] = profile;
        else
            Profiles.Add(profile);
    }
    
    public void NotifySaved() => OnSaved?.Invoke();


    public ObservableCollection<SplitEntry> FilteredEvents { get; } = new();

    private bool _isDirty;

    public bool IsDirty
    {
        get => _isDirty;
        private set => SetProperty(ref _isDirty, value);
    }

    public ObservableCollection<GameFlagViewModel> GameFlags { get; } = new();

    public bool HasGameFlags => GameFlags.Count > 0;

    public bool HasGroups => Splits.Any(s => s.Type == SplitType.Parent);

    public int SplitCount => Splits.Count(s => s.Type == SplitType.Child);

    #endregion

    #region Public Methods

    public void MoveItem(object draggedItem, int dropIndex)
    {
        if (draggedItem is SplitEntry entry)
            MoveSplit(entry, dropIndex);
    }


    public void MoveSplit(SplitEntry draggedEntry, int dropIndex)
    {
        if (draggedEntry == null || dropIndex < 0) return;

        var oldIndex = Splits.IndexOf(draggedEntry);
        if (oldIndex < 0 || oldIndex == dropIndex) return;

        Splits.RemoveAt(oldIndex);

        if (dropIndex > oldIndex)
            dropIndex--;

        if (dropIndex > Splits.Count)
            dropIndex = Splits.Count;

        Splits.Insert(dropIndex, draggedEntry);

        AssignGroupFromPosition(draggedEntry, dropIndex);
        IsDirty = true;
    }

    public void DropFromTemplates(IList<SplitEntry> items, int dropIndex)
    {
        foreach (var item in items)
        {
            if (Splits.Any(s => s.EventId == item.EventId)) continue;

            var newEntry = new SplitEntry
            {
                EventId = item.EventId,
                Name = item.Name,
                Type = SplitType.Child
            };

            if (dropIndex >= Splits.Count)
                Splits.Add(newEntry);
            else
                Splits.Insert(dropIndex, newEntry);

            dropIndex++;
        }

        FilterEvents();
        IsDirty = true;
    }

    public void RebuildGroupAssignments()
    {
        string currentGroupId = null;

        foreach (var split in Splits)
        {
            if (split.Type == SplitType.Parent)
            {
                currentGroupId = split.GroupId;
            }
            else
            {
                split.GroupId = currentGroupId;
            }
        }
    }

    #endregion

    #region Private Methods

    private void LoadProfile(Profile profile)
    {
        Splits.Clear();
        RebuildGameFlags(profile);
        if (profile == null) return;

        foreach (var split in profile.Splits)
            Splits.Add(new SplitEntry
            {
                EventId = split.EventId,
                Name = split.Name,
                DisplayName = split.DisplayName,
                PersonalBest = split.PersonalBest,
                Type = split.Type,
                GroupId = split.GroupId,
                Notes = split.Notes
            });

        FilterEvents();
        IsDirty = false;
    }

    private void RebuildGameFlags(Profile profile)
    {
        GameFlags.Clear();
        if (profile == null)
        {
            OnPropertyChanged(nameof(HasGameFlags));
            return;
        }

        foreach (var (key, displayName) in GameFlagRegistry.GetFlags(_gameTitle))
        {
            profile.GameSettings.TryGetValue(key, out var current);
            GameFlags.Add(new GameFlagViewModel(key, displayName, current, (k, v) =>
            {
                profile.GameSettings[k] = v;
                IsDirty = true;
            }));
        }

        OnPropertyChanged(nameof(HasGameFlags));
    }

    private void Add(SplitEntry entry)
    {
        var newEntry = new SplitEntry
        {
            EventId = entry.EventId,
            Name = entry.Name,
            Type = SplitType.Child
        };

        InsertSplit(newEntry);
        FilterEvents();
        IsDirty = true;
    }

    private void AddManualSplit()
    {
        var name = MsgBox.ShowInput("Split Name", "", "New Manual Split");
        if (name == null) return;
        if (string.IsNullOrWhiteSpace(name))
        {
            MsgBox.Show("Split name required", "New Manual Split");
            return;
        }

        InsertSplit(new SplitEntry
        {
            EventId = null,
            Name = name,
            Type = SplitType.Child
        });

        FilterEvents();
        IsDirty = true;
    }

    private void InsertSplit(SplitEntry newEntry)
    {
        if (SelectedSplit?.Type == SplitType.Parent)
        {
            var parentIndex = Splits.IndexOf(SelectedSplit);
            var insertIndex = FindLastChildIndex(parentIndex) + 1;
            newEntry.GroupId = SelectedSplit.GroupId;
            Splits.Insert(insertIndex, newEntry);
        }
        else if (SelectedSplit != null && !string.IsNullOrEmpty(SelectedSplit.GroupId))
        {
            var selectedIndex = Splits.IndexOf(SelectedSplit);
            newEntry.GroupId = SelectedSplit.GroupId;
            Splits.Insert(selectedIndex + 1, newEntry);
        }
        else
        {
            Splits.Add(newEntry);
        }
    }

    private void AddGroup()
    {
        var name = MsgBox.ShowInput("Group Name", "", "New Group");

        if (string.IsNullOrWhiteSpace(name))
        {
            MsgBox.Show("Group name required", "New Group");
            return;
        }

        var groupId = Guid.NewGuid().ToString();

        var parent = new SplitEntry
        {
            Name = name,
            Type = SplitType.Parent,
            GroupId = groupId
        };

        if (SelectedSplit != null)
        {
            var index = Splits.IndexOf(SelectedSplit);

            if (SelectedSplit.Type == SplitType.Parent)
                index = FindLastChildIndex(index);

            Splits.Insert(index + 1, parent);
        }
        else
        {
            Splits.Add(parent);
        }

        IsDirty = true;
    }

    private void AddSelected()
    {
        foreach (var item in SelectedTemplates.ToList())
        {
            if (!AllowDuplicates && Splits.Any(s => s.EventId == item.EventId))
                continue;
            Add(item);
        }
    }

    private void RemoveSelected()
    {
        foreach (var item in SelectedSplits.ToList())
            Remove(item);
    }

    private void RenameSplit(SplitEntry entry)
    {
        entry ??= SelectedSplit;
        if (entry == null) return;

        var newName = MsgBox.ShowInput("Rename Split", entry.Label, "Rename");
        if (string.IsNullOrWhiteSpace(newName)) return;

        entry.DisplayName = newName;
        entry.Name = newName;

        IsDirty = true;
    }

    private void Remove(SplitEntry entry)
    {
        if (entry.Type == SplitType.Parent)
        {
            foreach (var child in Splits.Where(s => s.GroupId == entry.GroupId && s.Type == SplitType.Child))
                child.GroupId = null;

            Splits.Remove(entry);
        }
        else
        {
            Splits.Remove(entry);
        }

        FilterEvents();
        IsDirty = true;
    }

    private void MoveUp(SplitEntry entry)
    {
        if (entry == null)
            return;

        var index = Splits.IndexOf(entry);
        if (index <= 0) return;

        if (entry.Type == SplitType.Parent)
        {
            var lastChild = FindLastChildIndex(index);
            var blockSize = lastChild - index + 1;

            var targetIndex = index - 1;
            if (Splits[targetIndex].Type == SplitType.Child)
            {
                for (int i = targetIndex - 1; i >= 0; i--)
                {
                    if (Splits[i].Type == SplitType.Parent)
                    {
                        targetIndex = i;
                        break;
                    }
                }
            }

            var block = Splits.Skip(index).Take(blockSize).ToList();
            for (int i = blockSize - 1; i >= 0; i--)
                Splits.RemoveAt(index + i);
            for (int i = 0; i < block.Count; i++)
                Splits.Insert(targetIndex + i, block[i]);

            IsDirty = true;
        }
        else
        {
            if (Splits[index - 1].Type == SplitType.Parent &&
                Splits[index - 1].GroupId == entry.GroupId)
                return;

            Splits.Move(index, index - 1);
            AssignGroupFromPosition(entry, index - 1);
            IsDirty = true;
        }
    }

    private bool CanMoveUp()
    {
        return SelectedSplit != null &&
               Splits.IndexOf(SelectedSplit) > 0;
    }

    private void MoveDown(SplitEntry entry)
    {
        if (entry == null)
            return;

        var index = Splits.IndexOf(entry);
        if (index >= Splits.Count - 1) return;

        if (entry.Type == SplitType.Parent)
        {
            var lastChild = FindLastChildIndex(index);
            if (lastChild >= Splits.Count - 1) return;

            var blockSize = lastChild - index + 1;
            var targetIndex = lastChild + 1;

            if (Splits[targetIndex].Type == SplitType.Parent)
                targetIndex = FindLastChildIndex(targetIndex) + 1;
            else
                targetIndex++;

            var block = Splits.Skip(index).Take(blockSize).ToList();
            for (int i = blockSize - 1; i >= 0; i--)
                Splits.RemoveAt(index + i);

            var insertAt = targetIndex - blockSize;
            for (int i = 0; i < block.Count; i++)
                Splits.Insert(insertAt + i, block[i]);

            IsDirty = true;
        }
        else
        {
            if (index + 1 < Splits.Count &&
                Splits[index + 1].Type == SplitType.Parent)
                return;

            Splits.Move(index, index + 1);
            AssignGroupFromPosition(entry, index + 1);
            IsDirty = true;
        }
    }

    private bool CanMoveDown()
    {
        return SelectedSplit != null &&
               Splits.IndexOf(SelectedSplit) < Splits.Count - 1;
    }

    public void RefreshSplits()
    {
        var selectedName = SelectedSplit?.Name;
        Splits.Clear();
        if (_selectedProfile == null) return;
        foreach (var split in _selectedProfile.Splits)
            Splits.Add(new SplitEntry
            {
                EventId = split.EventId,
                Name = split.Name,
                DisplayName = split.DisplayName,
                PersonalBest = split.PersonalBest,
                Type = split.Type,
                GroupId = split.GroupId,
                Notes = split.Notes
            });
        if (selectedName != null)
            SelectedSplit = Splits.FirstOrDefault(s => s.Name == selectedName);

        FilterEvents();
    }

    private int FindLastChildIndex(int parentIndex)
    {
        var lastIndex = parentIndex;

        for (int i = parentIndex + 1; i < Splits.Count; i++)
        {
            if (Splits[i].Type == SplitType.Parent)
                break;

            lastIndex = i;
        }

        return lastIndex;
    }

    private bool _hideAdded;

    public bool HideAdded
    {
        get => _hideAdded;
        set
        {
            if (SetProperty(ref _hideAdded, value))
                FilterEvents();
            SettingsManager.Default.HideAdded = value;
            SettingsManager.Default.Save();
        }
    }

    private bool _allowDuplicates;

    public bool AllowDuplicates
    {
        get => _allowDuplicates;
        set
        {
            if (SetProperty(ref _allowDuplicates, value))
            {
                AddCommand.RaiseCanExecuteChange();
                FilterEvents();
                SettingsManager.Default.AllowDuplicates = value;
                SettingsManager.Default.Save();
            }
        }
    }


    private void EditSplitEvent()
    {
        if (SelectedSplit == null || SelectedSplit.Type != SplitType.Child) return;

        var vm = new SplitEventEditorViewModel(SelectedSplit, _allEvents);
        var window = new SplitEventEditorWindow();
        window.SetViewModel(vm);
        window.ShowDialog();

        if (!vm.Confirmed) return;

        SelectedSplit.EventId = vm.ResultEventId;
        IsDirty = true;
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

        foreach (var entry in AllEvents)
        {
            var matchesDirect = string.IsNullOrEmpty(_searchText) ||
                                entry.Name.IndexOf(_searchText, StringComparison.OrdinalIgnoreCase) >= 0;

            var matchesAlias = aliasMatches.Contains(entry.Name);

            if (!matchesDirect && !matchesAlias) continue;

            if (HideAdded && !AllowDuplicates && Splits.Any(s => s.EventId == entry.EventId))
                continue;

            FilteredEvents.Add(entry);
        }

        AddCommand.RaiseCanExecuteChange();
    }

    private void NewProfile()
    {
        List<SplitEntry> splitsToKeep = null;

        if (Splits.Any())
        {
            var keepSplits = MsgBox.ShowYesNoCancel(
                $"Current splits list is not empty.{Environment.NewLine}Would you like to add them to the new profile?",
                "New Profile");


            if (keepSplits == null) return;
            if (keepSplits == true) splitsToKeep = Splits.ToList();
        }

        var name = MsgBox.ShowInput("Profile Name", "", "New Profile");
        if (string.IsNullOrWhiteSpace(name)) return;

        var profile = new Profile
        {
            Name = name,
            GameName = _gameName,
            Splits = splitsToKeep ?? []
        };

        _profileService.SaveProfile(profile);
        Profiles.Add(profile);
        SelectedProfile = profile;
        OnSaved?.Invoke();
    }

    private void RenameProfile()
    {
        if (_selectedProfile == null) return;

        var newName = MsgBox.ShowInput("Profile Name", _selectedProfile.Name, "Rename Profile");
        if (string.IsNullOrWhiteSpace(newName)) return;
        if (newName == _selectedProfile.Name) return;

        _profileService.DeleteProfile(_gameName, _selectedProfile.Name);
        _selectedProfile.Name = newName;
        _profileService.SaveProfile(_selectedProfile);
        OnSaved?.Invoke();

        IsDirty = false;
    }

    public event Action OnSaved;

    private void Save()
    {
        if (_selectedProfile == null) return;

        RebuildGroupAssignments();

        _selectedProfile.Splits = Splits.ToList();
        _profileService.SaveProfile(_selectedProfile);
        IsDirty = false;
        OnSaved?.Invoke();
    }

    private void Delete()
    {
        if (_selectedProfile == null) return;

        _profileService.DeleteProfile(_gameName, _selectedProfile.Name);
        Profiles.Remove(_selectedProfile);
        SelectedProfile = Profiles.FirstOrDefault();
        OnSaved?.Invoke();
    }

    private void AssignGroupFromPosition(SplitEntry entry, int index)
    {
        if (entry.Type == SplitType.Parent) return;

        string groupId = null;
        for (int i = index - 1; i >= 0; i--)
        {
            if (Splits[i].Type == SplitType.Parent)
            {
                groupId = Splits[i].GroupId;
                break;
            }
        }

        entry.GroupId = groupId;
    }

    private void ClearEventIds()
    {
        bool shouldClearEvents = MsgBox.ShowYesNo(
            "Are you sure you want to clear all the autosplit events for this profile?",
            "Clear Events"
        );

        if (shouldClearEvents)
        {
            foreach (var split in Splits)
                split.EventId = null;
            IsDirty = true;
        }
    }

    #endregion
}