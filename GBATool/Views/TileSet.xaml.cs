using ArchitectureLibrary.Signals;
using GBATool.FileSystem;
using GBATool.Models;
using GBATool.Signals;
using GBATool.Utils;
using GBATool.ViewModels;
using GBATool.VOs;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace GBATool.Views
{
    /// <summary>
    /// Interaction logic for TileSet.xaml
    /// </summary>
    public partial class TileSet : UserControl, ICleanable
    {
        private readonly static Color StrokeColor = Color.FromArgb(255, 255, 0, 255);
        private const double StrokeThickness = 0.2;

        private System.Windows.Point _previousMousePosition = new(0, 0);
        private bool _validPanningMovement = false;

        public TileSet()
        {
            InitializeComponent();

            slSprites.OnActivate();

            #region Signals
            SignalManager.Get<MouseWheelSignal>().Listener += OnMouseWheel;
            SignalManager.Get<MouseMoveSignal>().Listener += OnMouseMove;
            SignalManager.Get<MouseLeaveSignal>().Listener += OnMouseLeave;
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
            SignalManager.Get<AddSpriteSignal>().Listener += OnAddSprite;
            SignalManager.Get<DeletingSpriteSignal>().Listener += OnDeletingSprite;
            SignalManager.Get<SelectSpriteSignal>().Listener += OnSelectSprite;
            #endregion

            OnSpriteSelectCursor();
        }

        private void OnMouseLeave(MouseLeaveVO vo)
        {
            _previousMousePosition = new(0, 0);
            _validPanningMovement = false;
        }

        private void OnMouseMove(MouseMoveVO vo)
        {
            if (vo.Sender is ScrollViewer)
            {
                return;
            }

            if (tbSelect.IsChecked == false || !_validPanningMovement)
            {
                return;
            }

            if (DataContext is TileSetViewModel viewModel)
            {
                if (!viewModel.IsActive)
                {
                    return;
                }
            }

            if (vo.LeftButton != MouseButtonState.Pressed)
            {
                return;
            }

            if (_previousMousePosition.X != 0 && _previousMousePosition.Y != 0)
            {
                double dXInTargetPixels = vo.AbsolutePosition.X - _previousMousePosition.X;
                double dYInTargetPixels = vo.AbsolutePosition.Y - _previousMousePosition.Y;

                double newOffsetX = scrollViewer.HorizontalOffset - dXInTargetPixels;
                double newOffsetY = scrollViewer.VerticalOffset - dYInTargetPixels;

                if (double.IsNaN(newOffsetX) || double.IsNaN(newOffsetY))
                {
                    return;
                }

                scrollViewer.ScrollToHorizontalOffset(newOffsetX);
                scrollViewer.ScrollToVerticalOffset(newOffsetY);
            }

            _previousMousePosition = vo.AbsolutePosition;
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
            SignalManager.Get<MouseMoveSignal>().Listener -= OnMouseMove;
            SignalManager.Get<MouseLeaveSignal>().Listener -= OnMouseLeave;
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
            SignalManager.Get<AddSpriteSignal>().Listener -= OnAddSprite;
            SignalManager.Get<DeletingSpriteSignal>().Listener -= OnDeletingSprite;
            SignalManager.Get<SelectSpriteSignal>().Listener -= OnSelectSprite;
            #endregion

            slSprites.OnDeactivate();
        }

        private void OnSelectSprite(SpriteVO vo)
        {
            IEnumerable<Rectangle> rectangles = cvsImage.Children.OfType<Rectangle>();

            foreach (Rectangle rect in rectangles)
            {
                rect.Stroke = rect.Uid == vo.SpriteID ? new SolidColorBrush(Colors.Yellow) : new SolidColorBrush(StrokeColor);
            }
        }

        private void OnDeletingSprite(SpriteVO vo)
        {
            IEnumerable<Rectangle> rectangles = cvsImage.Children.OfType<Rectangle>();

            Rectangle? rect = rectangles.FirstOrDefault(rect => rect.Uid == vo.SpriteID);

            if (rect == null)
                return;

            cvsImage.Children.Remove(rect);
        }

        private void OnAddSprite(SpriteVO vo)
        {
            Rectangle rectangle = new()
            {
                Width = vo.Width / TileSetModel.ImageScale,
                Height = vo.Height / TileSetModel.ImageScale,
                Stroke = new SolidColorBrush(StrokeColor),
                StrokeThickness = StrokeThickness,
                Uid = vo.SpriteID
            };

            if (string.IsNullOrEmpty(vo.TileSetID))
                return;

            TileSetModel? model = ProjectFiles.GetModel<TileSetModel>(vo.TileSetID);

            if (model == null)
                return;

            SpriteModel? spriteModel = model.Sprites.Find(x => x.ID == vo.SpriteID);

            if (spriteModel == null)
                return;

            Canvas.SetTop(rectangle, spriteModel.PosY);
            Canvas.SetLeft(rectangle, spriteModel.PosX);

            cvsImage.Children.Add(rectangle);
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

        private void ScrollViewer_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            _previousMousePosition = new(0, 0);
        }

        private void ScrollViewer_MouseEnter(object sender, MouseEventArgs e)
        {
            _validPanningMovement = true;
        }
    }
}
