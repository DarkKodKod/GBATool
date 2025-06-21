using ArchitectureLibrary.Signals;
using GBATool.Signals;
using GBATool.ViewModels;
using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace GBATool.Views;

/// <summary>
/// Interaction logic for CharacterAnimationView.xaml
/// </summary>
public partial class CharacterAnimationView : UserControl
{
    private double _imagePreviewWidth = 0;
    private double _imagePreviewHeight = 0;
    private bool _fullyLoaded = false;

    public List<CharacterFrameView> FrameViewList { get; set; } = [];

    public CharacterAnimationView()
    {
        InitializeComponent();
    }

    public void OnActivate()
    {
        void CheckLayourUpdate(object? o, EventArgs e)
        {
            if (ActualHeight > 0 && ActualWidth > 0)
            {
                _fullyLoaded = true;

                LayoutUpdated -= CheckLayourUpdate;

                CalculateImagePosition();
            }
        }

        SignalManager.Get<NewAnimationFrameSignal>().Listener += OnNewAnimationFrame;
        SignalManager.Get<DeleteAnimationFrameSignal>().Listener += OnDeleteAnimationFrame;
        SignalManager.Get<PreviewImageUpdatedSignal>().Listener += OnPreviewImageUpdated;

        LayoutUpdated += CheckLayourUpdate;
    }

    public void OnDeactivate()
    {
        SignalManager.Get<NewAnimationFrameSignal>().Listener -= OnNewAnimationFrame;
        SignalManager.Get<DeleteAnimationFrameSignal>().Listener -= OnDeleteAnimationFrame;
        SignalManager.Get<PreviewImageUpdatedSignal>().Listener -= OnPreviewImageUpdated;

        FrameViewList.Clear();
        spFrames.Children.RemoveRange(0, spFrames.Children.Count - 1);
    }

    private void OnDeleteAnimationFrame(int frameIndex)
    {
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

    private void OnNewAnimationFrame(string animationID, string frameID)
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

        CharacterFrameView frame = new(animationID, frameID, spFrames.Children.Count - 1, viewModel.FileHandler, model);

        FrameViewList.Add(frame);

        spFrames.Children.Insert(spFrames.Children.Count - 1, frame);
    }

    private void OnPreviewImageUpdated(double imageWidth, double imageHeight)
    {
        _imagePreviewWidth = imageWidth;
        _imagePreviewHeight = imageHeight;

        CalculateImagePosition();
    }

    private void CalculateImagePosition()
    {
        if (!_fullyLoaded)
        {
            return;
        }

        double left = (parentCanvas.ActualWidth / 2) - (_imagePreviewWidth / 2);
        Canvas.SetLeft(previewImage, left);

        double top = (parentCanvas.ActualHeight / 2) - (_imagePreviewHeight / 2);
        Canvas.SetTop(previewImage, top);
    }
}