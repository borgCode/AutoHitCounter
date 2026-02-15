// 

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AutoHitCounter.Models;
using AutoHitCounter.Utilities;
using AutoHitCounter.ViewModels;

namespace AutoHitCounter.Behaviors;

public static class SplitListDragDropBehavior
{
    private static Point _dragStartPoint;
    private static bool _isDragging;

    public static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.RegisterAttached(
            "IsEnabled",
            typeof(bool),
            typeof(SplitListDragDropBehavior),
            new PropertyMetadata(false, OnIsEnabledChanged));

    public static bool GetIsEnabled(DependencyObject obj) => (bool)obj.GetValue(IsEnabledProperty);
    public static void SetIsEnabled(DependencyObject obj, bool value) => obj.SetValue(IsEnabledProperty, value);

    private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ListBox listBox) return;

        if ((bool)e.NewValue)
        {
            listBox.PreviewMouseLeftButtonDown += OnPreviewMouseLeftButtonDown;
            listBox.PreviewMouseMove += OnPreviewMouseMove;
            listBox.Drop += OnDrop;
            listBox.AllowDrop = true;
        }
        else
        {
            listBox.PreviewMouseLeftButtonDown -= OnPreviewMouseLeftButtonDown;
            listBox.PreviewMouseMove -= OnPreviewMouseMove;
            listBox.Drop -= OnDrop;
            listBox.AllowDrop = false;
        }
    }

    private static void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _dragStartPoint = e.GetPosition(null);
        _isDragging = false;
    }

    private static void OnPreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed) return;

        var diff = _dragStartPoint - e.GetPosition(null);

        if (Math.Abs(diff.X) < SystemParameters.MinimumHorizontalDragDistance &&
            Math.Abs(diff.Y) < SystemParameters.MinimumVerticalDragDistance)
            return;

        if (_isDragging) return;
        
        var listBoxItem = VisualTreeHelpers.FindAncestor<ListBoxItem>((DependencyObject)e.OriginalSource);
        if (listBoxItem == null) return;

        if (listBoxItem.DataContext is not SplitEntry entry) return;

        _isDragging = true;
        var data = new DataObject("SplitEntry", entry);
        DragDrop.DoDragDrop(listBoxItem, data, DragDropEffects.Move);
        _isDragging = false;
    }

    private static void OnDrop(object sender, DragEventArgs e)
    {
        if (!e.Data.GetDataPresent("SplitEntry")) return;

        var listBox = sender as ListBox;
        if (e.Data.GetData("SplitEntry") is not SplitEntry droppedEntry || listBox == null) return;

        if (listBox.DataContext is not ProfileEditorViewModel vm) return;

        var targetItem = VisualTreeHelpers.FindAncestor<ListBoxItem>((DependencyObject)e.OriginalSource);
        int dropIndex;

        if (targetItem != null)
        {
            var targetEntry = targetItem.DataContext as SplitEntry;
            dropIndex = vm.Splits.IndexOf(targetEntry);

            var pos = e.GetPosition(targetItem);
            if (pos.Y > targetItem.ActualHeight / 2)
                dropIndex++;
        }
        else
        {
            dropIndex = vm.Splits.Count;
        }

        vm.MoveSplit(droppedEntry, dropIndex);
    }
}