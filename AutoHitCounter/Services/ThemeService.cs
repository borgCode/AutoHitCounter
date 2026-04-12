using Microsoft.Win32;
using System;
using System.Linq;
using System.Windows;
using AutoHitCounter.Enums;

namespace AutoHitCounter.Services;

public class ThemeService
{
    private static ThemeMode _currentMode = ThemeMode.Dark;
    
        public static event Action ThemeChanged;

        public static void Apply(ThemeMode mode)
        {
            _currentMode = mode;

            if (mode == ThemeMode.System)
                ApplyThemeDictionary(IsSystemDark() ? "Dark" : "Light");
            else
                ApplyThemeDictionary(mode.ToString());
        }

        // Switch colours according to system default cause sounds cleaner idk
        
        // On App Start
        public static void StartWatchingSystem()
        {
            SystemEvents.UserPreferenceChanged += OnUserPreferenceChanged;
        }

        // On App Closing
        public static void StopWatchingSystem()
        {
            SystemEvents.UserPreferenceChanged -= OnUserPreferenceChanged;
        }

        private static void OnUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            if (e.Category != UserPreferenceCategory.General) return;
            
            if (_currentMode != ThemeMode.System) return;
            
            Application.Current.Dispatcher.Invoke(() =>
            {
                ApplyThemeDictionary(IsSystemDark() ? "Dark" : "Light");
            });
        }

        private static void ApplyThemeDictionary(string themeName)
        {
            var dict = new ResourceDictionary
            {
                Source = new Uri($"Themes/{themeName}Theme.xaml", UriKind.Relative)
            };

            var existing = Application.Current.Resources.MergedDictionaries
                .FirstOrDefault(d => d.Source?.OriginalString.Contains("Theme") == true);

            if (existing != null)
                Application.Current.Resources.MergedDictionaries.Remove(existing);

            Application.Current.Resources.MergedDictionaries.Add(dict);

            ThemeChanged?.Invoke();
        }

        private static bool IsSystemDark()
        {
            const string keyPath = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
            using var key = Registry.CurrentUser.OpenSubKey(keyPath);
            var value = key?.GetValue("AppsUseLightTheme");
            // 0 = dark mode, 1 = light mode
            return value is int i && i == 0;
        }
    }
