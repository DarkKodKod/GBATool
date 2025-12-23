using ArchitectureLibrary.Signals;
using GBATool.Signals;
using GBATool.ViewModels;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GBATool.Views;

/// <summary>
/// Interaction logic for CharacterAnimationView.xaml
/// </summary>
public partial class CharacterAnimationView : UserControl
{
    public List<CharacterFrameView> FrameViewList { get; set; } = [];

    public CharacterAnimationView()
    {
        InitializeComponent();
    }

    public void OnActivate()
    {
        SignalManager.Get<NewAnimationFrameSignal>().Listener += OnNewAnimationFrame;
        SignalManager.Get<DeleteAnimationFrameSignal>().Listener += OnDeleteAnimationFrame;
        SignalManager.Get<InformationToCorrectlyDisplayTheMetaSpriteCenteredSignal>().Listener += OnInformationToCorrectlyDisplayTheMetaSpriteCentered;
    }

    public void OnDeactivate()
    {
        SignalManager.Get<NewAnimationFrameSignal>().Listener -= OnNewAnimationFrame;
        SignalManager.Get<DeleteAnimationFrameSignal>().Listener -= OnDeleteAnimationFrame;
        SignalManager.Get<InformationToCorrectlyDisplayTheMetaSpriteCenteredSignal>().Listener -= OnInformationToCorrectlyDisplayTheMetaSpriteCentered;

        FrameViewList.Clear();
        spFrames.Children.RemoveRange(0, spFrames.Children.Count - 1);
    }

    private void OnDeleteAnimationFrame(string animationID, int frameIndex)
    {
        if (DataContext is not CharacterAnimationViewModel viewModel)
        {
            return;
        }

        if (viewModel.TabID != animationID)
        {
            return;
        }

        spFrames.Children.RemoveAt(frameIndex);

        foreach (CharacterFrameView frame in FrameViewList)
        {
            if (frame.FrameIndex == frameIndex)
            {
                FrameViewList.Remove(frame);
                break;
            }
        }

        int index = 0;

        // Adjust the index for all the remaining chidren
        foreach (object item in spFrames.Children)
        {
            if (item is CharacterFrameView view)
            {
                view.FrameIndex = index++;
            }
        }
    }

    private void OnNewAnimationFrame(string animationID, string frameID, int newIndex, bool isHeldFrame)
    {
        if (DataContext is not CharacterAnimationViewModel viewModel)
        {
            return;
        }

        if (viewModel.TabID != animationID)
        {
            return;
        }

        var model = viewModel.CharacterModel;

        if (viewModel.FileHandler == null || model == null)
        {
            return;
        }

        CharacterFrameView frame = new(animationID, frameID, spFrames.Children.Count - 1, viewModel.FileHandler, model, isHeldFrame);

        // Insert last
        if (newIndex == -1)
        {
            FrameViewList.Add(frame);

            spFrames.Children.Insert(spFrames.Children.Count - 1, frame);
        }
        else
        {
            // Insert in between

            spFrames.Children.Insert(newIndex, frame);

            FrameViewList.Insert(newIndex, frame);

            int index = 0;

            // Adjust the index for all the remaining chidren
            foreach (object item in spFrames.Children)
            {
                if (item is CharacterFrameView view)
                {
                    view.FrameIndex = index++;
                }
            }
        }
    }

    private void OnInformationToCorrectlyDisplayTheMetaSpriteCentered(double offsetX, double offsetY, double imageWidth, double imageHeight)
    {
        UpdateImagePosition(previewImage, imageWidth, imageHeight, offsetX, offsetY);
    }

    private void UpdateImagePosition(Image image, double imageWidth, double imageHeight, double offsetX, double offsetY)
    {
        double left = (parentCanvas.ActualWidth / 2) - (imageWidth / 2) - offsetX;
        Canvas.SetLeft(image, left);

        double top = (parentCanvas.ActualHeight / 2) - (imageHeight / 2) - offsetY;
        Canvas.SetTop(image, top);
    }

    private void PreviewImage_Loaded(object sender, RoutedEventArgs e)
    {
        if (sender is not Image image)
        {
            return;
        }

        if (image.RenderTransform is not ScaleTransform transform)
        {
            return;
        }

        double imageWidth = image.ActualWidth * transform.ScaleX;
        double imageHeight = image.ActualHeight * transform.ScaleY;

        UpdateImagePosition(image, imageWidth, imageHeight, 0.0, 0.0);
    }
}