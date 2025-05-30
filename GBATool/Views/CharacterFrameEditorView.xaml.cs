using ArchitectureLibrary.Signals;
using GBATool.Signals;
using GBATool.ViewModels;
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
}
