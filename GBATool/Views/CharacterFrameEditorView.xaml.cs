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
using System.Windows.Shapes;

namespace GBATool.Views;

/// <summary>
/// Interaction logic for CharacterFrameEditorView.xaml
/// </summary>
public partial class CharacterFrameEditorView : UserControl
{
    private readonly Dictionary<Image, SpriteControlVO> _spritesInFrames = [];
    private readonly List<Image> _onionSkinImages = [];
    private readonly Dictionary<Rectangle, CollisionControlVO> _collisionsInFrame = [];
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
        SignalManager.Get<LoadWithCollisionControlsSignal>().Listener += OnLoadWithCollisionControls;
        SignalManager.Get<FillWithPreviousFrameSpriteControlsSignal>().Listener += OnFillWithPreviousFrameSpriteControls;
        SignalManager.Get<OptionOnionSkinSignal>().Listener += OnOptionOnionSkin;
        SignalManager.Get<UpdateSpriteVisualPropertiesSignal>().Listener += OnUpdateSpriteVisualProperties;
        SignalManager.Get<UpdateCollisionViewSignal>().Listener += OnUpdateCollisionView;
        SignalManager.Get<NewCollisionIntoSpriteSignal>().Listener += OnNewCollisionIntoSprite;
        SignalManager.Get<DeleteCollisionSignal>().Listener += OnDeleteCollision;
        SignalManager.Get<UpdateSpriteCollisionInfoSignal>().Listener += OnUpdateSpriteCollisionInfo;
        #endregion

        frameView.OnActivate();
        bankViewer.OnActivate();

        if (DataContext is not CharacterFrameEditorViewModel viewModel)
        {
            return;
        }

        frameView.FrameCanvas.Children.Clear();
        frameView.OnionCanvas.Children.Clear();
        frameView.CollisionCanvas.Children.Clear();

        SignalManager.Get<SetBankModelToBankViewerSignal>().Dispatch(viewModel.BankModel);
        SignalManager.Get<CharacterFrameEditorViewLoadedSignal>().Dispatch();
    }

    private void UserControl_Unloaded(object sender, RoutedEventArgs e)
    {
        if (frameView.DataContext is not FrameView frameViewView)
        {
            return;
        }

        frameViewView.FrameCanvas.Children.Clear();
        frameViewView.OnionCanvas.Children.Clear();
        frameViewView.CollisionCanvas.Children.Clear();

        bankViewer.OnDeactivate();
        frameView.OnDeactivate();

        _spritesInFrames.Clear();
        _collisionsInFrame.Clear();

        #region Signals
        SignalManager.Get<LoadWithSpriteControlsSignal>().Listener -= OnFillWithSpriteControls;
        SignalManager.Get<LoadWithCollisionControlsSignal>().Listener -= OnLoadWithCollisionControls;
        SignalManager.Get<FillWithPreviousFrameSpriteControlsSignal>().Listener -= OnFillWithPreviousFrameSpriteControls;
        SignalManager.Get<OptionOnionSkinSignal>().Listener -= OnOptionOnionSkin;
        SignalManager.Get<UpdateSpriteVisualPropertiesSignal>().Listener -= OnUpdateSpriteVisualProperties;
        SignalManager.Get<UpdateCollisionViewSignal>().Listener -= OnUpdateCollisionView;
        SignalManager.Get<NewCollisionIntoSpriteSignal>().Listener -= OnNewCollisionIntoSprite;
        SignalManager.Get<DeleteCollisionSignal>().Listener -= OnDeleteCollision;
        SignalManager.Get<UpdateSpriteCollisionInfoSignal>().Listener -= OnUpdateSpriteCollisionInfo;
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

            List<FrameElementDragObjectVO> arrayOfDataObjects = [];
            arrayOfDataObjects.Add(new FrameElementDragObjectVO(spriteControl, elementPosition.X - spriteInfo.OffsetX, elementPosition.Y - spriteInfo.OffsetY));

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

        object data = e.Data.GetData(typeof(List<FrameElementDragObjectVO>));

        List<FrameElementDragObjectVO> list = (List<FrameElementDragObjectVO>)data;

        foreach (FrameElementDragObjectVO item in list)
        {
            if (item.DragableObject is SpriteControlVO spriteVO)
            {
                if (spriteVO.Image == null)
                {
                    continue;
                }

                spriteVO.Image.IsHitTestVisible = false;

                if (!bankViewerView.Canvas.Children.Contains(spriteVO.Image))
                {
                    _ = bankViewerView.Canvas.Children.Add(spriteVO.Image);
                }

                int exactPosX = (int)(mousePosition.X - item.OffsetX);
                int exactPosY = (int)(mousePosition.Y - item.OffsetY);

                Canvas.SetLeft(spriteVO.Image, exactPosX);
                Canvas.SetTop(spriteVO.Image, exactPosY);
            }
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

        object data = e.Data.GetData(typeof(List<FrameElementDragObjectVO>));

        List<FrameElementDragObjectVO> list = (List<FrameElementDragObjectVO>)data;

        foreach (FrameElementDragObjectVO item in list)
        {
            if (item.DragableObject is SpriteControlVO spriteVO)
            {
                if (spriteVO.Image == null)
                {
                    continue;
                }

                bankViewerView.Canvas.Children.Remove(spriteVO.Image);
            }
        }
    }

    private void BankViewer_Drop(object sender, DragEventArgs e)
    {
        if (bankViewer.DataContext is not BankViewerView bankViewerView)
        {
            return;
        }

        object data = e.Data.GetData(typeof(List<FrameElementDragObjectVO>));

        List<FrameElementDragObjectVO> list = (List<FrameElementDragObjectVO>)data;

        foreach (FrameElementDragObjectVO item in list)
        {
            if (item.DragableObject is SpriteControlVO spriteVO)
            {
                // If the dragging object is comming from the FrameView then it needs to be removed from the Character model too
                if (spriteVO.Image != null)
                {
                    if (_spritesInFrames.TryGetValue(spriteVO.Image, out SpriteControlVO? spriteControl))
                    {
                        SignalManager.Get<DeleteElementsFromCharacterFrameSignal>().Dispatch([spriteControl.ID]);

                        _ = _spritesInFrames.Remove(spriteVO.Image);
                    }
                }

                if (bankViewerView.Canvas.Children.Contains(spriteVO.Image))
                {
                    bankViewerView.Canvas.Children.Remove(spriteVO.Image);
                }
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
            if (vo.Image == null)
            {
                continue;
            }

            SpriteControlVO sprite = AddSpriteToFrameView(vo.ID, vo, vo.PositionX, vo.PositionY, vo.Image);

            TransformImage(sprite.Image, vo.FlipHorizontal, vo.FlipVertical);

            _ = frameViewView.FrameCanvas.Children.Add(sprite.Image);
        }
    }

    private void OnUpdateSpriteCollisionInfo(SpriteCollisionVO collision)
    {
        if (DataContext is not CharacterFrameEditorViewModel viewModel)
        {
            return;
        }

        if (viewModel.AnimationID != collision.AnimationID ||
            viewModel.FrameID != collision.FrameID)
        {
            return;
        }

        foreach (KeyValuePair<Rectangle, CollisionControlVO> item in _collisionsInFrame)
        {
            if (item.Value.ID == collision.ID)
            {
                if (item.Value.Rectangle == null)
                {
                    return;
                }

                item.Value.Rectangle.Fill = collision.Color;
                item.Value.Rectangle.Width = collision.Width;
                item.Value.Rectangle.Height = collision.Height;

                CollisionControlVO updatedVO = new()
                {
                    ID = item.Value.ID,
                    Rectangle = item.Value.Rectangle,
                    Width = collision.Width,
                    Height = collision.Height,
                    PositionX = collision.PosX,
                    PositionY = collision.PosY,
                    Color = collision.Color,
                    Mask = collision.Mask,
                    CustomMask = collision.CustomMask,
                    AnimationID = collision.AnimationID,
                    FrameID = collision.FrameID
                };

                Canvas.SetLeft(item.Value.Rectangle, collision.PosX);
                Canvas.SetTop(item.Value.Rectangle, collision.PosY);

                _collisionsInFrame[item.Value.Rectangle] = updatedVO;
            }
        }
    }

    private void OnDeleteCollision(string animationID, string frameID, string collisionID)
    {
        if (DataContext is not CharacterFrameEditorViewModel viewModel)
        {
            return;
        }

        if (frameView.DataContext is not FrameView frameViewView)
        {
            return;
        }

        if (viewModel.AnimationID != animationID ||
            viewModel.FrameID != frameID)
        {
            return;
        }

        foreach (KeyValuePair<Rectangle, CollisionControlVO> item in _collisionsInFrame)
        {
            if (item.Value.ID == collisionID)
            {
                if (item.Value.Rectangle == null)
                    return;

                frameViewView.CollisionCanvas.Children.Remove(item.Value.Rectangle);

                _collisionsInFrame.Remove(item.Value.Rectangle);
            }
        }
    }

    private void OnNewCollisionIntoSprite(string animationID, string frameID, SpriteCollisionVO collisionVO)
    {
        if (DataContext is not CharacterFrameEditorViewModel viewModel)
        {
            return;
        }

        if (viewModel.AnimationID != animationID ||
            viewModel.FrameID != frameID)
        {
            return;
        }

        AddCollisionToCanvas(collisionVO);
    }

    private void OnLoadWithCollisionControls(List<SpriteCollisionVO> collisions, string frameID)
    {
        if (DataContext is not CharacterFrameEditorViewModel viewModel)
        {
            return;
        }

        if (viewModel.FrameID != frameID)
        {
            return;
        }

        foreach (SpriteCollisionVO item in collisions)
        {
            AddCollisionToCanvas(item);
        }
    }

    private void AddCollisionToCanvas(SpriteCollisionVO item)
    {
        if (frameView.DataContext is not FrameView frameViewView)
        {
            return;
        }

        SolidColorBrush brush = new(Color.FromArgb(item.Color.Color.A, item.Color.Color.R, item.Color.Color.G, item.Color.Color.B));

        Rectangle rect = new()
        {
            StrokeThickness = 0,
            Fill = brush,
            Opacity = 0.4,
            Width = item.Width,
            Height = item.Height,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
        };

        CollisionControlVO collision = new()
        {
            Rectangle = rect,
            ID = item.ID,
            Height = item.Height,
            Width = item.Width,
            PositionX = item.PosX,
            PositionY = item.PosY,
            Color = item.Color,
            Mask = item.Mask,
            CustomMask = item.CustomMask,
            AnimationID = item.AnimationID,
            FrameID = item.FrameID
        };

        Canvas.SetLeft(rect, item.PosX);
        Canvas.SetTop(rect, item.PosY);

        _collisionsInFrame.Add(rect, collision);

        _ = frameViewView.CollisionCanvas.Children.Add(rect);
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

            vo.Image.Opacity = ModelManager.Get<GBAToolConfigurationModel>().EnableOnionSkin ? ModelManager.Get<GBAToolConfigurationModel>().OnionSkinOpacity : 0.0;

            Canvas.SetLeft(vo.Image, vo.PositionX);
            Canvas.SetTop(vo.Image, vo.PositionY);

            TransformImage(vo.Image, vo.FlipHorizontal, vo.FlipVertical);

            _ = frameViewView.OnionCanvas.Children.Add(vo.Image);

            _onionSkinImages.Add(vo.Image);
        }
    }

    private void FrameView_Drop(object sender, DragEventArgs e)
    {
        if (frameView.DataContext is not FrameView frameViewView)
        {
            return;
        }

        object data = e.Data.GetData(typeof(List<FrameElementDragObjectVO>));

        List<FrameElementDragObjectVO> list = (List<FrameElementDragObjectVO>)data;

        foreach (FrameElementDragObjectVO item in list)
        {
            if (item.DragableObject is SpriteControlVO spriteVO)
            {
                if (!string.IsNullOrEmpty(_bankID) && spriteVO.BankID != _bankID)
                {
                    _ = MessageBox.Show("For now it is not possible to have a frame with sprites from two or more different banks", "Error", MessageBoxButton.OK);

                    RemoveFromFrameViewTempElement(frameViewView, item);
                    return;
                }

                if (string.IsNullOrEmpty(_bankID))
                {
                    _bankID = spriteVO.BankID;
                }

                if (frameViewView.FrameCanvas.Children.Contains(spriteVO.Image))
                {
                    string id = spriteVO.ID;

                    if (id == "new")
                    {
                        id = Guid.NewGuid().ToString();
                    }

                    Image image = new() { Source = spriteVO.Image?.Source };

                    Point elementPosition = e.GetPosition(frameViewView.FrameCanvas);

                    int exactPosX = (int)(elementPosition.X - item.OffsetX);
                    int exactPosY = (int)(elementPosition.Y - item.OffsetY);

                    SpriteControlVO sprite = AddSpriteToFrameView(id, spriteVO, exactPosX, exactPosY, image);

                    TransformImage(sprite.Image, spriteVO.FlipHorizontal, spriteVO.FlipVertical);

                    _ = frameViewView.FrameCanvas.Children.Add(sprite.Image);

                    // remove the previous instance of the Image control in the canvas now that there is a new one to replace it.
                    RemoveFromFrameViewTempElement(frameViewView, item);

                    SaveCharacterSpriteInformation(sprite, new Point(exactPosX, exactPosY), spriteVO.BankID);
                }
            }
            else if (item.DragableObject is CollisionControlVO collisionVO)
            {
                if (frameViewView.CollisionCanvas.Children.Contains(collisionVO.Rectangle))
                {
                    Point elementPosition = e.GetPosition(frameViewView.CollisionCanvas);

                    int exactPosX = (int)(elementPosition.X - item.OffsetX);
                    int exactPosY = (int)(elementPosition.Y - item.OffsetY);

                    CollisionControlVO collision = new()
                    {
                        Rectangle = collisionVO.Rectangle,
                        ID = collisionVO.ID,
                        Height = collisionVO.Height,
                        Width = collisionVO.Width,
                        PositionX = exactPosX,
                        PositionY = exactPosY,
                        Color = collisionVO.Color,
                        Mask = collisionVO.Mask,
                        CustomMask = collisionVO.CustomMask,
                        AnimationID = collisionVO.AnimationID,
                        FrameID = collisionVO.FrameID
                    };

                    Canvas.SetLeft(collisionVO.Rectangle, exactPosX);
                    Canvas.SetTop(collisionVO.Rectangle, exactPosY);

                    if (collisionVO.Rectangle != null)
                    {
                        if (!_collisionsInFrame.TryGetValue(collisionVO.Rectangle, out _))
                        {
                            _collisionsInFrame.Add(collisionVO.Rectangle, collision);
                        }
                    }

                    SpriteCollisionVO vo = new()
                    {
                        ActAsVO = true,
                        ID = collisionVO.ID,
                        Width = collisionVO.Width,
                        Height = collisionVO.Height,
                        PosX = exactPosX,
                        PosY = exactPosY,
                        Color = collisionVO.Color,
                        Mask = collisionVO.Mask,
                        CustomMask = collisionVO.CustomMask,
                        AnimationID = collisionVO.AnimationID,
                        FrameID = collisionVO.FrameID
                    };

                    SignalManager.Get<UpdateSpriteCollisionInfoSignal>().Dispatch(vo);
                }
            }
        }
    }

    private void RemoveFromFrameViewTempElement(FrameView frameViewView, FrameElementDragObjectVO item)
    {
        if (item.DragableObject is SpriteControlVO spriteVO)
        {
            if (spriteVO.Image == null)
            {
                return;
            }

            if (_spritesInFrames.TryGetValue(spriteVO.Image, out _))
            {
                _ = _spritesInFrames.Remove(spriteVO.Image);
            }

            frameViewView.FrameCanvas.Children.Remove(spriteVO.Image);

            if (_spritesInFrames.Count == 0)
            {
                _bankID = string.Empty;
            }
        }
        else if (item.DragableObject is CollisionControlVO collisionVO)
        {
            if (collisionVO.Rectangle == null)
            {
                return;
            }

            if (_collisionsInFrame.TryGetValue(collisionVO.Rectangle, out _))
            {
                _ = _collisionsInFrame.Remove(collisionVO.Rectangle);
            }

            frameViewView.CollisionCanvas.Children.Remove(collisionVO.Rectangle);
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

            foreach (object child in frameViewView.FrameCanvas.Children)
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

    private SpriteControlVO AddSpriteToFrameView(string id, SpriteControlVO draggingSprite, int exactPosX, int exactPosY, Image image)
    {
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

        if (!_spritesInFrames.TryGetValue(sprite.Image, out _))
        {
            _spritesInFrames.Add(sprite.Image, sprite);
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

        Point mousePosition = e.GetPosition(frameViewView.FrameCanvas);

        object data = e.Data.GetData(typeof(List<FrameElementDragObjectVO>));

        List<FrameElementDragObjectVO> list = (List<FrameElementDragObjectVO>)data;

        foreach (FrameElementDragObjectVO item in list)
        {
            if (item.DragableObject is SpriteControlVO spriteVO)
            {
                if (spriteVO.Image == null)
                {
                    continue;
                }

                if (!frameViewView.FrameCanvas.Children.Contains(spriteVO.Image))
                {
                    spriteVO.Image.IsHitTestVisible = false;

                    _ = frameViewView.FrameCanvas.Children.Add(spriteVO.Image);
                }

                int exactPosX = (int)(mousePosition.X - item.OffsetX);
                int exactPosY = (int)(mousePosition.Y - item.OffsetY);

                Canvas.SetLeft(spriteVO.Image, exactPosX);
                Canvas.SetTop(spriteVO.Image, exactPosY);
            }
            else if (item.DragableObject is CollisionControlVO collisionVO)
            {
                if (collisionVO.Rectangle == null)
                {
                    continue;
                }

                if (!frameViewView.CollisionCanvas.Children.Contains(collisionVO.Rectangle))
                {
                    collisionVO.Rectangle.IsHitTestVisible = false;

                    _ = frameViewView.CollisionCanvas.Children.Add(collisionVO.Rectangle);
                }

                int exactPosX = (int)(mousePosition.X - item.OffsetX);
                int exactPosY = (int)(mousePosition.Y - item.OffsetY);

                Canvas.SetLeft(collisionVO.Rectangle, exactPosX);
                Canvas.SetTop(collisionVO.Rectangle, exactPosY);
            }
        }
    }

    private void FrameView_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.OriginalSource is not Canvas and not Rectangle)
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

        _initialMousePositionInCanvas = e.GetPosition(frameViewView.FrameCanvas);

        SignalManager.Get<ResetFrameSpritesSelectionAreaSignal>().Dispatch(_initialMousePositionInCanvas);

        List<CollisionControlVO> selectedCollisions = [];
        List<SpriteControlVO> selectedSprites = [];

        bool canSelectCollisions = viewModel.ShowCollisions;

        // this will prioritize the rectangles over the images
        if (canSelectCollisions)
        {
            GetListCollisionSelected(frameViewView.collisionCanvas, _initialMousePositionInCanvas, ref selectedCollisions);
        }

        GetListSpriteControlSelected(frameViewView.FrameCanvas, _initialMousePositionInCanvas, ref selectedSprites);

        bool spriteWasAlreadySelected = false;
        bool collisionWasAlreadySelected = false;

        foreach (SpriteControlVO spriteVO in selectedSprites)
        {
            if (viewModel.SelectedFrameSprites.Contains(spriteVO.ID))
            {
                spriteWasAlreadySelected = true;
            }
        }

        foreach (CollisionControlVO collisionVO in selectedCollisions)
        {
            if (viewModel.SelectedFrameCollisions.Contains(collisionVO.ID))
            {
                collisionWasAlreadySelected = true;
            }
        }

        if ((viewModel.SelectedFrameSprites.Length == 0 || !spriteWasAlreadySelected)
            && (viewModel.SelectedFrameCollisions.Length == 0 || !collisionWasAlreadySelected))
        {
            SignalManager.Get<SelectFrameElementsSignal>().Dispatch([.. selectedSprites], [.. selectedCollisions]);
        }
    }

    private void FrameView_MouseUp(object sender, MouseButtonEventArgs e)
    {
        if (e.OriginalSource is not Canvas and not Image and not Rectangle)
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

        if (frameViewView.FrameCanvas.IsMouseCaptured)
        {
            frameViewView.FrameCanvas.ReleaseMouseCapture();
        }

        List<SpriteControlVO> selectedSprites = [];
        List<CollisionControlVO> selectedCollisions = [];

        bool canSelectCollisions = viewModel.ShowCollisions;

        if (frameViewView.MouseSelectionActive == Visibility.Visible)
        {
            (selectedSprites, selectedCollisions) = CheckMouseAreaSelected(frameViewView, canSelectCollisions);
        }

        Point pos = e.GetPosition(frameViewView.FrameCanvas);

        SignalManager.Get<ResetFrameSpritesSelectionAreaSignal>().Dispatch(pos);

        if (canSelectCollisions && selectedCollisions.Count == 0)
        {
            GetListCollisionSelected(frameViewView.collisionCanvas, pos, ref selectedCollisions);
        }

        if (selectedSprites.Count == 0)
        {
            GetListSpriteControlSelected(frameViewView.FrameCanvas, pos, ref selectedSprites);
        }

        SignalManager.Get<SelectFrameElementsSignal>().Dispatch([.. selectedSprites], [.. selectedCollisions]);
    }

    private void GetListSpriteControlSelected(Canvas canvas, Point position, ref List<SpriteControlVO> selectedSprites)
    {
        List<Image> images = GetSelectionMouseOver<Image>(canvas, position);

        if (images.Count > 0)
        {
            if (_spritesInFrames.TryGetValue(images.First(), out SpriteControlVO? spriteControl))
            {
                selectedSprites.Add(spriteControl);
            }
        }
    }

    private void GetListCollisionSelected(Canvas canvas, Point position, ref List<CollisionControlVO> selectedCollisions)
    {
        List<Rectangle> rects = GetSelectionMouseOver<Rectangle>(canvas, position);

        if (rects.Count > 0)
        {
            if (_collisionsInFrame.TryGetValue(rects.First(), out CollisionControlVO? collision))
            {
                selectedCollisions.Add(collision);
            }
        }
    }

    private static List<T> GetSelectionMouseOver<T>(Canvas canvas, Point positionInCanvas) where T : FrameworkElement, new()
    {
        CanvasHitDetection<T, EllipseGeometry> hitDetection = new(new(positionInCanvas, 1.0, 1.0), canvas);
        List<T> hitList = hitDetection.HitTest();

        return hitList;
    }

    private (List<SpriteControlVO>, List<CollisionControlVO>) CheckMouseAreaSelected(FrameView frameViewView, bool canSelectCollisions)
    {
        List<SpriteControlVO> sprites = [];
        List<CollisionControlVO> collisions = [];

        if (frameViewView.MouseSelectionWidth == 0 ||
            frameViewView.MouseSelectionHeight == 0)
        {
            return (sprites, collisions);
        }

        Rect rectangle = new(
            frameViewView.MouseSelectionOriginX,
            frameViewView.MouseSelectionOriginY,
            frameViewView.MouseSelectionWidth,
            frameViewView.MouseSelectionHeight);

        CanvasHitDetection<Image, RectangleGeometry> hitDetection = new(new(rectangle), frameViewView.FrameCanvas);
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

        if (canSelectCollisions)
        {
            CanvasHitDetection<Rectangle, RectangleGeometry> hitDetection2 = new(new(rectangle), frameViewView.collisionCanvas);
            List<Rectangle> hitList2 = hitDetection2.HitTest();

            if (hitList2.Count > 0)
            {
                foreach (Rectangle item in hitList2)
                {
                    if (_collisionsInFrame.TryGetValue(item, out CollisionControlVO? collision))
                    {
                        collisions.Add(collision);
                    }
                }
            }
        }

        return (sprites, collisions);
    }

    private void FrameView_DragLeave(object sender, DragEventArgs e)
    {
        if (e.OriginalSource is not Canvas and not Image and not Rectangle)
        {
            return;
        }

        if (frameView.DataContext is not FrameView frameViewView)
        {
            return;
        }

        object data = e.Data.GetData(typeof(List<FrameElementDragObjectVO>));

        List<FrameElementDragObjectVO> list = (List<FrameElementDragObjectVO>)data;

        foreach (FrameElementDragObjectVO item in list)
        {
            RemoveFromFrameViewTempElement(frameViewView, item);
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

        Point positionInCanvas = e.GetPosition(frameViewView.FrameCanvas);

        if (Util.AboutEqual(positionInCanvas.X, _initialMousePositionInCanvas.X) &&
            Util.AboutEqual(positionInCanvas.Y, _initialMousePositionInCanvas.Y))
        {
            return;
        }

        if (viewModel.SelectedFrameSprites.Length > 0 ||
            viewModel.SelectedFrameCollisions.Length > 0)
        {
            DragFrameElements(positionInCanvas, viewModel.SelectedFrameSprites, viewModel.SelectedFrameCollisions, (DependencyObject)e.Source);
        }
        else
        {
            if (!frameViewView.FrameCanvas.IsMouseCaptured)
            {
                frameViewView.FrameCanvas.CaptureMouse();
            }

            UpdateMouseSelectionArea(frameViewView, positionInCanvas);
        }
    }

    private void DragFrameElements(Point positionInCanvas, string[] selectedFrameSprites, string[] selectedFrameCollisions, DependencyObject dragSource)
    {
        List<FrameElementDragObjectVO> characterDragObjects = [];

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

        for (int i = 0; i < selectedFrameCollisions.Length; i++)
        {
            string collisionID = selectedFrameCollisions[i];

            foreach (KeyValuePair<Rectangle, CollisionControlVO> item in _collisionsInFrame)
            {
                if (item.Value.ID != collisionID)
                {
                    continue;
                }

                double rectanglePosX = Canvas.GetLeft(item.Value.Rectangle);
                double rectanglePosY = Canvas.GetTop(item.Value.Rectangle);

                characterDragObjects.Add(new(item.Value, positionInCanvas.X - rectanglePosX, positionInCanvas.Y - rectanglePosY));

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
            string[] ids = [.. characterDragObjects.Select(o => o.DragableObject.ID)];

            SignalManager.Get<DeleteElementsFromCharacterFrameSignal>().Dispatch(ids);

            // update images
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

            // update collisions
            Dictionary<Rectangle, CollisionControlVO> collisionsInFramesTmp = [];

            foreach (KeyValuePair<Rectangle, CollisionControlVO> item in _collisionsInFrame)
            {
                if (!ids.Contains(item.Value.ID))
                {
                    collisionsInFramesTmp.Add(item.Key, item.Value);
                }
            }

            _collisionsInFrame.Clear();

            foreach (KeyValuePair<Rectangle, CollisionControlVO> item in collisionsInFramesTmp)
            {
                _collisionsInFrame.Add(item.Key, item.Value);
            }
        }
        else
        {
            SignalManager.Get<ElementFrameShowSelectionSignal>().Dispatch([.. characterDragObjects]);
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
            image.Opacity = enabledOnionSkin ? ModelManager.Get<GBAToolConfigurationModel>().OnionSkinOpacity : 0.0;
        }
    }

    private void UserControl_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        double left = e.Key == Key.Left ? -1 : e.Key == Key.Right ? 1 : 0;
        double top = e.Key == Key.Up ? -1 : e.Key == Key.Down ? 1 : 0;

        if (left == 0 && top == 0)
        {
            return;
        }

        if (DataContext is not CharacterFrameEditorViewModel viewModel)
        {
            return;
        }

        bool updated = false;
        List<SpriteControlVO> selectedSprites = [];
        List<CollisionControlVO> selectedCollisions = [];

        foreach (KeyValuePair<Image, SpriteControlVO> item in _spritesInFrames)
        {
            if (viewModel.SelectedFrameSprites.Contains(item.Value.ID))
            {
                if (!updated)
                {
                    SignalManager.Get<ResetFrameSpritesSelectionAreaSignal>().Dispatch(_initialMousePositionInCanvas);
                    updated = true;
                }

                double x = Canvas.GetLeft(item.Key);
                double y = Canvas.GetTop(item.Key);

                x += left;
                y += top;

                CharacterSprite characterSprite = new()
                {
                    ID = item.Value.ID,
                    Position = new Point(x, y),
                    FlipHorizontal = item.Value.FlipHorizontal,
                    FlipVertical = item.Value.FlipVertical,
                    SpriteID = item.Value.SpriteID,
                    TileSetID = item.Value.TileSetID,
                    Width = item.Value.Width,
                    Height = item.Value.Height,
                };

                Canvas.SetLeft(item.Key, x);
                Canvas.SetTop(item.Key, y);

                SignalManager.Get<AddOrUpdateSpriteIntoCharacterFrameSignal>().Dispatch(characterSprite, item.Value.BankID);

                selectedSprites.Add(item.Value);
            }
        }

        foreach (KeyValuePair<Rectangle, CollisionControlVO> item in _collisionsInFrame)
        {
            if (viewModel.SelectedFrameCollisions.Contains(item.Value.ID))
            {
                SpriteCollisionVO? collisionVO = viewModel.CharacterCollisions.Find(x => x.ID == item.Value.ID);

                if (collisionVO == null)
                    continue;

                if (!updated)
                {
                    SignalManager.Get<ResetFrameSpritesSelectionAreaSignal>().Dispatch(_initialMousePositionInCanvas);
                    updated = true;
                }

                double x = Canvas.GetLeft(item.Key);
                double y = Canvas.GetTop(item.Key);

                x += left;
                y += top;

                SpriteCollisionVO vo = new()
                {
                    ActAsVO = true,
                    ID = collisionVO.ID,
                    Width = item.Value.Width,
                    Height = item.Value.Height,
                    PosX = (int)x,
                    PosY = (int)y,
                    Color = collisionVO.Color,
                    Mask = collisionVO.Mask,
                    CustomMask = collisionVO.CustomMask,
                    AnimationID = collisionVO.AnimationID,
                    FrameID = collisionVO.FrameID
                };

                SignalManager.Get<UpdateSpriteCollisionInfoSignal>().Dispatch(vo);

                selectedCollisions.Add(item.Value);
            }
        }

        e.Handled = updated;

        if (selectedSprites.Count > 0 || selectedCollisions.Count > 0)
        {
            SignalManager.Get<SelectFrameElementsSignal>().Dispatch([.. selectedSprites], [.. selectedCollisions]);
        }
    }

    private void ListViewItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not ListViewItem listViewItem)
        {
            return;
        }

        SpriteCollisionVO collision = (SpriteCollisionVO)lvFrameCollisions.ItemContainerGenerator.ItemFromContainer(listViewItem);

        foreach (KeyValuePair<Rectangle, CollisionControlVO> item in _collisionsInFrame)
        {
            if (item.Value.ID != collision.ID)
            {
                continue;
            }

            if (item.Value.Rectangle == null)
            {
                return;
            }

            CollisionControlVO collisionVO = new()
            {
                ID = item.Value.ID,
                Rectangle = item.Value.Rectangle,
                Width = item.Value.Width,
                Height = item.Value.Height,
                PositionX = item.Value.PositionX,
                PositionY = item.Value.PositionY,
                Color = item.Value.Color,
                Mask = item.Value.Mask,
                CustomMask = item.Value.CustomMask,
                AnimationID = item.Value.AnimationID,
                FrameID = item.Value.FrameID
            };

            SignalManager.Get<SelectFrameElementsSignal>().Dispatch([], [collisionVO]);

            break;
        }
    }
}
