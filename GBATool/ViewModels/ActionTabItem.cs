using ArchitectureLibrary.Signals;
using ArchitectureLibrary.ViewModel;
using GBATool.Signals;
using GBATool.Views;
using System.Windows.Controls;

namespace GBATool.ViewModels;

public class ActionTabItem : ViewModel
{
    private bool _isInEditMode;
    private string _header = string.Empty;
    private UserControl? _content;

    public string ID { get; set; } = string.Empty;
    public UserControl? FramesView { get; set; }
    public UserControl? PixelsView { get; set; }

    public string Header
    {
        get => _header;
        set
        {
            if (_header != value)
            {
                bool changedName = !string.IsNullOrEmpty(_header);

                _header = value;

                OnPropertyChanged(nameof(Header));

                if (changedName)
                {
                    SignalManager.Get<RenamedAnimationTabSignal>().Dispatch(value);
                }
            }
        }
    }

    public UserControl? Content
    {
        get => _content;
        set
        {
            _content = value;

            OnPropertyChanged(nameof(Content));
        }
    }

    public string OldCaptionValue { get; set; } = "";

    public void SwapContent(string tabId, string frameId, int frameIndex)
    {
        if (Content != FramesView)
        {
            if (Content is CharacterFrameEditorView characterView)
            {
                CharacterFrameEditorViewModel? currentFrameViewModel = characterView.DataContext as CharacterFrameEditorViewModel;

                currentFrameViewModel?.OnDeactivate();
            }

            Content = FramesView;

            if (Content is CharacterAnimationView animationView)
            {
                animationView.OnActivate();

                CharacterAnimationViewModel? viewModel = animationView.DataContext as CharacterAnimationViewModel;
                viewModel?.OnActivate();
            }
        }
        else
        {
            string previousFrameID = string.Empty;
            int previousFrameIndex = 0;

            if (Content is CharacterAnimationView animationView)
            {
                for (int i = 0; i < animationView.FrameViewList.Count; i++)
                {
                    CharacterFrameView characterFrameView = animationView.FrameViewList[i];
                    if (characterFrameView.FrameID == frameId)
                    {
                        int index = i == 0 ? animationView.FrameViewList.Count - 1 : i - 1;

                        previousFrameID = animationView.FrameViewList[index].FrameID;
                        previousFrameIndex = animationView.FrameViewList[index].FrameIndex;
                    }
                }

                animationView.OnDeactivate();

                CharacterAnimationViewModel? viewModel = animationView.DataContext as CharacterAnimationViewModel;
                viewModel?.OnDeactivate();
            }

            Content = PixelsView;

            if (Content is CharacterFrameEditorView characterView)
            {
                if (characterView.DataContext is CharacterFrameEditorViewModel currentFrameViewModel)
                {
                    currentFrameViewModel.TabID = tabId;
                    currentFrameViewModel.FrameIndex = frameIndex;
                    currentFrameViewModel.FrameID = frameId;
                    currentFrameViewModel.PreviousFrameID = previousFrameID;
                    currentFrameViewModel.PreviousFrameIndex = previousFrameIndex;

                    currentFrameViewModel.OnActivate();
                }
            }
        }
    }

    public bool IsInEditMode
    {
        get => _isInEditMode;
        set
        {
            if (_isInEditMode != value)
            {
                _isInEditMode = value;

                OnPropertyChanged(nameof(IsInEditMode));
            }
        }
    }
}
