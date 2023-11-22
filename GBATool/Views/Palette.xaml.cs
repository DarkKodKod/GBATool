using GBATool.Utils;
using GBATool.ViewModels;
using System.Windows.Controls;

namespace GBATool.Views
{
    /// <summary>
    /// Interaction logic for Palette.xaml
    /// </summary>
    public partial class Palette : UserControl, ICleanable
    {
        public Palette()
        {
            InitializeComponent();

            if (palette.DataContext is PaletteViewerViewModel paletteViewerViewModel)
            {
                paletteViewerViewModel.OnActivate();
            }
        }

        public void CleanUp()
        {
            if (palette.DataContext is PaletteViewerViewModel paletteViewerViewModel)
            {
                paletteViewerViewModel.OnDeactivate();
            }
        }
    }
}
