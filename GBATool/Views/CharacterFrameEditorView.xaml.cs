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

    public CharacterFrameEditorView()
    {
        InitializeComponent();
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        bankViewer.OnActivate();
        frameView.OnActivate();

        if (DataContext is not CharacterFrameEditorViewModel viewModel)
        {
            return;
        }

        SignalManager.Get<SetBankModelToBankViewerSignal>().Dispatch(viewModel.BankModel);
    }

    private void UserControl_Unloaded(object sender, RoutedEventArgs e)
    {
        bankViewer.OnDeactivate();
        frameView.OnDeactivate();
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

        if (bankViewerView.Canvas.Children.Contains(sprite.Image))
        {
            bankViewerView.Canvas.Children.Remove(sprite.Image);
        }
    }

    private void FrameView_Drop(object sender, DragEventArgs e)
    {
        object data = e.Data.GetData(typeof(SpriteControlVO));

        if (frameView.DataContext is not FrameView frameViewView)
        {
            return;
        }

        SpriteControlVO draggingSprite = (SpriteControlVO)data;

        if (frameViewView.Canvas.Children.Contains(draggingSprite.Image))
        {
            string id = draggingSprite.ID;

            if (id == "new")
            {
                id = Guid.NewGuid().ToString();
            }

            SpriteControlVO sprite = new()
            {
                ID = id,
                Image = new() { Source = draggingSprite.Image?.Source },
                SpriteID = draggingSprite.SpriteID,
                TileSetID = draggingSprite.TileSetID,
                Width = draggingSprite.Width,
                Height = draggingSprite.Height
            };

            Point elementPosition = e.GetPosition(frameViewView.Canvas);

            int exactPosX = (int)(elementPosition.X - _spriteOffsetX);
            int exactPosY = (int)(elementPosition.Y - _spriteOffsetY);

            Canvas.SetLeft(sprite.Image, exactPosX);
            Canvas.SetTop(sprite.Image, exactPosY);

            _ = frameViewView.Canvas.Children.Add(sprite.Image);

            frameViewView.Canvas.Children.Remove(draggingSprite.Image);

            SaveCharacterSpriteInformation(sprite, new Point(exactPosX, exactPosY));
        }
    }

    private void SaveCharacterSpriteInformation(SpriteControlVO sprite, Point position)
    {
        if (sprite.Image == null)
        {
            return;
        }

        if (!_spritesInFrames.TryGetValue(sprite.Image, out _))
        {
            _spritesInFrames.Add(sprite.Image, sprite);
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
            Height = sprite.Height
        };

        SignalManager.Get<AddNewSpriteIntoCharacterFrame>().Dispatch(characterSprite);
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
            return;

        if (!frameViewView.Canvas.Children.Contains(draggingSprite.Image))
        {
            draggingSprite.Image.IsHitTestVisible = false;

            _ = frameViewView.Canvas.Children.Add(draggingSprite.Image);
        }

        Point elementPosition = e.GetPosition(frameViewView.Canvas);

        int exactPosX = (int)(elementPosition.X - _spriteOffsetX);
        int exactPosY = (int)(elementPosition.Y - _spriteOffsetY);

        Canvas.SetLeft(draggingSprite.Image, exactPosX);
        Canvas.SetTop(draggingSprite.Image, exactPosY);
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

        EllipseGeometry hitArea = new(positionInCanvas, 1.0, 1.0);
        List<Image> hitList = [];

        VisualTreeHelper.HitTest(frameViewView.canvas,
                new HitTestFilterCallback(o =>
                {
                    if (o.GetType() == typeof(Image))
                        return HitTestFilterBehavior.ContinueSkipChildren;
                    else
                        return HitTestFilterBehavior.Continue;
                }),
                new HitTestResultCallback(result =>
                {
                    if (result?.VisualHit is Image)
                    {
                        IntersectionDetail intersectionDetail = ((GeometryHitTestResult)result).IntersectionDetail;
                        if (intersectionDetail == IntersectionDetail.FullyContains)
                        {
                            hitList.Add((Image)result.VisualHit);
                            return HitTestResultBehavior.Continue;
                        }
                        else if (intersectionDetail != IntersectionDetail.Intersects &&
                            intersectionDetail != IntersectionDetail.FullyInside)
                        {
                            return HitTestResultBehavior.Stop;
                        }
                    }

                    return HitTestResultBehavior.Continue;
                }),
                new GeometryHitTestParameters(hitArea));

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
                if (frameViewView.Canvas.Children.Contains(spriteControl.Image))
                {
                    frameViewView.Canvas.Children.Remove(spriteControl.Image);

                    SignalManager.Get<DeleteSpriteFromCharacterFrame>().Dispatch(spriteControl.ID);

                    _spritesInFrames.Remove(hitList[0]);
                }
            }
        }
    }
}
