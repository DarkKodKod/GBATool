using ArchitectureLibrary.Signals;
using GBATool.Models;
using GBATool.Signals;
using GBATool.Utils;
using GBATool.ViewModels;
using GBATool.VOs;
using System;
using System.Collections.Generic;
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

    public CharacterFrameEditorView()
    {
        InitializeComponent();
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        #region Signals
        SignalManager.Get<SelectImageControlInFrameViewSignal>().Listener += OnSelectImageControlInFrameView;
        SignalManager.Get<FillWithSpriteControlsSignal>().Listener += OnFillWithSpriteControls;
        #endregion

        bankViewer.OnActivate();

        if (DataContext is not CharacterFrameEditorViewModel viewModel)
        {
            return;
        }

        frameView.canvas.Children.Clear();

        SignalManager.Get<SetBankModelToBankViewerSignal>().Dispatch(viewModel.BankModel);
    }

    private void UserControl_Unloaded(object sender, RoutedEventArgs e)
    {
        bankViewer.OnDeactivate();

        _spritesInFrames.Clear();
        _spriteOffsetX = 0;
        _spriteOffsetY = 0;

        #region Signals
        SignalManager.Get<SelectImageControlInFrameViewSignal>().Listener -= OnSelectImageControlInFrameView;
        SignalManager.Get<FillWithSpriteControlsSignal>().Listener -= OnFillWithSpriteControls;
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
                SignalManager.Get<DeleteSpriteFromCharacterFrameSignal>().Dispatch(spriteControl.ID);

                _spritesInFrames.Remove(sprite.Image);
            }
        }

        if (bankViewerView.Canvas.Children.Contains(sprite.Image))
        {
            bankViewerView.Canvas.Children.Remove(sprite.Image);
        }
    }

    private void OnFillWithSpriteControls(List<SpriteControlVO> spriteVOList)
    {
        if (frameView.DataContext is not FrameView frameViewView)
        {
            return;
        }

        foreach (SpriteControlVO vo in spriteVOList)
        {
            (SpriteControlVO _, Border border) = AddSpriteToFrameView(vo.ID, vo, vo.PositionX, vo.PositionY, vo.Image);

            _ = frameViewView.Canvas.Children.Add(border);
        }

        UnselectAllImagesInFrameView();
    }

    private void FrameView_Drop(object sender, DragEventArgs e)
    {
        if (frameView.DataContext is not FrameView frameViewView)
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

            UnselectAllImagesInFrameView();

            Point elementPosition = e.GetPosition(frameViewView.Canvas);

            int exactPosX = (int)(elementPosition.X - _spriteOffsetX);
            int exactPosY = (int)(elementPosition.Y - _spriteOffsetY);

            (SpriteControlVO sprite, Border border) = AddSpriteToFrameView(id, draggingSprite, exactPosX, exactPosY, image);

            _ = frameViewView.Canvas.Children.Add(border);

            // remove the previous instance of the Image control in the canvas now that there is a new one to replace it.
            if (draggingSprite.Image != null)
            {
                if (_spritesInFrames.TryGetValue(draggingSprite.Image, out _))
                {
                    _spritesInFrames.Remove(draggingSprite.Image);
                }
            }

            frameViewView.Canvas.Children.Remove(draggingSprite.Image);

            string bankID = string.IsNullOrEmpty(sprite.BankID) ? viewModel.BankModel.GUID : sprite.BankID;

            SaveCharacterSpriteInformation(sprite, new Point(exactPosX, exactPosY), bankID);
        }
    }

    private (SpriteControlVO, Border) AddSpriteToFrameView(string id, SpriteControlVO draggingSprite, int exactPosX, int exactPosY, Image? image)
    {
        SpriteControlVO sprite = new()
        {
            ID = id,
            Image = image,
            SpriteID = draggingSprite.SpriteID,
            TileSetID = draggingSprite.TileSetID,
            Width = draggingSprite.Width,
            Height = draggingSprite.Height,
            OffsetX = draggingSprite.OffsetX,
            OffsetY = draggingSprite.OffsetY
        };

        Border border = new()
        {
            BorderBrush = Brushes.Red,
            BorderThickness = SelectionThickness,
            Child = sprite.Image,
        };

        Canvas.SetLeft(border, exactPosX);
        Canvas.SetTop(border, exactPosY);

        if (sprite.Image != null)
        {
            if (!_spritesInFrames.TryGetValue(sprite.Image, out _))
            {
                _spritesInFrames.Add(sprite.Image, sprite);
            }
        }

        return (sprite, border);
    }

    private static void SaveCharacterSpriteInformation(SpriteControlVO sprite, Point position, string bankID)
    {
        if (sprite.Image == null)
        {
            return;
        }

        CharacterSprite characterSprite = new()
        {
            ID = sprite.ID,
            Position = position,
            FlipX = false,
            FlipY = false,
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

        if (draggingSprite.Image.Parent is Border border)
        {
            if (frameViewView.Canvas.Children.Contains(border))
            {
                border.Child = null;

                double imagePosX = Canvas.GetLeft(border);
                double imagePosY = Canvas.GetTop(border);

                frameViewView.Canvas.Children.Remove(border);

                _spriteOffsetX = elementPosition.X - imagePosX;
                _spriteOffsetY = elementPosition.Y - imagePosY;
            }
        }

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

    private void FrameView_DragLeave(object sender, DragEventArgs e)
    {
        if (e.OriginalSource is not Canvas)
        {
            return;
        }

        object data = e.Data.GetData(typeof(SpriteControlVO));

        SpriteControlVO sprite = (SpriteControlVO)data;

        if (sprite.Image == null)
        {
            return;
        }

        if (frameView.DataContext is FrameView frameViewView)
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

        Point positionInCanvas = e.GetPosition(frameViewView.canvas);

        CanvasHitDetection<Image> hitDetection = new(new(positionInCanvas, 1.0, 1.0), frameViewView.canvas);
        List<Image> hitList = hitDetection.HitTest();

        if (hitList.Count > 0)
        {
            if (!_spritesInFrames.TryGetValue(hitList[0], out SpriteControlVO? spriteControl))
            {
                return;
            }

            DataObject data = new(spriteControl);

            _spriteOffsetX = 0;
            _spriteOffsetY = 0;

            DragDropEffects result = DragDrop.DoDragDrop((DependencyObject)e.Source, data, DragDropEffects.Move);

            if (result == DragDropEffects.None)
            {
                SignalManager.Get<DeleteSpriteFromCharacterFrameSignal>().Dispatch(spriteControl.ID);

                _spritesInFrames.Remove(hitList[0]);
            }
        }
    }

    private void UnselectAllImagesInFrameView()
    {
        foreach (KeyValuePair<Image, SpriteControlVO> item in _spritesInFrames)
        {
            if (item.Key != null)
            {
                if (item.Key.Parent is Border border)
                {
                    border.BorderThickness = new Thickness(0.0);
                }
            }
        }
    }

    private void OnSelectImageControlInFrameView(Point point)
    {
        if (frameView.DataContext is not FrameView frameViewView)
        {
            return;
        }

        CanvasHitDetection<Image> hitDetection = new(new(point, 1.0, 1.0), frameViewView.canvas);
        List<Image> hitList = hitDetection.HitTest();

        if (hitList.Count > 0)
        {
            UnselectAllImagesInFrameView();

            if (hitList[0].Parent is Border border)
            {
                border.BorderThickness = SelectionThickness;

                if (_spritesInFrames.TryGetValue(hitList[0], out SpriteControlVO? sprite))
                {
                    SignalManager.Get<SelectFrameSpriteSignal>().Dispatch(sprite);
                }
            }
        }
    }
}
