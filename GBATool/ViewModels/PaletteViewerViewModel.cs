using ArchitectureLibrary.Signals;
using ArchitectureLibrary.ViewModel;
using GBATool.Commands;
using GBATool.Signals;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace GBATool.ViewModels
{
    public class PaletteViewerViewModel : ViewModel
    {
        private ObservableCollection<SolidColorBrush> _solidColorBrushList = new();

        #region Commands
        public ShowColorPaletteCommand ShowColorPaletteCommand { get; } = new();
        #endregion

        #region get/set
        public ObservableCollection<SolidColorBrush> SolidColorBrushList
        {
            get => _solidColorBrushList;
            set
            {
                _solidColorBrushList = value;

                OnPropertyChanged("SolidColorBrushList");
            }
        }
        #endregion

        public PaletteViewerViewModel()
        {
            for (int i = 0; i < 16; i++)
            {
                SolidColorBrushList.Add(new SolidColorBrush(Color.FromRgb(0, 0, 0)));
            }
        }

        public override void OnActivate()
        {
            base.OnActivate();

            #region Signals
            SignalManager.Get<ColorPaletteSelectedSignal>().Listener += OnColorPaletteSelected;
            #endregion
        }

        public override void OnDeactivate()
        {
            base.OnDeactivate();

            #region Signals
            SignalManager.Get<ColorPaletteSelectedSignal>().Listener -= OnColorPaletteSelected;
            #endregion
        }

        private void OnColorPaletteSelected(Color color, int colorIndex)
        {
            SolidColorBrushList[colorIndex] = new SolidColorBrush(color);
        }
    }
}
