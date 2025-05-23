using ArchitectureLibrary.Signals;
using GBATool.Signals;
using GBATool.Utils;
using GBATool.ViewModels;
using System;
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
                tempList[i] = new SolidColorBrush(Util.GetColorFromInt(colors[i]));
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
    }
}
