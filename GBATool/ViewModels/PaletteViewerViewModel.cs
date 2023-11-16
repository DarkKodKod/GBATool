using ArchitectureLibrary.ViewModel;
using GBATool.Commands;
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

        #region
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
    }
}
