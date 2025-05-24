using ArchitectureLibrary.Signals;
using GBATool.Signals;
using GBATool.ViewModels;
using System.Collections.Generic;
using System.Windows.Controls;

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

        OnActivate();
    }

    public void OnActivate()
    {
        SignalManager.Get<NewAnimationFrameSignal>().Listener += OnNewAnimationFrame;
        SignalManager.Get<DeleteAnimationFrameSignal>().Listener += OnDeleteAnimationFrame;
    }

    public void OnDeactivate()
    {
        SignalManager.Get<NewAnimationFrameSignal>().Listener -= OnNewAnimationFrame;
        SignalManager.Get<DeleteAnimationFrameSignal>().Listener -= OnDeleteAnimationFrame;

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
}
