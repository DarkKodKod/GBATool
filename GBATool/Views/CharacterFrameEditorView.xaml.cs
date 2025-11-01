using ArchitectureLibrary.Model;
using ArchitectureLibrary.Signals;
using GBATool.Models;
using GBATool.Signals;
using GBATool.Utils;
using GBATool.ViewModels;
using GBATool.VOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace GBATool.Views;

/// <summary>
/// Interaction logic for CharacterFrameEditorView.xaml
/// </summary>
public partial class CharacterFrameEditorView : UserControl
{
    private double _spriteOffsetX;
    private double _spriteOffsetY;
    private readonly Dictionary<Image, SpriteControlVO> _spritesInFrames = [];
    private readonly Thickness SelectionThickness = new(0.3, 0.1, 0.1, 0.3);
    private readonly List<Image> _onionSkinImages = [];
    private const double _onionSkinOpacity = 0.25;
    private Point _initialMousePositionInCanvas;

    public CharacterFrameEditorView()
    {
        InitializeComponent();
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        #region Signals
        SignalManager.Get<FillWithSpriteControlsSignal>().Listener += OnFillWithSpriteControls;
        SignalManager.Get<FillWithPreviousFrameSpriteControlsSignal>().Listener += OnFillWithPreviousFrameSpriteControls;
        SignalManager.Get<OptionOnionSkinSignal>().Listener += OnOptionOnionSkin;
        #endregion

        frameView.OnActivate();
        bankViewer.OnActivate();

        if (DataContext is not CharacterFrameEditorViewModel viewModel)
        {
            return;
        }

        frameView.canvas.Children.Clear();

        SignalManager.Get<SetBankModelToBankViewerSignal>().Dispatch(viewModel.BankModel);
        SignalManager.Get<CharacterFrameEditorViewLoadedSignal>().Dispatch();
    }

    private void UserControl_Unloaded(object sender, RoutedEventArgs e)
    {
        if (frameView.DataContext is not FrameView frameViewView)
        {
            return;
        }

        frameViewView.Canvas.Children.Clear();

        bankViewer.OnDeactivate();
        frameView.OnDeactivate();

        _spritesInFrames.Clear();
        _spriteOffsetX = 0;
        _spriteOffsetY = 0;

        #region Signals
        SignalManager.Get<FillWithSpriteControlsSignal>().Listener -= OnFillWithSpriteControls;
        SignalManager.Get<FillWithPreviousFrameSpriteControlsSignal>().Listener -= OnFillWithPreviousFrameSpriteControls;
        SignalManager.Get<OptionOnionSkinSignal>().Listener -= OnOptionOnionSkin;
        #endregion
    }

    private void BankViewer_MouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed)
        {
            return;
        }

        if (bankViewer.DataContext is not BankViewerView bankViewerView)
        {
            return;
        }

        if (bankViewerView.SelectedSpriteFromBank == null)
        {
            return;
        }

        if (bankViewerView.MetaData == null)
        {
            return;
        }

        if (DataContext is not CharacterFrameEditorViewModel viewModel)
        {
            return;
        }

        if (viewModel.BankModel == null)
        {
            return;
        }

        if (bankViewerView.MetaData.Sprites.TryGetValue(bankViewerView.SelectedSpriteFromBank.ID, out SpriteInfo? spriteInfo))
        {
            if (spriteInfo.BitmapSource == null)
            {
                return;
            }

            int width = 0;
            int height = 0;
            SpriteUtils.ConvertToWidthHeight(bankViewerView.SelectedSpriteFromBank.Shape, bankViewerView.SelectedSpriteFromBank.Size, ref width, ref height);

            SpriteControlVO spriteControl = new()
            {
                ID = "new",
                Image = Util.GetImageFromWriteableBitmap(spriteInfo.BitmapSource),
                SpriteID = bankViewerView.SelectedSpriteFromBank.ID,
                TileSetID = bankViewerView.SelectedSpriteFromBank.TileSetID,
                BankID = viewModel.BankModel.GUID,
                Width = width,
                Height = height,
                OffsetX = spriteInfo.OffsetX,
                OffsetY = spriteInfo.OffsetY
            };

            DataObject data = new(spriteControl);

            Point elementPosition = e.GetPosition(bankViewerView.Canvas);

            _spriteOffsetX = elementPosition.X - spriteInfo.OffsetX;
            _spriteOffsetY = elementPosition.Y - spriteInfo.OffsetY;

            DragDropEffects result = DragDrop.DoDragDrop((DependencyObject)e.Source, data, DragDropEffects.Move);

            if (result == DragDropEffects.None)
            {
                if (bankViewerView.Canvas.Children.Contains(spriteControl.Image))
                {
                    bankViewerView.Canvas.Children.Remove(spriteControl.Image);
                }
            }
        }
    }

    private void BankViewer_DragOver(object sender, DragEventArgs e)
    {
        if (bankViewer.DataContext is not BankViewerView bankViewerView)
        {
            return;
        }

        object data = e.Data.GetData(typeof(SpriteControlVO));

        SpriteControlVO sprite = (SpriteControlVO)data;

        if (sprite.Image == null)
            return;

        sprite.Image.IsHitTestVisible = false;

        Point elementPosition = e.GetPosition(bankViewerView.Canvas);

        if (!bankViewerView.Canvas.Children.Contains(sprite.Image))
        {
            _ = bankViewerView.Canvas.Children.Add(sprite.Image);
        }

        Canvas.SetLeft(sprite.Image, elementPosition.X - _spriteOffsetX);
        Canvas.SetTop(sprite.Image, elementPosition.Y - _spriteOffsetY);
    }

    private void BankViewer_DragLeave(object sender, DragEventArgs e)
    {
        if (e.OriginalSource is not Image)
        {
            return;
        }

        object data = e.Data.GetData(typeof(SpriteControlVO));

        SpriteControlVO sprite = (SpriteControlVO)data;

        if (sprite.Image == null)
        {
            return;
        }

        if (bankViewer.DataContext is BankViewerView bankViewerView)
        {
            bankViewerView.Canvas.Children.Remove(sprite.Image);
        }
    }

    private void BankViewer_Drop(object sender, DragEventArgs e)
    {
        if (bankViewer.DataContext is not BankViewerView bankViewerView)
        {
            return;
        }

        object data = e.Data.GetData(typeof(SpriteControlVO));

        SpriteControlVO sprite = (SpriteControlVO)data;

        // If the dragging object is comming from the FrameView then it needs to be removed from the Character model too
        if (sprite.Image != null)
        {
            if (_spritesInFrames.TryGetValue(sprite.Image, out SpriteControlVO? spriteControl))
            {
                SignalManager.Get<DeleteSpritesFromCharacterFrameSignal>().Dispatch([spriteControl.ID]);

                _ = _spritesInFrames.Remove(sprite.Image);
            }
        }

        if (bankViewerView.Canvas.Children.Contains(sprite.Image))
        {
            bankViewerView.Canvas.Children.Remove(sprite.Image);
        }
    }

    private void OnFillWithSpriteControls(List<SpriteControlVO> spriteVOList, string frameID)
    {
        if (DataContext is not CharacterFrameEditorViewModel viewModel)
        {
            return;
        }

        if (viewModel.FrameID != frameID)
        {
            return;
        }

        if (frameView.DataContext is not FrameView frameViewView)
        {
            return;
        }

        foreach (SpriteControlVO vo in spriteVOList)
        {
            SpriteControlVO? sprite = AddSpriteToFrameView(vo.ID, vo, vo.PositionX, vo.PositionY, vo.Image);

            if (sprite != null)
            {
                _ = frameViewView.Canvas.Children.Add(sprite.Image);
            }
        }
    }

    private void OnFillWithPreviousFrameSpriteControls(List<SpriteControlVO> spriteVOList, string animationID)
    {
        if (DataContext is not CharacterFrameEditorViewModel viewModel)
        {
            return;
        }

        if (viewModel.AnimationID != animationID)
        {
            return;
        }

        if (frameView.DataContext is not FrameView frameViewView)
        {
            return;
        }

        foreach (SpriteControlVO vo in spriteVOList)
        {
            if (vo.Image == null)
            {
                continue;
            }

            vo.Image.Opacity = ModelManager.Get<GBAToolConfigurationModel>().EnableOnionSkin ? _onionSkinOpacity : 0.0;

            Canvas.SetLeft(vo.Image, vo.PositionX);
            Canvas.SetTop(vo.Image, vo.PositionY);

            _ = frameViewView.Canvas.Children.Add(vo.Image);

            _onionSkinImages.Add(vo.Image);
        }
    }

    private void FrameView_Drop(object sender, DragEventArgs e)
    {
        if (frameView.DataContext is not FrameView frameViewView)
        {
            return;
        }

        object data = e.Data.GetData(typeof(SpriteControlVO));

        SpriteControlVO draggingSprite = (SpriteControlVO)data;

        if (frameViewView.Canvas.Children.Contains(draggingSprite.Image))
        {
            string id = draggingSprite.ID;

            if (id == "new")
            {
                id = Guid.NewGuid().ToString();
            }

            Image image = new() { Source = draggingSprite.Image?.Source };

            Point elementPosition = e.GetPosition(frameViewView.Canvas);

            int exactPosX = (int)(elementPosition.X - _spriteOffsetX);
            int exactPosY = (int)(elementPosition.Y - _spriteOffsetY);

            SpriteControlVO? sprite = AddSpriteToFrameView(id, draggingSprite, exactPosX, exactPosY, image);

            if (sprite != null)
            {
                _ = frameViewView.Canvas.Children.Add(sprite.Image);
            }

            // remove the previous instance of the Image control in the canvas now that there is a new one to replace it.
            if (draggingSprite.Image != null)
            {
                if (_spritesInFrames.TryGetValue(draggingSprite.Image, out _))
                {
                    _ = _spritesInFrames.Remove(draggingSprite.Image);
                }
            }

            frameViewView.Canvas.Children.Remove(draggingSprite.Image);

            SaveCharacterSpriteInformation(sprite, new Point(exactPosX, exactPosY), draggingSprite.BankID);
        }
    }

    private SpriteControlVO? AddSpriteToFrameView(string id, SpriteControlVO draggingSprite, int exactPosX, int exactPosY, Image? image)
    {
        if (image == null)
        {
            return null;
        }

        SpriteControlVO sprite = new()
        {
            ID = id,
            Image = image,
            SpriteID = draggingSprite.SpriteID,
            TileSetID = draggingSprite.TileSetID,
            BankID = draggingSprite.BankID,
            Width = draggingSprite.Width,
            Height = draggingSprite.Height,
            OffsetX = draggingSprite.OffsetX,
            OffsetY = draggingSprite.OffsetY
        };

        Canvas.SetLeft(sprite.Image, exactPosX);
        Canvas.SetTop(sprite.Image, exactPosY);

        if (sprite.Image != null)
        {
            if (!_spritesInFrames.TryGetValue(sprite.Image, out _))
            {
                _spritesInFrames.Add(sprite.Image, sprite);
            }
        }

        return sprite;
    }

    private static void SaveCharacterSpriteInformation(SpriteControlVO? sprite, Point position, string bankID)
    {
        if (sprite == null || sprite.Image == null || string.IsNullOrEmpty(bankID))
        {
            return;
        }

        CharacterSprite characterSprite = new()
        {
            ID = sprite.ID,
            Position = position,
            FlipHorizontal = false,
            FlipVertical = false,
            SpriteID = sprite.SpriteID,
            TileSetID = sprite.TileSetID,
            Width = sprite.Width,
            Height = sprite.Height,
        };

        SignalManager.Get<AddOrUpdateSpriteIntoCharacterFrameSignal>().Dispatch(characterSprite, bankID);
    }

    private void FrameView_DragOver(object sender, DragEventArgs e)
    {
        if (frameView.DataContext is not FrameView frameViewView)
        {
            return;
        }

        object data = e.Data.GetData(typeof(SpriteControlVO));

        SpriteControlVO draggingSprite = (SpriteControlVO)data;

        if (draggingSprite.Image == null)
        {
            return;
        }

        Point elementPosition = e.GetPosition(frameViewView.Canvas);

        if (!frameViewView.Canvas.Children.Contains(draggingSprite.Image))
        {
            draggingSprite.Image.IsHitTestVisible = false;

            _ = frameViewView.Canvas.Children.Add(draggingSprite.Image);
        }

        int exactPosX = (int)(elementPosition.X - _spriteOffsetX);
        int exactPosY = (int)(elementPosition.Y - _spriteOffsetY);

        Canvas.SetLeft(draggingSprite.Image, exactPosX);
        Canvas.SetTop(draggingSprite.Image, exactPosY);
    }

    private void FrameView_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.OriginalSource is not Canvas and not Image)
        {
            return;
        }

        if (frameView.DataContext is not FrameView frameViewView)
        {
            return;
        }

        frameViewView.canvas.CaptureMouse();

        _initialMousePositionInCanvas = e.GetPosition(frameViewView.canvas);

        frameViewView.MouseSelectionActive = Visibility.Hidden;
        frameViewView.MouseSelectionOriginX = (int)_initialMousePositionInCanvas.X;
        frameViewView.MouseSelectionOriginY = (int)_initialMousePositionInCanvas.Y;
        frameViewView.MouseSelectionWidth = 0;
        frameViewView.MouseSelectionHeight = 0;

        List<SpriteControlVO> spriteControls = [];

        List<Image> images = GetSelectionMouseOver(frameViewView.canvas, _initialMousePositionInCanvas);

        if (images.Count > 0)
        {
            if (_spritesInFrames.TryGetValue(images.First(), out SpriteControlVO? spriteControl))
            {
                if (spriteControl != null)
                {
                    spriteControls.Add(spriteControl);
                }
            }
        }

        SignalManager.Get<SelectFrameSpritesSignal>().Dispatch([.. spriteControls]);
    }

    private void FrameView_MouseUp(object sender, MouseButtonEventArgs e)
    {
        if (e.OriginalSource is not Canvas and not Image)
        {
            return;
        }

        if (DataContext is not CharacterFrameEditorViewModel viewModel)
        {
            return;
        }

        if (frameView.DataContext is not FrameView frameViewView)
        {
            return;
        }

        List<SpriteControlVO> selectedSprites = [];

        frameViewView.canvas.ReleaseMouseCapture();

        if (frameViewView.MouseSelectionActive == Visibility.Visible)
        {
            selectedSprites = CheckMouseAreaSelected(frameViewView);
        }

        frameViewView.MouseSelectionActive = Visibility.Hidden;
        frameViewView.MouseSelectionWidth = 0;
        frameViewView.MouseSelectionHeight = 0;

        if (selectedSprites.Count == 0)
        {
            // Check if there is a selected image

            CanvasHitDetection<Image, EllipseGeometry> hitDetection = new(new(e.GetPosition(frameViewView.canvas), 1.0, 1.0), frameViewView.canvas);
            List<Image> hitList = hitDetection.HitTest();

            if (hitList.Count > 0)
            {
                // Clicking on only one image

                if (_spritesInFrames.TryGetValue(hitList.First(), out SpriteControlVO? sprite))
                {
                    string? foundID = Array.Find(viewModel.SelectedFrameSprite, e => e == sprite.ID);

                    if (string.IsNullOrEmpty(foundID))
                    {
                        selectedSprites.Add(sprite);
                    }
                }
            }
        }

        SignalManager.Get<SelectFrameSpritesSignal>().Dispatch([.. selectedSprites]);
    }

    private static List<Image> GetSelectionMouseOver(Canvas canvas, Point positionInCanvas)
    {
        CanvasHitDetection<Image, EllipseGeometry> hitDetection = new(new(positionInCanvas, 1.0, 1.0), canvas);
        List<Image> hitList = hitDetection.HitTest();

        return hitList;
    }

    private List<SpriteControlVO> CheckMouseAreaSelected(FrameView frameViewView)
    {
        List<SpriteControlVO> sprites = [];

        if (frameViewView.MouseSelectionWidth == 0 ||
            frameViewView.MouseSelectionHeight == 0)
        {
            return sprites;
        }

        Rect rectangle = new(
            frameViewView.MouseSelectionOriginX,
            frameViewView.MouseSelectionOriginY,
            frameViewView.MouseSelectionWidth,
            frameViewView.MouseSelectionHeight);

        CanvasHitDetection<Image, RectangleGeometry> hitDetection = new(new(rectangle), frameViewView.canvas);
        List<Image> hitList = hitDetection.HitTest();

        if (hitList.Count > 0)
        {
            foreach (Image item in hitList)
            {
                if (_spritesInFrames.TryGetValue(item, out SpriteControlVO? sprite))
                {
                    sprites.Add(sprite);
                }
            }
        }

        return sprites;
    }

    private void FrameView_DragLeave(object sender, DragEventArgs e)
    {
        if (e.OriginalSource is not Canvas and not Image)
        {
            return;
        }

        if (frameView.DataContext is not FrameView frameViewView)
        {
            return;
        }

        object data = e.Data.GetData(typeof(SpriteControlVO));

        SpriteControlVO sprite = (SpriteControlVO)data;

        if (sprite.Image != null)
        {
            frameViewView.Canvas.Children.Remove(sprite.Image);
        }
    }

    private void FrameView_MouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed)
        {
            return;
        }

        if (frameView.DataContext is not FrameView frameViewView)
        {
            return;
        }

        if (DataContext is not CharacterFrameEditorViewModel viewModel)
        {
            return;
        }

        Point positionInCanvas = e.GetPosition(frameViewView.canvas);

        if (viewModel.SelectedFrameSprite.Length > 0)
        {
            DragImages(frameViewView.canvas, positionInCanvas, viewModel.SelectedFrameSprite, (DependencyObject)e.Source);
        }
        else
        {
            UpdateMouseSelectionArea(frameViewView, positionInCanvas);
        }
    }

    private void DragImages(Canvas canvas, Point positionInCanvas, string[] selectedFrameSprites, DependencyObject dragSource)
    {
        List<Image> images = GetSelectionMouseOver(canvas, positionInCanvas);

        if (images.Count == 0)
        {
            return;
        }

        if (!_spritesInFrames.TryGetValue(images.First(), out SpriteControlVO? leadControl))
        {
            return;
        }

        if (leadControl == null)
        {
            return;
        }

        if (selectedFrameSprites.Length == 1)
        {
            DataObject data = new(leadControl);

            double imagePosX = Canvas.GetLeft(leadControl.Image);
            double imagePosY = Canvas.GetTop(leadControl.Image);

            _spriteOffsetX = positionInCanvas.X - imagePosX;
            _spriteOffsetY = positionInCanvas.Y - imagePosY;

            DragDropEffects result = DragDrop.DoDragDrop(dragSource, data, DragDropEffects.Move);

            if (result == DragDropEffects.None)
            {
                SignalManager.Get<DeleteSpritesFromCharacterFrameSignal>().Dispatch([leadControl.ID]);

                _ = _spritesInFrames.Remove(images.First());
            }
        }
        else
        {
            // Move when there are move than one selected
        }
    }

    private void UpdateMouseSelectionArea(FrameView frameViewView, Point positionInCanvas)
    {
        if (frameViewView.MouseSelectionActive == Visibility.Hidden)
        {
            frameViewView.MouseSelectionActive = Visibility.Visible;
        }

        if (_initialMousePositionInCanvas.X < positionInCanvas.X)
        {
            frameViewView.MouseSelectionOriginX = (int)_initialMousePositionInCanvas.X;
            frameViewView.MouseSelectionWidth = (int)(positionInCanvas.X - _initialMousePositionInCanvas.X);
        }
        else
        {
            frameViewView.MouseSelectionOriginX = (int)positionInCanvas.X;
            frameViewView.MouseSelectionWidth = (int)(_initialMousePositionInCanvas.X - positionInCanvas.X);
        }

        if (_initialMousePositionInCanvas.Y < positionInCanvas.Y)
        {
            frameViewView.MouseSelectionOriginY = (int)(_initialMousePositionInCanvas.Y);
            frameViewView.MouseSelectionHeight = (int)(positionInCanvas.Y - _initialMousePositionInCanvas.Y);
        }
        else
        {
            frameViewView.MouseSelectionOriginY = (int)(positionInCanvas.Y);
            frameViewView.MouseSelectionHeight = (int)(_initialMousePositionInCanvas.Y - positionInCanvas.Y);
        }
    }

    private void OnOptionOnionSkin(bool enabledOnionSkin)
    {
        foreach (Image image in _onionSkinImages)
        {
            image.Opacity = enabledOnionSkin ? _onionSkinOpacity : 0.0;
        }
    }
}
