using ArchitectureLibrary.Signals;
using ArchitectureLibrary.ViewModel;
using GBATool.Commands;
using GBATool.Signals;
using GBATool.Utils;
using System.Windows.Media;

namespace GBATool.ViewModels
{
    public class PaletteViewerViewModel : ViewModel
    {
        private SolidColorBrush[] _solidColorBrushList = new SolidColorBrush[16];

        #region get/set
        public SolidColorBrush[] SolidColorBrushList
        {
            get => _solidColorBrushList;
            set
            {
                _solidColorBrushList = value;

                OnPropertyChanged("SolidColorBrushList");
            }
        }
        #endregion

        #region Commands
        public ShowColorPaletteCommand ShowColorPaletteCommand { get; } = new();
        #endregion

        public PaletteViewerViewModel()
        {
            SolidColorBrush[] tempList = new SolidColorBrush[16];

            for (int i = 0; i < 16; i++)
            {
                tempList[i] = new SolidColorBrush(Color.FromRgb(0, 0, 0));
            }

            SolidColorBrushList = tempList;
        }

        public override void OnActivate()
        {
            base.OnActivate();

            #region Signals
            SignalManager.Get<ColorPaletteSelectedSignal>().Listener += OnColorPaletteSelected;
            SignalManager.Get<PaletteColorArrayChangeSignal>().Listener += OnPaletteColorArrayChange;
            #endregion
        }

        public override void OnDeactivate()
        {
            base.OnDeactivate();

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
                tempList[i] = new SolidColorBrush(Util.GetColorFromInt(colors[i]));
            }

            SolidColorBrushList = tempList;
        }

        private void OnColorPaletteSelected(Color color, int colorIndex, int paletteIndex)
        {
            SolidColorBrushList[colorIndex] = new SolidColorBrush(color);

            OnPropertyChanged("SolidColorBrushList");
        }
    }
}
