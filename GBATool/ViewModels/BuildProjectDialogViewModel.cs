using ArchitectureLibrary.Model;
using ArchitectureLibrary.Signals;
using ArchitectureLibrary.ViewModel;
using GBATool.Commands.Project;
using GBATool.Commands.Utils;
using GBATool.Models;
using GBATool.Signals;

namespace GBATool.ViewModels;

public class BuildProjectDialogViewModel : ViewModel
{
    private bool _keepWindowOpen;

    #region Commands
    public BuildProjectCommand BuildProjectCommand { get; } = new();
    public DispatchSignalCommand<CloseDialogSignal> CloseDialogCommand { get; } = new();
    #endregion

    #region get/set
    public bool KeepWindowOpen
    {
        get => _keepWindowOpen;
        set
        {
            _keepWindowOpen = value;

            ModelManager.Get<GBAToolConfigurationModel>().KeepBuildDialogOpen = value;
            ModelManager.Get<GBAToolConfigurationModel>().Save();

            OnPropertyChanged(nameof(KeepWindowOpen));
        }
    }
    #endregion

    public BuildProjectDialogViewModel()
    {
        #region Signals
        SignalManager.Get<CloseDialogSignal>().Listener += OnCloseDialog;
        #endregion

        KeepWindowOpen = ModelManager.Get<GBAToolConfigurationModel>().KeepBuildDialogOpen;
    }

    private void OnCloseDialog()
    {
        #region Signals
        SignalManager.Get<CloseDialogSignal>().Listener -= OnCloseDialog;
        #endregion
    }
}
