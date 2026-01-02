using ArchitectureLibrary.Signals;
using GBATool.Signals;
using GBATool.Utils;
using GBATool.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace GBATool.Views
{
    /// <summary>
    /// Interaction logic for Character.xaml
    /// </summary>
    public partial class Character : UserControl, ICleanable
    {
        public Character()
        {
            InitializeComponent();

            actionTabs.ItemsSource = vmCharacterModel.Tabs;

            #region Signals
            SignalManager.Get<CleanColorPaletteControlSelectedSignal>().Listener += OnCleanColorPaletteControlSelected;
            SignalManager.Get<ColorPaletteControlSelectedSignal>().Listener += OnColorPaletteControlSelected;
            #endregion
        }

        private void OnCleanColorPaletteControlSelected()
        {
            ColorPaletteCleanup();
        }

        private void OnColorPaletteControlSelected(int[] colors)
        {
            SolidColorBrush[] tempList = new SolidColorBrush[16];

            for (int i = 0; i < 16; i++)
            {
                tempList[i] = new SolidColorBrush(PaletteUtils.GetColorFromInt(colors[i]));
            }

            palette.SolidColorBrushList = tempList;
        }

        private void ContentControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ContentControl? tb = sender as ContentControl;

            if (tb?.DataContext is ActionTabItem item)
            {
                item.IsInEditMode = true;
            }
        }

        private void EditableTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox? tb = sender as TextBox;

            if (tb?.DataContext is ActionTabItem item)
            {
                tb.Text = item.OldCaptionValue;

                item.IsInEditMode = false;
            }
        }

        private void ActionTabs_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement source = (FrameworkElement)e.OriginalSource;

            if (source.DataContext.ToString() == "{NewItemPlaceholder}")
            {
                e.Handled = true;
            }
        }

        private void EditableTextBox_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is TextBox tb)
            {
                if (tb.IsVisible)
                {
                    // the method to set the focus to the TextBox
                    _ = Dispatcher.BeginInvoke(
                        DispatcherPriority.ContextIdle,
                        new Action(delegate ()
                        {
                            _ = tb.Focus();
                            tb.SelectAll();
                        }));

                    if (tb.DataContext is ActionTabItem item)
                    {
                        // back up - for possible cancelling
                        item.OldCaptionValue = tb.Text;
                    }
                }
            }
        }

        private void ColorPaletteCleanup()
        {
            SolidColorBrush[] tempList = new SolidColorBrush[16];
            SolidColorBrush brush = new(Util.NullColor);

            for (int i = 0; i < 16; i++)
            {
                tempList[i] = brush;
            }

            palette.SolidColorBrushList = tempList;
        }

        public void CleanUp()
        {
            ColorPaletteCleanup();

            #region Signals
            SignalManager.Get<CleanColorPaletteControlSelectedSignal>().Listener -= OnCleanColorPaletteControlSelected;
            SignalManager.Get<ColorPaletteControlSelectedSignal>().Listener -= OnColorPaletteControlSelected;
            #endregion
        }

        private void EditableTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            TextBox? tb = sender as TextBox;

            if (tb?.DataContext is ActionTabItem item)
            {
                if (e.Key == Key.Enter)
                {
                    item.IsInEditMode = false;

                    string name = tb.Text;

                    if (name != item.Header)
                    {
                        tb.Text = name;
                    }
                }

                if (e.Key == Key.Escape)
                {
                    tb.Text = item.OldCaptionValue;

                    item.IsInEditMode = false;
                }
            }
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex _regex = IsAllNumbersRegex();

            e.Handled = _regex.IsMatch(e.Text);
        }

        [GeneratedRegex("[^0-9.-]+")]
        private static partial Regex IsAllNumbersRegex();

        private void TabItem_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.Source is not TabItem tabItem)
            {
                return;
            }

            if (Mouse.PrimaryDevice.LeftButton == MouseButtonState.Pressed)
            {
                DragDrop.DoDragDrop(tabItem, tabItem, DragDropEffects.Move);
            }
        }

        private void TabItem_Drop(object sender, DragEventArgs e)
        {
            if (e.Source is not TabItem tabItemTarget)
            {
                return;
            }

            if (e.Data.GetData(typeof(TabItem)) is not TabItem tabItemSource)
            {
                return;
            }

            if (tabItemTarget.Equals(tabItemSource))
            {
                return;
            }

            DependencyObject parent = VisualTreeHelper.GetParent(tabItemTarget);

            while (parent != null)
            {
                if (parent is TabControl)
                {
                    break;
                }

                parent = VisualTreeHelper.GetParent(parent);
            }

            if (parent is TabControl tabControl)
            {
                if (tabItemTarget.Content is not ActionTabItem actionTabItemTarget)
                {
                    return;
                }

                if (tabItemSource.Content is not ActionTabItem actionTabItemSource)
                {
                    return;
                }

                List<ActionTabItem> tabItems = [.. tabControl.ItemsSource.Cast<ActionTabItem>()];

                int targetIndex = tabItems.IndexOf(actionTabItemTarget);
                int sourceIndex = tabItems.IndexOf(actionTabItemSource);

                SignalManager.Get<SwapAnimationTabSignal>().Dispatch(sourceIndex, targetIndex);
            }
        }
    }
}
