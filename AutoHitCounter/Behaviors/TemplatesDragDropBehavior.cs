using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using AutoHitCounter.Models;
using AutoHitCounter.Utilities;
using AutoHitCounter.ViewModels;

namespace AutoHitCounter.Behaviors;

public static class TemplatesDragDropBehavior
{
    private const string DragDataFormat = "TemplateItems";

    private static Point _dragStartPoint;
    private static bool _isDragging;
    private static List<SplitEntry> _pendingDragItems;
    private static InsertionAdorner _currentAdorner;
    private static ListBoxItem _lastAdornedItem;
    private static bool _lastDrawAbove;

    // --- Source (templates ListBox) ---

    public static readonly DependencyProperty IsSourceProperty =
        DependencyProperty.RegisterAttached(
            "IsSource",
            typeof(bool),
            typeof(TemplatesDragDropBehavior),
            new PropertyMetadata(false, OnIsSourceChanged));

    public static bool GetIsSource(DependencyObject obj) => (bool)obj.GetValue(IsSourceProperty);
    public static void SetIsSource(DependencyObject obj, bool value) => obj.SetValue(IsSourceProperty, value);

    private static void OnIsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ListBox listBox) return;

        if ((bool)e.NewValue)
        {
            listBox.PreviewMouseLeftButtonDown += OnSourceMouseDown;
            listBox.PreviewMouseMove += OnSourceMouseMove;
        }
        else
        {
            listBox.PreviewMouseLeftButtonDown -= OnSourceMouseDown;
            listBox.PreviewMouseMove -= OnSourceMouseMove;
        }
    }

    private static void OnSourceMouseDown(object sender, MouseButtonEventArgs e)
    {
        var item = VisualTreeHelpers.FindAncestor<ListBoxItem>((DependencyObject)e.OriginalSource);
        if (item == null) return;

        _dragStartPoint = e.GetPosition(null);
        _isDragging = false;

        var listBox = sender as ListBox;
        if (listBox != null)
            _pendingDragItems = listBox.SelectedItems.Cast<SplitEntry>().ToList();
    }

    private static void OnSourceMouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed) return;

        var diff = _dragStartPoint - e.GetPosition(null);
        if (System.Math.Abs(diff.X) < SystemParameters.MinimumHorizontalDragDistance &&
            System.Math.Abs(diff.Y) < SystemParameters.MinimumVerticalDragDistance)
            return;

        if (_isDragging) return;
        if (_pendingDragItems == null || !_pendingDragItems.Any()) return;

        var listBox = sender as ListBox;
        if (listBox == null) return;


        _isDragging = true;
        _dragStartPoint = default;
        System.Diagnostics.Debug.WriteLine($"Dragging {_pendingDragItems.Count} items");
        var data = new DataObject(DragDataFormat, _pendingDragItems);
        _pendingDragItems = null;
        DragDrop.DoDragDrop(listBox, data, DragDropEffects.Copy);
        _isDragging = false;
    }

    // --- Target (splits ListBox) ---

    public static readonly DependencyProperty IsTargetProperty =
        DependencyProperty.RegisterAttached(
            "IsTarget",
            typeof(bool),
            typeof(TemplatesDragDropBehavior),
            new PropertyMetadata(false, OnIsTargetChanged));

    public static bool GetIsTarget(DependencyObject obj) => (bool)obj.GetValue(IsTargetProperty);
    public static void SetIsTarget(DependencyObject obj, bool value) => obj.SetValue(IsTargetProperty, value);

    private static void OnIsTargetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ListBox listBox) return;

        if ((bool)e.NewValue)
        {
            listBox.DragOver += OnTargetDragOver;
            listBox.DragLeave += OnTargetDragLeave;
            listBox.Drop += OnTargetDrop;
            listBox.AllowDrop = true;
        }
        else
        {
            listBox.DragOver -= OnTargetDragOver;
            listBox.DragLeave -= OnTargetDragLeave;
            listBox.Drop -= OnTargetDrop;
            listBox.AllowDrop = false;
        }
    }

    private static void OnTargetDragOver(object sender, DragEventArgs e)
    {
        if (!e.Data.GetDataPresent(DragDataFormat))
        {
            e.Effects = DragDropEffects.None;
            return;
        }

        var targetItem = VisualTreeHelpers.FindAncestor<ListBoxItem>((DependencyObject)e.OriginalSource);

        if (targetItem == null)
        {
            RemoveAdorner();
            return;
        }

        var pos = e.GetPosition(targetItem);
        var drawAbove = pos.Y <= targetItem.ActualHeight / 2;

        if (targetItem != _lastAdornedItem || drawAbove != _lastDrawAbove)
        {
            RemoveAdorner();
            ShowAdorner(targetItem, drawAbove);
        }

        e.Effects = DragDropEffects.Copy;
        e.Handled = true;
    }

    private static void OnTargetDragLeave(object sender, DragEventArgs e)
    {
        RemoveAdorner();
    }

    private static void OnTargetDrop(object sender, DragEventArgs e)
    {
        RemoveAdorner();

        if (!e.Data.GetDataPresent(DragDataFormat)) return;

        var listBox = sender as ListBox;
        if (listBox == null) return;

        var items = e.Data.GetData(DragDataFormat) as List<SplitEntry>;
        if (items == null || !items.Any()) return;

        if (listBox.DataContext is not ProfileEditorViewModel vm) return;

        if (!vm.AllowDuplicates)
            items = items.Where(i => vm.Splits.All(s => s.EventId != i.EventId)).ToList();
        
        if (!items.Any()) return;

        var targetItem = VisualTreeHelpers.FindAncestor<ListBoxItem>((DependencyObject)e.OriginalSource);
        int dropIndex;

        if (targetItem != null)
        {
            dropIndex = listBox.Items.IndexOf(targetItem.DataContext);
            var pos = e.GetPosition(targetItem);
            if (pos.Y > targetItem.ActualHeight / 2)
                dropIndex++;
        }
        else
        {
            dropIndex = listBox.Items.Count;
        }

        vm.DropFromTemplates(items, dropIndex);
    }

    private static void ShowAdorner(ListBoxItem item, bool drawAbove)
    {
        var layer = AdornerLayer.GetAdornerLayer(item);
        if (layer == null) return;

        _currentAdorner = new InsertionAdorner(item, drawAbove);
        _lastAdornedItem = item;
        _lastDrawAbove = drawAbove;
        layer.Add(_currentAdorner);
    }

    private static void RemoveAdorner()
    {
        if (_currentAdorner != null && _lastAdornedItem != null)
        {
            var layer = AdornerLayer.GetAdornerLayer(_lastAdornedItem);
            layer?.Remove(_currentAdorner);
        }

        _currentAdorner = null;
        _lastAdornedItem = null;
    }
}