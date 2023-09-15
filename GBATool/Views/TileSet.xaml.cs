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

            #region Signals
            SignalManager.Get<MouseWheelSignal>().Listener += OnMouseWheel;
            SignalManager.Get<SpriteSelectCursorSignal>().Listener += OnSpriteSelectCursor;
            SignalManager.Get<SpriteSize16x16Signal>().Listener += OnSpriteSize16x16;
            SignalManager.Get<SpriteSize16x32Signal>().Listener += OnSpriteSize16x32;
            SignalManager.Get<SpriteSize16x8Signal>().Listener += OnSpriteSize16x8;
            SignalManager.Get<SpriteSize32x16Signal>().Listener += OnSpriteSize32x16;
            SignalManager.Get<SpriteSize32x32Signal>().Listener += OnSpriteSize32x32;
            SignalManager.Get<SpriteSize32x64Signal>().Listener += OnSpriteSize32x64;
            SignalManager.Get<SpriteSize32x8Signal>().Listener += OnSpriteSize32x8;
            SignalManager.Get<SpriteSize64x32Signal>().Listener += OnSpriteSize64x32;
            SignalManager.Get<SpriteSize64x64Signal>().Listener += OnSpriteSize64x64;
            SignalManager.Get<SpriteSize8x16Signal>().Listener += OnSpriteSize8x16;
            SignalManager.Get<SpriteSize8x32Signal>().Listener += OnSpriteSize8x32;
            SignalManager.Get<SpriteSize8x8Signal>().Listener += OnSpriteSize8x8;
            SignalManager.Get<UpdateSpriteListSignal>().Listener += OnUpdateSpriteList;
            SignalManager.Get<SelectSpriteSignal>().Listener += OnSelectSprite;
            #endregion

            OnSpriteSelectCursor();
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
            #region Signals
            SignalManager.Get<MouseWheelSignal>().Listener -= OnMouseWheel;
            SignalManager.Get<SpriteSelectCursorSignal>().Listener -= OnSpriteSelectCursor;
            SignalManager.Get<SpriteSize16x16Signal>().Listener -= OnSpriteSize16x16;
            SignalManager.Get<SpriteSize16x32Signal>().Listener -= OnSpriteSize16x32;
            SignalManager.Get<SpriteSize16x8Signal>().Listener -= OnSpriteSize16x8;
            SignalManager.Get<SpriteSize32x16Signal>().Listener -= OnSpriteSize32x16;
            SignalManager.Get<SpriteSize32x32Signal>().Listener -= OnSpriteSize32x32;
            SignalManager.Get<SpriteSize32x64Signal>().Listener -= OnSpriteSize32x64;
            SignalManager.Get<SpriteSize32x8Signal>().Listener -= OnSpriteSize32x8;
            SignalManager.Get<SpriteSize64x32Signal>().Listener -= OnSpriteSize64x32;
            SignalManager.Get<SpriteSize64x64Signal>().Listener -= OnSpriteSize64x64;
            SignalManager.Get<SpriteSize8x16Signal>().Listener -= OnSpriteSize8x16;
            SignalManager.Get<SpriteSize8x32Signal>().Listener -= OnSpriteSize8x32;
            SignalManager.Get<SpriteSize8x8Signal>().Listener -= OnSpriteSize8x8;
            SignalManager.Get<UpdateSpriteListSignal>().Listener -= OnUpdateSpriteList;
            SignalManager.Get<SelectSpriteSignal>().Listener -= OnSelectSprite;
            #endregion
        }

        private void OnSelectSprite(SpriteVO sprite)
        {
            // TODO:
        }

        private void OnUpdateSpriteList()
        {
            if (DataContext is TileSetViewModel viewModel)
            {
                if (!viewModel.IsActive)
                {
                    return;
                }

                lvSprites.Items.Refresh();
            }
        }

        private void OnSpriteSelectCursor()
        {
            tbSelect.IsChecked = true;
            tb8x8.IsChecked = false;
            tb16x16.IsChecked = false;
            tb32x32.IsChecked = false;
            tb16x32.IsChecked = false;
            tb16x8.IsChecked = false;
            tb32x16.IsChecked = false;
            tb32x8.IsChecked = false;
            tb64x32.IsChecked = false;
            tb64x64.IsChecked = false;
            tb8x16.IsChecked = false;
            tb8x32.IsChecked = false;
            tb32x64.IsChecked = false;
        }

        private void OnSpriteSize16x16()
        {
            tbSelect.IsChecked = false;
            tb8x8.IsChecked = false;
            tb16x16.IsChecked = true;
            tb32x32.IsChecked = false;
            tb16x32.IsChecked = false;
            tb16x8.IsChecked = false;
            tb32x16.IsChecked = false;
            tb32x8.IsChecked = false;
            tb64x32.IsChecked = false;
            tb64x64.IsChecked = false;
            tb8x16.IsChecked = false;
            tb8x32.IsChecked = false;
            tb32x64.IsChecked = false;
        }

        private void OnSpriteSize16x32()
        {
            tbSelect.IsChecked = false;
            tb8x8.IsChecked = false;
            tb16x16.IsChecked = false;
            tb32x32.IsChecked = false;
            tb16x32.IsChecked = true;
            tb16x8.IsChecked = false;
            tb32x16.IsChecked = false;
            tb32x8.IsChecked = false;
            tb64x32.IsChecked = false;
            tb64x64.IsChecked = false;
            tb8x16.IsChecked = false;
            tb8x32.IsChecked = false;
            tb32x64.IsChecked = false;
        }

        private void OnSpriteSize16x8()
        {
            tbSelect.IsChecked = false;
            tb8x8.IsChecked = false;
            tb16x16.IsChecked = false;
            tb32x32.IsChecked = false;
            tb16x32.IsChecked = false;
            tb16x8.IsChecked = true;
            tb32x16.IsChecked = false;
            tb32x8.IsChecked = false;
            tb64x32.IsChecked = false;
            tb64x64.IsChecked = false;
            tb8x16.IsChecked = false;
            tb8x32.IsChecked = false;
            tb32x64.IsChecked = false;
        }

        private void OnSpriteSize32x16()
        {
            tbSelect.IsChecked = false;
            tb8x8.IsChecked = false;
            tb16x16.IsChecked = false;
            tb32x32.IsChecked = false;
            tb16x32.IsChecked = false;
            tb16x8.IsChecked = false;
            tb32x16.IsChecked = true;
            tb32x8.IsChecked = false;
            tb64x32.IsChecked = false;
            tb64x64.IsChecked = false;
            tb8x16.IsChecked = false;
            tb8x32.IsChecked = false;
            tb32x64.IsChecked = false;

        }

        private void OnSpriteSize32x32()
        {
            tbSelect.IsChecked = false;
            tb8x8.IsChecked = false;
            tb16x16.IsChecked = false;
            tb32x32.IsChecked = true;
            tb16x32.IsChecked = false;
            tb16x8.IsChecked = false;
            tb32x16.IsChecked = false;
            tb32x8.IsChecked = false;
            tb64x32.IsChecked = false;
            tb64x64.IsChecked = false;
            tb8x16.IsChecked = false;
            tb8x32.IsChecked = false;
            tb32x64.IsChecked = false;
        }

        private void OnSpriteSize32x64()
        {
            tbSelect.IsChecked = false;
            tb8x8.IsChecked = false;
            tb16x16.IsChecked = false;
            tb32x32.IsChecked = false;
            tb16x32.IsChecked = false;
            tb16x8.IsChecked = false;
            tb32x16.IsChecked = false;
            tb32x8.IsChecked = false;
            tb64x32.IsChecked = false;
            tb64x64.IsChecked = false;
            tb8x16.IsChecked = false;
            tb8x32.IsChecked = false;
            tb32x64.IsChecked = true;
        }

        private void OnSpriteSize32x8()
        {
            tbSelect.IsChecked = false;
            tb8x8.IsChecked = false;
            tb16x16.IsChecked = false;
            tb32x32.IsChecked = false;
            tb16x32.IsChecked = false;
            tb16x8.IsChecked = false;
            tb32x16.IsChecked = false;
            tb32x8.IsChecked = true;
            tb64x32.IsChecked = false;
            tb64x64.IsChecked = false;
            tb8x16.IsChecked = false;
            tb8x32.IsChecked = false;
            tb32x64.IsChecked = false;
        }

        private void OnSpriteSize64x32()
        {
            tbSelect.IsChecked = false;
            tb8x8.IsChecked = false;
            tb16x16.IsChecked = false;
            tb32x32.IsChecked = false;
            tb16x32.IsChecked = false;
            tb16x8.IsChecked = false;
            tb32x16.IsChecked = false;
            tb32x8.IsChecked = false;
            tb64x32.IsChecked = true;
            tb64x64.IsChecked = false;
            tb8x16.IsChecked = false;
            tb8x32.IsChecked = false;
            tb32x64.IsChecked = false;
        }

        private void OnSpriteSize64x64()
        {
            tbSelect.IsChecked = false;
            tb8x8.IsChecked = false;
            tb16x16.IsChecked = false;
            tb32x32.IsChecked = false;
            tb16x32.IsChecked = false;
            tb16x8.IsChecked = false;
            tb32x16.IsChecked = false;
            tb32x8.IsChecked = false;
            tb64x32.IsChecked = false;
            tb64x64.IsChecked = true;
            tb8x16.IsChecked = false;
            tb8x32.IsChecked = false;
            tb32x64.IsChecked = false;
        }

        private void OnSpriteSize8x16()
        {
            tbSelect.IsChecked = false;
            tb8x8.IsChecked = false;
            tb16x16.IsChecked = false;
            tb32x32.IsChecked = false;
            tb16x32.IsChecked = false;
            tb16x8.IsChecked = false;
            tb32x16.IsChecked = false;
            tb32x8.IsChecked = false;
            tb64x32.IsChecked = false;
            tb64x64.IsChecked = false;
            tb8x16.IsChecked = true;
            tb8x32.IsChecked = false;
            tb32x64.IsChecked = false;
        }

        private void OnSpriteSize8x32()
        {
            tbSelect.IsChecked = false;
            tb8x8.IsChecked = false;
            tb16x16.IsChecked = false;
            tb32x32.IsChecked = false;
            tb16x32.IsChecked = false;
            tb16x8.IsChecked = false;
            tb32x16.IsChecked = false;
            tb32x8.IsChecked = false;
            tb64x32.IsChecked = false;
            tb64x64.IsChecked = false;
            tb8x16.IsChecked = false;
            tb8x32.IsChecked = true;
            tb32x64.IsChecked = false;
        }

        private void OnSpriteSize8x8()
        {
            tbSelect.IsChecked = false;
            tb8x8.IsChecked = true;
            tb16x16.IsChecked = false;
            tb32x32.IsChecked = false;
            tb16x32.IsChecked = false;
            tb16x8.IsChecked = false;
            tb32x16.IsChecked = false;
            tb32x8.IsChecked = false;
            tb64x32.IsChecked = false;
            tb64x64.IsChecked = false;
            tb8x16.IsChecked = false;
            tb8x32.IsChecked = false;
            tb32x64.IsChecked = false;
        }
    }
}
