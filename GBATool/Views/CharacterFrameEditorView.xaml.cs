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
    private readonly Dictionary<Image, SpriteControlVO> _spritesInFrames = [];
    private readonly List<Image> _onionSkinImages = [];
    private const double _onionSkinOpacity = 0.25;
    private Point _initialMousePositionInCanvas;
    private string _bankID = string.Empty;

    public CharacterFrameEditorView()
    {
        InitializeComponent();
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        #region Signals
        SignalManager.Get<LoadWithSpriteControlsSignal>().Listener += OnFillWithSpriteControls;
        SignalManager.Get<FillWithPreviousFrameSpriteControlsSignal>().Listener += OnFillWithPreviousFrameSpriteControls;
        SignalManager.Get<OptionOnionSkinSignal>().Listener += OnOptionOnionSkin;
        SignalManager.Get<UpdateSpriteVisualPropertiesSignal>().Listener += OnUpdateSpriteVisualProperties;
        SignalManager.Get<UpdateCollisionViewSignal>().Listener += OnUpdateCollisionView;
        #endregion

        frameView.OnActivate();
        bankViewer.OnActivate();

        if (DataContext is not CharacterFrameEditorViewModel viewModel)
        {
            return;
        }

        frameView.Canvas.Children.Clear();

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

        #region Signals
        SignalManager.Get<LoadWithSpriteControlsSignal>().Listener -= OnFillWithSpriteControls;
        SignalManager.Get<FillWithPreviousFrameSpriteControlsSignal>().Listener -= OnFillWithPreviousFrameSpriteControls;
        SignalManager.Get<OptionOnionSkinSignal>().Listener -= OnOptionOnionSkin;
        SignalManager.Get<UpdateSpriteVisualPropertiesSignal>().Listener -= OnUpdateSpriteVisualProperties;
        SignalManager.Get<UpdateCollisionViewSignal>().Listener -= OnUpdateCollisionView;
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

            Point elementPosition = e.GetPosition(bankViewerView.Canvas);

            List<CharacterDragObjectVO> arrayOfDataObjects = [];
            arrayOfDataObjects.Add(new CharacterDragObjectVO(spriteControl, elementPosition.X - spriteInfo.OffsetX, elementPosition.Y - spriteInfo.OffsetY));

            DataObject data = new(arrayOfDataObjects);

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

        Point mousePosition = e.GetPosition(bankViewerView.Canvas);

        object data = e.Data.GetData(typeof(List<CharacterDragObjectVO>));

        List<CharacterDragObjectVO> list = (List<CharacterDragObjectVO>)data;

        foreach (CharacterDragObjectVO item in list)
        {
            if (item.SpriteControl.Image == null)
            {
                continue;
            }

            item.SpriteControl.Image.IsHitTestVisible = false;

            if (!bankViewerView.Canvas.Children.Contains(item.SpriteControl.Image))
            {
                _ = bankViewerView.Canvas.Children.Add(item.SpriteControl.Image);
            }

            int exactPosX = (int)(mousePosition.X - item.SpriteOffsetX);
            int exactPosY = (int)(mousePosition.Y - item.SpriteOffsetY);

            Canvas.SetLeft(item.SpriteControl.Image, exactPosX);
            Canvas.SetTop(item.SpriteControl.Image, exactPosY);
        }
    }

    private void BankViewer_DragLeave(object sender, DragEventArgs e)
    {
        if (e.OriginalSource is not Image)
        {
            return;
        }

        if (bankViewer.DataContext is not BankViewerView bankViewerView)
        {
            return;
        }

        object data = e.Data.GetData(typeof(List<CharacterDragObjectVO>));

        List<CharacterDragObjectVO> list = (List<CharacterDragObjectVO>)data;

        foreach (CharacterDragObjectVO item in list)
        {
            if (item.SpriteControl.Image == null)
            {
                continue;
            }

            bankViewerView.Canvas.Children.Remove(item.SpriteControl.Image);
        }
    }

    private void BankViewer_Drop(object sender, DragEventArgs e)
    {
        if (bankViewer.DataContext is not BankViewerView bankViewerView)
        {
            return;
        }

        object data = e.Data.GetData(typeof(List<CharacterDragObjectVO>));

        List<CharacterDragObjectVO> list = (List<CharacterDragObjectVO>)data;

        foreach (CharacterDragObjectVO item in list)
        {
            // If the dragging object is comming from the FrameView then it needs to be removed from the Character model too
            if (item.SpriteControl.Image != null)
            {
                if (_spritesInFrames.TryGetValue(item.SpriteControl.Image, out SpriteControlVO? spriteControl))
                {
                    SignalManager.Get<DeleteSpritesFromCharacterFrameSignal>().Dispatch([spriteControl.ID]);

                    _ = _spritesInFrames.Remove(item.SpriteControl.Image);
                }
            }

            if (bankViewerView.Canvas.Children.Contains(item.SpriteControl.Image))
            {
                bankViewerView.Canvas.Children.Remove(item.SpriteControl.Image);
            }
        }
    }

    private void OnUpdateCollisionView()
    {
        lvFrameCollisions.Items.Refresh();
    }

    private void OnFillWithSpriteControls(List<SpriteControlVO> spriteVOList, string frameID, string bankID)
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

        if (spriteVOList.Count > 0)
        {
            _bankID = bankID;
        }

        foreach (SpriteControlVO vo in spriteVOList)
        {
            SpriteControlVO? sprite = AddSpriteToFrameView(vo.ID, vo, vo.PositionX, vo.PositionY, vo.Image);

            if (sprite != null && sprite.Image != null)
            {
                TransformImage(sprite.Image, vo.FlipHorizontal, vo.FlipVertical);

                _ = frameViewView.Canvas.Children.Add(sprite.Image);
            }
        }
    }

    private static void TransformImage(Image? image, bool flipHorizontal, bool flipVertical)
    {
        if (image == null)
            return;

        image.RenderTransformOrigin = new Point(0.5, 0.5);

        ScaleTransform flipTrans = new();

        if (flipHorizontal)
        {
            flipTrans.ScaleX = -1;
        }

        if (flipVertical)
        {
            flipTrans.ScaleY = -1;
        }

        image.RenderTransform = flipTrans;
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

            TransformImage(vo.Image, vo.FlipHorizontal, vo.FlipVertical);

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

        object data = e.Data.GetData(typeof(List<CharacterDragObjectVO>));

        List<CharacterDragObjectVO> list = (List<CharacterDragObjectVO>)data;

        foreach (CharacterDragObjectVO item in list)
        {
            if (!string.IsNullOrEmpty(_bankID) && item.SpriteControl.BankID != _bankID)
            {
                _ = MessageBox.Show("For now it is not possible to have a frame with sprites from two or more different banks", "Error", MessageBoxButton.OK);

                RemoveFromFrameViewTempImage(frameViewView, item);
                return;
            }

            if (string.IsNullOrEmpty(_bankID))
            {
                _bankID = item.SpriteControl.BankID;
            }

            if (frameViewView.Canvas.Children.Contains(item.SpriteControl.Image))
            {
                string id = item.SpriteControl.ID;

                if (id == "new")
                {
                    id = Guid.NewGuid().ToString();
                }

                Image image = new() { Source = item.SpriteControl.Image?.Source };

                Point elementPosition = e.GetPosition(frameViewView.Canvas);

                int exactPosX = (int)(elementPosition.X - item.SpriteOffsetX);
                int exactPosY = (int)(elementPosition.Y - item.SpriteOffsetY);

                SpriteControlVO? sprite = AddSpriteToFrameView(id, item.SpriteControl, exactPosX, exactPosY, image);

                if (sprite != null)
                {
                    TransformImage(sprite.Image, item.SpriteControl.FlipHorizontal, item.SpriteControl.FlipVertical);

                    _ = frameViewView.Canvas.Children.Add(sprite.Image);
                }

                // remove the previous instance of the Image control in the canvas now that there is a new one to replace it.
                RemoveFromFrameViewTempImage(frameViewView, item);

                SaveCharacterSpriteInformation(sprite, new Point(exactPosX, exactPosY), item.SpriteControl.BankID);
            }
        }
    }

    private void RemoveFromFrameViewTempImage(FrameView frameViewView, CharacterDragObjectVO item)
    {
        if (item.SpriteControl.Image == null)
        {
            return;
        }

        if (_spritesInFrames.TryGetValue(item.SpriteControl.Image, out _))
        {
            _ = _spritesInFrames.Remove(item.SpriteControl.Image);
        }

        frameViewView.Canvas.Children.Remove(item.SpriteControl.Image);

        if (_spritesInFrames.Count == 0)
        {
            _bankID = string.Empty;
        }
    }

    private void OnUpdateSpriteVisualProperties(string spriteID, bool horizontalFlip, bool verticalFlip)
    {
        if (frameView.DataContext is not FrameView frameViewView)
        {
            return;
        }

        foreach (KeyValuePair<Image, SpriteControlVO> item in _spritesInFrames)
        {
            if (item.Value.SpriteID != spriteID)
            {
                continue;
            }

            foreach (object child in frameViewView.Canvas.Children)
            {
                if (child is not Image image)
                {
                    continue;
                }

                if (image == item.Key)
                {
                    TransformImage(image, horizontalFlip, verticalFlip);

                    _spritesInFrames[image].FlipHorizontal = horizontalFlip;
                    _spritesInFrames[image].FlipVertical = verticalFlip;

                    return;
                }
            }
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
            OffsetY = draggingSprite.OffsetY,
            FlipHorizontal = draggingSprite.FlipHorizontal,
            FlipVertical = draggingSprite.FlipVertical
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
            FlipHorizontal = sprite.FlipHorizontal,
            FlipVertical = sprite.FlipVertical,
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

        Point mousePosition = e.GetPosition(frameViewView.Canvas);

        object data = e.Data.GetData(typeof(List<CharacterDragObjectVO>));

        List<CharacterDragObjectVO> list = (List<CharacterDragObjectVO>)data;

        foreach (CharacterDragObjectVO item in list)
        {
            if (item.SpriteControl.Image == null)
            {
                continue;
            }

            if (!frameViewView.Canvas.Children.Contains(item.SpriteControl.Image))
            {
                item.SpriteControl.Image.IsHitTestVisible = false;

                _ = frameViewView.Canvas.Children.Add(item.SpriteControl.Image);
            }

            int exactPosX = (int)(mousePosition.X - item.SpriteOffsetX);
            int exactPosY = (int)(mousePosition.Y - item.SpriteOffsetY);

            Canvas.SetLeft(item.SpriteControl.Image, exactPosX);
            Canvas.SetTop(item.SpriteControl.Image, exactPosY);
        }
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

        if (DataContext is not CharacterFrameEditorViewModel viewModel)
        {
            return;
        }

        _initialMousePositionInCanvas = e.GetPosition(frameViewView.Canvas);

        SignalManager.Get<ResetFrameSpritesSelectionAreaSignal>().Dispatch(_initialMousePositionInCanvas);

        List<SpriteControlVO> selectedSprites = [];

        GetListSpriteControlSelected(frameViewView.Canvas, _initialMousePositionInCanvas, ref selectedSprites);

        bool isOneOfThePreviouslySelected = false;

        foreach (SpriteControlVO spriteVO in selectedSprites)
        {
            if (viewModel.SelectedFrameSprites.Contains(spriteVO.ID))
            {
                isOneOfThePreviouslySelected = true;
            }
        }

        if (viewModel.SelectedFrameSprites.Length == 0 ||
            !isOneOfThePreviouslySelected)
        {
            SignalManager.Get<SelectFrameSpritesSignal>().Dispatch([.. selectedSprites]);
        }
    }

    private void FrameView_MouseUp(object sender, MouseButtonEventArgs e)
    {
        if (e.OriginalSource is not Canvas and not Image)
        {
            return;
        }

        if (frameView.DataContext is not FrameView frameViewView)
        {
            return;
        }

        if (frameViewView.Canvas.IsMouseCaptured)
        {
            frameViewView.Canvas.ReleaseMouseCapture();
        }

        List<SpriteControlVO> selectedSprites = [];

        if (frameViewView.MouseSelectionActive == Visibility.Visible)
        {
            selectedSprites = CheckMouseAreaSelected(frameViewView);
        }

        Point pos = e.GetPosition(frameViewView.Canvas);

        SignalManager.Get<ResetFrameSpritesSelectionAreaSignal>().Dispatch(pos);

        if (selectedSprites.Count == 0)
        {
            GetListSpriteControlSelected(frameViewView.Canvas, pos, ref selectedSprites);
        }

        SignalManager.Get<SelectFrameSpritesSignal>().Dispatch([.. selectedSprites]);
    }

    private void GetListSpriteControlSelected(Canvas canvas, Point position, ref List<SpriteControlVO> selectedSprites)
    {
        List<Image> images = GetSelectionMouseOver(canvas, position);

        if (images.Count > 0)
        {
            if (_spritesInFrames.TryGetValue(images.First(), out SpriteControlVO? spriteControl))
            {
                if (spriteControl != null)
                {
                    selectedSprites.Add(spriteControl);
                }
            }
        }
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

        CanvasHitDetection<Image, RectangleGeometry> hitDetection = new(new(rectangle), frameViewView.Canvas);
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

        object data = e.Data.GetData(typeof(List<CharacterDragObjectVO>));

        List<CharacterDragObjectVO> list = (List<CharacterDragObjectVO>)data;

        foreach (CharacterDragObjectVO item in list)
        {
            RemoveFromFrameViewTempImage(frameViewView, item);
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

        Point positionInCanvas = e.GetPosition(frameViewView.Canvas);

        if (Util.AboutEqual(positionInCanvas.X, _initialMousePositionInCanvas.X) &&
            Util.AboutEqual(positionInCanvas.Y, _initialMousePositionInCanvas.Y))
        {
            return;
        }

        if (viewModel.SelectedFrameSprites.Length > 0)
        {
            DragImages(positionInCanvas, viewModel.SelectedFrameSprites, (DependencyObject)e.Source);
        }
        else
        {
            if (!frameViewView.Canvas.IsMouseCaptured)
            {
                frameViewView.Canvas.CaptureMouse();
            }

            UpdateMouseSelectionArea(frameViewView, positionInCanvas);
        }
    }

    private void DragImages(Point positionInCanvas, string[] selectedFrameSprites, DependencyObject dragSource)
    {
        List<CharacterDragObjectVO> characterDragObjects = [];

        for (int i = 0; i < selectedFrameSprites.Length; i++)
        {
            string spriteID = selectedFrameSprites[i];

            foreach (KeyValuePair<Image, SpriteControlVO> item in _spritesInFrames)
            {
                if (item.Value.ID != spriteID)
                {
                    continue;
                }

                double imagePosX = Canvas.GetLeft(item.Value.Image);
                double imagePosY = Canvas.GetTop(item.Value.Image);

                characterDragObjects.Add(new(item.Value, positionInCanvas.X - imagePosX, positionInCanvas.Y - imagePosY));

                break;
            }
        }

        if (characterDragObjects.Count == 0)
        {
            return;
        }

        DataObject data = new(characterDragObjects);

        SignalManager.Get<SpriteFrameHideSelectionSignal>().Dispatch();

        DragDropEffects result = DragDrop.DoDragDrop(dragSource, data, DragDropEffects.Move);

        if (result == DragDropEffects.None)
        {
            string[] ids = [.. characterDragObjects.Select(o => o.SpriteControl.ID)];

            SignalManager.Get<DeleteSpritesFromCharacterFrameSignal>().Dispatch(ids);

            Dictionary<Image, SpriteControlVO> spritesInFramesTmp = [];

            foreach (KeyValuePair<Image, SpriteControlVO> item in _spritesInFrames)
            {
                if (!ids.Contains(item.Value.ID))
                {
                    spritesInFramesTmp.Add(item.Key, item.Value);
                }
            }

            _spritesInFrames.Clear();

            foreach (KeyValuePair<Image, SpriteControlVO> item in spritesInFramesTmp)
            {
                _spritesInFrames.Add(item.Key, item.Value);
            }
        }
        else
        {
            SignalManager.Get<SpriteFrameShowSelectionSignal>().Dispatch([.. characterDragObjects]);
        }
    }

    private void UpdateMouseSelectionArea(FrameView frameViewView, Point positionInCanvas)
    {
        if (frameViewView.MouseSelectionActive == Visibility.Collapsed)
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
