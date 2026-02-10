// 

using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using AutoHitCounter.Interfaces;
using AutoHitCounter.Models;

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
        Profile? activeProfile)
    {
        _allEvents = allEvents;
        _profileService = profileService;
        _gameName = gameName;

        Profiles = new ObservableCollection<Profile>(profileService.GetProfiles(gameName));

        foreach (var kvp in allEvents)
            AllEvents.Add(new SplitEntry { EventId = kvp.Key, Name = kvp.Value });

        if (activeProfile != null)
        {
            SelectedProfile = Profiles.FirstOrDefault(p => p.Name == activeProfile.Name);
        }
    }
    
    
    #region Properties

    public ObservableCollection<SplitEntry> AllEvents { get; } = new();
    public ObservableCollection<SplitEntry> Splits { get; } = new();
    public ObservableCollection<Profile> Profiles { get; }

    private Profile? _selectedProfile;
    public Profile? SelectedProfile
    {
        get => _selectedProfile;
        set
        {
            if (SetProperty(ref _selectedProfile, value))
                LoadProfile(value);
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

    public ObservableCollection<SplitEntry> FilteredEvents { get; } = new();

    #endregion

    #region Private Methods

    private void LoadProfile(Profile? profile)
    {
        Splits.Clear();
        if (profile == null) return;

        foreach (var split in profile.Splits)
            Splits.Add(split);

        FilterEvents();
    }

    private void Add(SplitEntry entry)
    {
        Splits.Add(new SplitEntry { EventId = entry.EventId, Name = entry.Name });
        FilterEvents();
    }

    private void Remove(SplitEntry entry)
    {
        Splits.Remove(entry);
        FilterEvents();
    }

    private void MoveUp(SplitEntry entry)
    {
        var index = Splits.IndexOf(entry);
        Splits.Move(index, index - 1);
    }

    private void MoveDown(SplitEntry entry)
    {
        var index = Splits.IndexOf(entry);
        Splits.Move(index, index + 1);
    }

    private void FilterEvents()
    {
        FilteredEvents.Clear();
        var selectedIds = new HashSet<uint>(Splits.Select(s => s.EventId));

        // foreach (var entry in AllEvents)
        // {
        //     if (!string.IsNullOrEmpty(_searchText) &&
        //         !entry.Name.Contains(_searchText, StringComparison.OrdinalIgnoreCase))
        //         continue;
        //
        //     FilteredEvents.Add(entry);
        // }
    }

    private void NewProfile()
    {
        var profile = new Profile
        {
            Name = "New Profile",
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

    #endregion
}