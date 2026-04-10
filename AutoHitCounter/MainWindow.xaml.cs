using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using AutoHitCounter.Utilities;
using AutoHitCounter.ViewModels;
using AutoHitCounter.Views.Windows;

namespace AutoHitCounter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            Loaded += (s, e) =>
            {
                if (SettingsManager.Default.MainWindowLeft > 0)
                    Left = SettingsManager.Default.MainWindowLeft;

                if (SettingsManager.Default.MainWindowTop > 0)
                    Top = SettingsManager.Default.MainWindowTop;

                if (DataContext is MainViewModel vm)
                {
                    vm.PropertyChanged += MainViewModel_PropertyChanged;
                    vm.PropertyChanged += (s, e) =>
                    {
                        if (e.PropertyName == nameof(MainViewModel.CurrentSplit))
                            Dispatcher.BeginInvoke(DispatcherPriority.Loaded, () =>
                            {
                                if (vm.CurrentSplit != null)
                                    SplitListBox.ScrollIntoView(vm.CurrentSplit);
                            });
                    };
                }
            };
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            var clip = new RectangleGeometry(
                new Rect(0, 0, sizeInfo.NewSize.Width, sizeInfo.NewSize.Height), 5, 5);
            // find the root grid
            if (Content is Grid grid)
                grid.Clip = clip;
        }


        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);
            SettingsManager.Default.MainWindowLeft = Left;
            SettingsManager.Default.MainWindowTop = Top;
            SettingsManager.Default.Save();
        }


        private void SplitItem_RightClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListBoxItem item)
                item.IsSelected = true;
        }

        private void SplitItem_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is not ListBoxItem { DataContext: SplitViewModel split }) return;

            var vm = (MainViewModel)DataContext;

            if (vm.IsUnlocked)
                split.IsEditing = true;
            else
                vm.JumpToSplit(split);

            e.Handled = true;
        }

        private void RenameBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (sender is not TextBox { DataContext: SplitViewModel split }) return;

            if (e.Key == Key.Enter)
            {
                ((MainViewModel)DataContext).CommitRename(split);
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                split.IsEditing = false;
                e.Handled = true;
            }
        }

        private void RenameBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox { DataContext: SplitViewModel split })
                ((MainViewModel)DataContext).CommitRename(split);
        }

        private void MainViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MainViewModel.SelectedSplit))
            {
                var vm = (MainViewModel)DataContext;
                if (vm.SelectedSplit != null)
                    vm.SelectedSplit.PropertyChanged += SelectedSplit_PropertyChanged;
            }
        }

        private void SelectedSplit_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is not SplitViewModel split) return;

            if (e.PropertyName == nameof(SplitViewModel.IsEditing) && split.IsEditing)
            {
                Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Input, () =>
                {
                    var container = SplitListBox.ItemContainerGenerator.ContainerFromItem(split) as ListBoxItem;
                    if (container == null) return;
                    var textBox = VisualTreeHelpers.FindDescendant<TextBox>(container, "RenameBox");
                    textBox?.Focus();
                    textBox?.SelectAll();
                });
            }
            else if (e.PropertyName == nameof(SplitViewModel.IsEditingPb) && split.IsEditingPb)
            {
                Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Input, () =>
                {
                    var container = SplitListBox.ItemContainerGenerator.ContainerFromItem(split) as ListBoxItem;
                    if (container == null) return;
                    var textBox = VisualTreeHelpers.FindDescendant<TextBox>(container, "PbBox");
                    textBox?.Focus();
                    textBox?.SelectAll();
                });
            }
        }

        private void ResetAttempts_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
                vm.CommitAttemptsEdit("0");
        }

        private void AttemptsBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (DataContext is not MainViewModel vm) return;
            if (e.Key == Key.Enter)
                vm.CommitAttemptsEdit(((TextBox)sender).Text);
            else if (e.Key == Key.Escape)
                vm.CommitAttemptsEdit(vm.AttemptCount.ToString());
        }


        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseDown(e);
            var hit = e.OriginalSource as DependencyObject;
            if (DataContext is not MainViewModel vm) return;

            if (vm.IsEditingAttempts && hit != null && !IsDescendantOf(AttemptsBox, hit))
                vm.CommitAttemptsEdit(AttemptsBox.Text);

            var editingSplit = vm.Splits.FirstOrDefault(s => s.IsEditing);
            if (editingSplit != null)
            {
                var renameBox = FindRenameBox(SplitListBox, editingSplit);
                if (renameBox == null || (hit != null && !IsDescendantOf(renameBox, hit)))
                    vm.CommitRename(editingSplit);
            }

            var editingPbSplit = vm.Splits.FirstOrDefault(s => s.IsEditingPb);
            if (editingPbSplit != null)
            {
                var pbBox = FindRenameBox(SplitListBox, editingPbSplit, "PbBox");
                if (pbBox == null || (hit != null && !IsDescendantOf(pbBox, hit)))
                {
                    var clickedItem = hit != null
                        ? VisualTreeHelpers.FindAncestor<ListBoxItem>(hit as DependencyObject)
                        : null;
                    var clickedSplit = clickedItem?.DataContext as SplitViewModel;

                    vm.CommitPbEdit(editingPbSplit, pbBox?.Text ?? editingPbSplit.PersonalBest.ToString());
                    
                    if (clickedSplit != null)
                    {
                        var newSplit = vm.Splits.FirstOrDefault(s => s.Name == clickedSplit.Name);
                        if (newSplit != null)
                            vm.SelectedSplit = newSplit;
                    }
                }
            }
        }

        private static bool IsDescendantOf(DependencyObject parent, DependencyObject child)
        {
            var current = child;
            while (current != null)
            {
                if (current == parent) return true;
                current = VisualTreeHelper.GetParent(current);
            }

            return false;
        }

        private static TextBox FindRenameBox(DependencyObject parent, object dataContext, string name = "RenameBox")
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is TextBox tb && tb.Name == name && tb.DataContext == dataContext)
                    return tb;
                var result = FindRenameBox(child, dataContext, name);
                if (result != null) return result;
            }

            return null;
        }

        private void SplitContextDots_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left) return;

            if (sender is FrameworkElement fe)
            {
                var item = VisualTreeHelpers.FindAncestor<ListBoxItem>(fe);
                if (item != null)
                    item.IsSelected = true;
            }

            var contextMenu = SplitListBox.ContextMenu;
            if (contextMenu == null) return;
            contextMenu.PlacementTarget = SplitListBox;
            contextMenu.IsOpen = true;
            e.Handled = true;
        }

        private void SplitList_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
                vm.IsSplitListScrollbarVisible = e.ExtentHeight > e.ViewportHeight;
        }

        private void PbBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (sender is not TextBox box) return;
            if (box.DataContext is not SplitViewModel split) return;

            if (e.Key == Key.Enter)
            {
                if (DataContext is MainViewModel vm)
                    vm.CommitPbEdit(split, box.Text);
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                split.IsEditingPb = false;
                e.Handled = true;
            }
        }

        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            new HelpWindow { Owner = this }.Show();
        }

        private void NotesBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
                vm.SaveNotesCommand.Execute(null);
        }
    }
}