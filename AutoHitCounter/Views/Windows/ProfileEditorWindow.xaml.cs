using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using AutoHitCounter.Models;
using AutoHitCounter.Utilities;
using AutoHitCounter.ViewModels;

namespace AutoHitCounter.Views.Windows;

public partial class ProfileEditorWindow : Window
{
    public ProfileEditorWindow()
    {
        InitializeComponent();
        TemplatesListBox.SelectionChanged += TemplatesListBox_SelectionChanged;
        SplitsListBox.SelectionChanged += SplitsListBox_SelectionChanged;
        
        if (Application.Current.MainWindow != null)
          {
              Application.Current.MainWindow.Closing += (sender, args) => { Close(); };
          }
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        if (DataContext is ProfileEditorViewModel { IsDirty: true } vm)
        {
            var result = MsgBox.ShowYesNoCancel("You have unsaved changes. Would you like to save before closing?",
                "Unsaved Changes");

            if (result == null)
            {
                e.Cancel = true;
            }
            else if (result == true)
            {
                vm.SaveCommand.Execute(null);
            }
        }

        base.OnClosing(e);
    }

    private void TemplatesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is not ProfileEditorViewModel vm) return;

        foreach (SplitEntry item in e.RemovedItems)
            vm.SelectedTemplates.Remove(item);
        foreach (SplitEntry item in e.AddedItems)
            vm.SelectedTemplates.Add(item);
    }

    private void SplitsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is not ProfileEditorViewModel vm) return;

        foreach (SplitEntry item in e.RemovedItems)
            vm.SelectedSplits.Remove(item);
        foreach (SplitEntry item in e.AddedItems)
            vm.SelectedSplits.Add(item);
    }

    private void ExportButton_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not ProfileEditorViewModel vm) return;

        var scope = MsgBox.ShowCustomButtons("Which profiles would you like to export?", "Export",
            [CustomMessageBoxResult.All, CustomMessageBoxResult.Current, CustomMessageBoxResult.Cancel]);
        if (scope == CustomMessageBoxResult.Cancel) return;

        var profilesToExport = scope == CustomMessageBoxResult.All
            ? vm.Profiles.ToList()
            : vm.SelectedProfile != null
                ? new List<Profile> { vm.SelectedProfile }
                : null;

        if (profilesToExport == null || !profilesToExport.Any())
        {
            MsgBox.Show("No profile selected to export.", "Export");
            return;
        }

        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Filter = "JSON files (*.json)|*.json",
            DefaultExt = ".json",
            FileName = scope == CustomMessageBoxResult.All ? $"{vm.GameName} Profiles" : vm.SelectedProfile?.Name,
            InitialDirectory = SettingsManager.Default.LastImportExportPath
        };

        if (dialog.ShowDialog() != true) return;

        SettingsManager.Default.LastImportExportPath = Path.GetDirectoryName(dialog.FileName);
        SettingsManager.Default.Save();

        var json = JsonSerializer.Serialize(profilesToExport, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(dialog.FileName, json);
    }

    private void ImportButton_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not ProfileEditorViewModel vm) return;

        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "JSON files (*.json)|*.json",
            DefaultExt = ".json",
            InitialDirectory = SettingsManager.Default.LastImportExportPath
        };

        if (dialog.ShowDialog() != true) return;

        SettingsManager.Default.LastImportExportPath = Path.GetDirectoryName(dialog.FileName);
        SettingsManager.Default.Save();

        List<Profile> imported;
        try
        {
            var json = File.ReadAllText(dialog.FileName);
            imported = JsonSerializer.Deserialize<List<Profile>>(json);
        }
        catch
        {
            MsgBox.Show("Failed to read the file. Make sure it's a valid profile export.", "Import");
            return;
        }

        if (imported == null || !imported.Any())
        {
            MsgBox.Show("No profiles found in the file.", "Import");
            return;
        }

        foreach (var profile in imported)
        {
            profile.GameName = vm.GameName;
            var existing = vm.Profiles.FirstOrDefault(p => p.Name == profile.Name);

            if (existing != null)
            {
                var result = MsgBox.ShowCustomButtons(
                    $"Profile \"{profile.Name}\" already exists.{Environment.NewLine}What would you like to do?",
                    "Import Conflict",
                    [CustomMessageBoxResult.Replace, CustomMessageBoxResult.Rename, CustomMessageBoxResult.Skip]);
                if (result == CustomMessageBoxResult.Skip) continue;
                if (result == CustomMessageBoxResult.Rename)
                {
                    var newName = MsgBox.ShowInput("New profile name", profile.Name, "Rename Profile");
                    if (string.IsNullOrWhiteSpace(newName) || newName == profile.Name) continue;
                    if (vm.Profiles.Any(p => p.Name == newName))
                    {
                        MsgBox.Show($"A profile named \"{newName}\" already exists.", "Rename Profile");
                        continue;
                    }
                    profile.Name = newName;
                }
            }

            vm.ImportProfile(profile);
            
        }
        vm.NotifySaved();
    }
}