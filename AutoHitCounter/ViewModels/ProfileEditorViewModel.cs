// 

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AutoHitCounter.Core;
using AutoHitCounter.Enums;
using AutoHitCounter.Interfaces;
using AutoHitCounter.Models;
using AutoHitCounter.Utilities;

namespace AutoHitCounter.ViewModels;

public class ProfileEditorViewModel : BaseViewModel
{
    private readonly IProfileService _profileService;
    private readonly Dictionary<uint, string> _allEvents;
    private readonly string _gameName;

    public ProfileEditorViewModel(
        Dictionary<uint, string> allEvents,
        IProfileService profileService,
        string gameName,
        Profile activeProfile)
    {
        _allEvents = allEvents;
        _profileService = profileService;
        _gameName = gameName;

        Profiles = new ObservableCollection<Profile>(profileService.GetProfiles(gameName));

        foreach (var kvp in allEvents)
            AllEvents.Add(new SplitEntry { EventId = kvp.Key, Name = kvp.Value });

        AddCommand = new DelegateCommand<SplitEntry>(Add, e => Splits.All(s => s.EventId != e.EventId));
        RemoveCommand = new DelegateCommand<SplitEntry>(Remove);
        MoveUpCommand = new DelegateCommand<SplitEntry>(MoveUp, e => Splits.IndexOf(e) > 0);
        MoveDownCommand = new DelegateCommand<SplitEntry>(MoveDown, e => Splits.IndexOf(e) < Splits.Count - 1);
        AddManualSplitCommand = new DelegateCommand(AddManualSplit);
        AddGroupCommand = new DelegateCommand(AddGroup);
        RenameSplitCommand = new DelegateCommand<SplitEntry>(RenameSplit);
        NewProfileCommand = new DelegateCommand(NewProfile);
        SaveCommand = new DelegateCommand(Save, () => SelectedProfile != null);
        DeleteCommand = new DelegateCommand(Delete, () => SelectedProfile != null);


        if (activeProfile != null)
        {
            SelectedProfile = Profiles.FirstOrDefault(p => p.Name == activeProfile.Name);
        }
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
    public DelegateCommand SaveCommand { get; }
    public DelegateCommand DeleteCommand { get; }

    #endregion

    #region Properties

    public ObservableCollection<SplitEntry> AllEvents { get; } = new();
    public ObservableCollection<SplitEntry> Splits { get; } = new();
    public ObservableCollection<Profile> Profiles { get; }

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
        }
    }

    private SplitEntry _selectedSplit;

    public SplitEntry SelectedSplit
    {
        get => _selectedSplit;
        set => SetProperty(ref _selectedSplit, value);
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

    public ObservableCollection<SplitEntry> FilteredEvents { get; } = new();

    #endregion

    #region Public Methods

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
        if (profile == null) return;

        foreach (var split in profile.Splits)
            Splits.Add(split);

        FilterEvents();
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
    }

    private void AddManualSplit()
    {
        var name = MsgBox.ShowInput("Split Name", "", "New Manual Split");
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
    }

    private void RenameSplit(SplitEntry entry)
    {
        if (entry == null) return;

        var newName = MsgBox.ShowInput("Rename Split", entry.Label, "Rename");
        if (string.IsNullOrWhiteSpace(newName)) return;

        if (entry.IsAuto)
        {
            entry.DisplayName = newName;
        }
        else
        {
            entry.Name = newName;
        }
    }

    private void Remove(SplitEntry entry)
    {
        if (entry.Type == SplitType.Parent)
        {
            var toRemove = Splits
                .Where(s => s.GroupId == entry.GroupId)
                .ToList();

            foreach (var s in toRemove)
                Splits.Remove(s);
        }
        else
        {
            Splits.Remove(entry);
        }

        FilterEvents();
    }
    private void MoveUp(SplitEntry entry)
    {
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
        }
        else
        {
            if (Splits[index - 1].Type == SplitType.Parent &&
                Splits[index - 1].GroupId == entry.GroupId)
                return;

            Splits.Move(index, index - 1);
            AssignGroupFromPosition(entry, index - 1);
        }
    }

    private void MoveDown(SplitEntry entry)
    {
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
        }
        else
        {
            if (index + 1 < Splits.Count &&
                Splits[index + 1].Type == SplitType.Parent)
                return;

            Splits.Move(index, index + 1);
            AssignGroupFromPosition(entry, index + 1);
        }
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


    private void FilterEvents()
    {
        FilteredEvents.Clear();
        
        foreach (var entry in AllEvents)
        {
            if (!string.IsNullOrEmpty(_searchText) &&
                !entry.Name.ToLower().Contains(_searchText.ToLower()))
                continue;

            FilteredEvents.Add(entry);
        }
    }

    private void NewProfile()
    {
        var result = MsgBox.ShowInput("Profile Name", "", "New Profile");


        if (string.IsNullOrWhiteSpace(result))
        {
            MsgBox.Show("Profile name required", "New Profile");
            return;
        }

        var profile = new Profile
        {
            Name = result,
            GameName = _gameName,
            Splits = []
        };

        _profileService.SaveProfile(profile);
        Profiles.Add(profile);
        SelectedProfile = profile;
    }

    private void Save()
    {
        if (_selectedProfile == null) return;

        RebuildGroupAssignments();

        _selectedProfile.Splits = Splits.ToList();
        _profileService.SaveProfile(_selectedProfile);
    }

    private void Delete()
    {
        if (_selectedProfile == null) return;

        _profileService.DeleteProfile(_gameName, _selectedProfile.Name);
        Profiles.Remove(_selectedProfile);
        SelectedProfile = Profiles.FirstOrDefault();
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
    
    

    #endregion
}