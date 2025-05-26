using ArchitectureLibrary.Signals;
using GBATool.Models;
using GBATool.Signals;
using GBATool.Utils;
using GBATool.ViewModels;
using GBATool.VOs;
using System.Windows;
using System.Windows.Controls;

namespace GBATool.Views;

/// <summary>
/// Interaction logic for CharacterFrameEditorView.xaml
/// </summary>
public partial class CharacterFrameEditorView : UserControl
{
    public CharacterFrameEditorView()
    {
        InitializeComponent();
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        bankViewer.OnActivate();
        frameView.OnActivate();

        #region Signals
        SignalManager.Get<UpdateCharacterImageSignal>().Listener += OnUpdateCharacterImage;
        #endregion

        LoadSpritesProperties();
        LoadFrameImage();
    }

    private void UserControl_Unloaded(object sender, RoutedEventArgs e)
    {
        bankViewer.OnDeactivate();
        frameView.OnDeactivate();

        #region Signals
        SignalManager.Get<UpdateCharacterImageSignal>().Listener -= OnUpdateCharacterImage;
        #endregion
    }

    private void LoadSpritesProperties()
    {
        if (DataContext is not CharacterFrameEditorViewModel viewModel)
        {
            return;
        }

        CharacterModel? charModel = viewModel.CharacterModel;

        if (charModel == null)
            return;
    }

    private void OnUpdateCharacterImage()
    {
        if (DataContext is CharacterFrameEditorViewModel viewModel)
        {
            if (!viewModel.IsActive)
            {
                return;
            }

            LoadFrameImage();
        }
    }

    public void LoadFrameImage()
    {
        if (DataContext is CharacterFrameEditorViewModel viewModel)
        {
            if (viewModel.CharacterModel == null)
            {
                return;
            }

            ImageVO? vo = CharacterUtils.CreateImage(viewModel.CharacterModel, viewModel.AnimationID, viewModel.FrameID);

            if (vo != null && vo.Image != null)
            {
                frameView.FrameImage = vo.Image;
            }
        }
    }
}
