using ArchitectureLibrary.Signals;
using GBATool.Signals;
using GBATool.Utils;
using GBATool.ViewModels;
using GBATool.VOs;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GBATool.Views;

/// <summary>
/// Interaction logic for CharacterFrameEditorView.xaml
/// </summary>
public partial class CharacterFrameEditorView : UserControl
{
    private double _spriteOffsetX;
    private double _spriteOffsetY;

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

    private void UserControl_MouseMove(object sender, MouseEventArgs e)
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

    private void UserControl_DragOver(object sender, DragEventArgs e)
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
            SpriteControlVO sprite = new()
            {
                Image = new() { Source = draggingSprite.Image?.Source },
                SpriteID = draggingSprite.SpriteID,
                TileSetID = draggingSprite.TileSetID,
                Width = draggingSprite.Width,
                Height = draggingSprite.Height
            };

            Point elementPosition = e.GetPosition(frameViewView.Canvas);

            Canvas.SetLeft(sprite.Image, elementPosition.X - _spriteOffsetX);
            Canvas.SetTop(sprite.Image, elementPosition.Y - _spriteOffsetY);

            _ = frameViewView.Canvas.Children.Add(sprite.Image);

            frameViewView.Canvas.Children.Remove(draggingSprite.Image);
        }
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

        Canvas.SetLeft(draggingSprite.Image, elementPosition.X - _spriteOffsetX);
        Canvas.SetTop(draggingSprite.Image, elementPosition.Y - _spriteOffsetY);
    }
}
