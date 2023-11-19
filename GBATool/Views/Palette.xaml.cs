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

            if (palette.DataContext is PaletteViewerViewModel viewerViewModel)
            {
                viewerViewModel.OnActivate();
            }
        }

        public void CleanUp()
        {
            if (palette.DataContext is PaletteViewerViewModel viewerViewModel)
            {
                viewerViewModel.OnDeactivate();
            }
        }
    }
}
