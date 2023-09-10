using ArchitectureLibrary.Signals;
using GBATool.Signals;
using GBATool.UserControls;
using GBATool.ViewModels;
using GBATool.VOs;
using System.Windows.Controls;

namespace GBATool.Views
{
    /// <summary>
    /// Interaction logic for TileSet.xaml
    /// </summary>
    public partial class TileSet : UserControl, ICleanable
    {
        public TileSet()
        {
            InitializeComponent();

            SignalManager.Get<MouseWheelSignal>().Listener += OnMouseWheel;
        }

        private void OnMouseWheel(MouseWheelVO vo)
        {
            if (DataContext is TileSetViewModel viewModel)
            {
                if (!viewModel.IsActive)
                {
                    return;
                }
            }

            const double ScaleRate = 1.1;

            if (vo.Delta > 0)
            {
                scaleCanvas.ScaleX *= ScaleRate;
                scaleCanvas.ScaleY *= ScaleRate;
            }
            else
            {
                scaleCanvas.ScaleX /= ScaleRate;
                scaleCanvas.ScaleY /= ScaleRate;
            }
        }

        public void CleanUp()
        {
            SignalManager.Get<MouseWheelSignal>().Listener -= OnMouseWheel;
        }
    }
}
