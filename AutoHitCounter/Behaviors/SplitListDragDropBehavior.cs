// 

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using AutoHitCounter.Interfaces;
using AutoHitCounter.Utilities;

namespace AutoHitCounter.Behaviors;

public static class SplitListDragDropBehavior
{
    private const string DragDataFormat = "SplitListItem";

    private static Point _dragStartPoint;
    private static bool _isDragging;

    private static InsertionAdorner _currentAdorner;
    private static ListBoxItem _lastAdornedItem;
    private static bool _lastDrawAbove;

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
            listBox.DragOver += OnDragOver;
            listBox.DragLeave += OnDragLeave;
            listBox.Drop += OnDrop;
            listBox.AllowDrop = true;
        }
        else
        {
            listBox.PreviewMouseLeftButtonDown -= OnPreviewMouseLeftButtonDown;
            listBox.PreviewMouseMove -= OnPreviewMouseMove;
            listBox.DragOver -= OnDragOver;
            listBox.DragLeave -= OnDragLeave;
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
        if (listBoxItem?.DataContext == null) return;

        _isDragging = true;
        var data = new DataObject(DragDataFormat, listBoxItem.DataContext);
        DragDrop.DoDragDrop(listBoxItem, data, DragDropEffects.Move);
        _isDragging = false;
    }

    private static void OnDragOver(object sender, DragEventArgs e)
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

        e.Effects = DragDropEffects.Move;
        e.Handled = true;
    }

    private static void OnDragLeave(object sender, DragEventArgs e)
    {
        RemoveAdorner();
    }

    private static void OnDrop(object sender, DragEventArgs e)
    {
        RemoveAdorner();

        if (!e.Data.GetDataPresent(DragDataFormat)) return;

        var listBox = sender as ListBox;
        var droppedItem = e.Data.GetData(DragDataFormat);

        if (droppedItem == null || listBox == null) return;

        if (listBox.DataContext is not IReorderHandler handler) return;

        var targetItem = VisualTreeHelpers.FindAncestor<ListBoxItem>((DependencyObject)e.OriginalSource);
        int dropIndex;

        if (targetItem != null)
        {
            var targetData = targetItem.DataContext;
            dropIndex = listBox.Items.IndexOf(targetData);

            var pos = e.GetPosition(targetItem);
            if (pos.Y > targetItem.ActualHeight / 2)
                dropIndex++;
        }
        else
        {
            dropIndex = listBox.Items.Count;
        }

        handler.MoveItem(droppedItem, dropIndex);
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