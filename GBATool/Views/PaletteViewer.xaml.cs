using ArchitectureLibrary.Signals;
using GBATool.Commands.Palettes;
using GBATool.Signals;
using GBATool.Utils;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GBATool.Views
{
    /// <summary>
    /// Interaction logic for PaletteViewer.xaml
    /// </summary>
    public partial class PaletteViewer : UserControl, INotifyPropertyChanged
    {
        private SolidColorBrush[] _solidColorBrushList = new SolidColorBrush[16];

        public static readonly DependencyProperty PaletteIndexProperty =
            DependencyProperty.Register("PaletteIndex",
                typeof(int),
                typeof(PaletteViewer),
                new PropertyMetadata(0));

        public event PropertyChangedEventHandler? PropertyChanged;

        #region get/set
        public int PaletteIndex
        {
            get => (int)GetValue(PaletteIndexProperty);
            set => SetValue(PaletteIndexProperty, value);
        }

        public SolidColorBrush[] SolidColorBrushList
        {
            get => _solidColorBrushList;
            set
            {
                _solidColorBrushList = value;

                OnPropertyChanged(nameof(SolidColorBrushList));
            }
        }
        #endregion

        #region Commands
        public ShowColorPaletteCommand ShowColorPaletteCommand { get; } = new();
        #endregion

        public PaletteViewer()
        {
            InitializeComponent();

            SolidColorBrush[] tempList = new SolidColorBrush[16];

            for (int i = 0; i < 16; i++)
            {
                tempList[i] = new SolidColorBrush(Color.FromRgb(0, 0, 0));
            }

            SolidColorBrushList = tempList;
        }

        public void OnActivate()
        {
            #region Signals
            SignalManager.Get<ColorPaletteSelectedSignal>().Listener += OnColorPaletteSelected;
            SignalManager.Get<PaletteColorArrayChangeSignal>().Listener += OnPaletteColorArrayChange;
            #endregion
        }

        public void OnDeactivate()
        {
            #region Signals
            SignalManager.Get<ColorPaletteSelectedSignal>().Listener -= OnColorPaletteSelected;
            SignalManager.Get<PaletteColorArrayChangeSignal>().Listener -= OnPaletteColorArrayChange;
            #endregion
        }

        private void OnPaletteColorArrayChange(int[] colors)
        {
            SolidColorBrush[] tempList = new SolidColorBrush[16];

            for (int i = 0; i < 16; i++)
            {
                tempList[i] = new SolidColorBrush(PaletteUtils.GetColorFromInt(colors[i]));
            }

            SolidColorBrushList = tempList;
        }

        private void OnColorPaletteSelected(Color color, int colorIndex, int paletteIndex)
        {
            if (PaletteIndex != paletteIndex)
            {
                return;
            }

            SolidColorBrushList[colorIndex] = new SolidColorBrush(color);

            OnPropertyChanged(nameof(SolidColorBrushList));
        }

        protected virtual void OnPropertyChanged(string propname)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));
        }
    }
}
