using ArchitectureLibrary.Signals;
using ArchitectureLibrary.ViewModel;
using GBATool.Commands.FileSystem;
using GBATool.Commands.Menu;
using GBATool.Commands.Utils;
using GBATool.Signals;
using GBATool.Utils;
using System.Windows.Controls;

namespace GBATool.ViewModels;

public class ProjectDialogViewModel : ViewModel
{
    public CreateProjectCommand CreateProjectCommand { get; } = new();
    public BrowseFolderCommand BrowseFolderCommand { get; } = new();
    public DispatchSignalCommand<CloseDialogSignal> CloseDialogCommand { get; } = new();

    #region get/set
    public string ProjectName
    {
        get => _projectName;
        set
        {
            if (!Util.IsValidFileName(value))
                return;

            _projectName = value;
            OnPropertyChanged(nameof(ProjectName));
        }
    }

    public string FolderPath
    {
        get => _folderPath;
        set
        {
            _folderPath = value;
            OnPropertyChanged(nameof(FolderPath));
        }
    }
    #endregion

    private string _projectName = string.Empty;
    private string _folderPath = string.Empty;

    public ProjectDialogViewModel()
    {
        SignalManager.Get<BrowseFolderSuccessSignal>().Listener += BrowseFolderSuccess;
        SignalManager.Get<CloseDialogSignal>().Listener += OnCloseDialog;
    }

    private void OnCloseDialog()
    {
        SignalManager.Get<BrowseFolderSuccessSignal>().Listener -= BrowseFolderSuccess;
        SignalManager.Get<CloseDialogSignal>().Listener -= OnCloseDialog;
    }

    private void BrowseFolderSuccess(Control owner, string folderPath) => FolderPath = folderPath;
}
