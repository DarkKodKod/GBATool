using ArchitectureLibrary.Model;
using ArchitectureLibrary.Signals;
using ArchitectureLibrary.ViewModel;
using GBATool.Commands;
using GBATool.Models;
using GBATool.Signals;
using System.Windows.Controls;

namespace GBATool.ViewModels;

public class BuildProjectDialogViewModel : ViewModel
{
    private string _folderSourcePath = "";
    private string _folderAssetsPath = "";

    #region Commands
    public BuildProjectCommand BuildProjectCommand { get; } = new();
    public BrowseFolderCommand BrowseFolderCommand { get; } = new();
    public DispatchSignalCommand<CloseDialogSignal> CloseDialogCommand { get; } = new();
    #endregion

    #region get/set
    public string FolderSourcePath
    {
        get => _folderSourcePath;
        set
        {
            _folderSourcePath = value;
            OnPropertyChanged("FolderSourcePath");
        }
    }

    public string FolderAssetsPath
    {
        get => _folderAssetsPath;
        set
        {
            _folderAssetsPath = value;
            OnPropertyChanged("FolderAssetsPath");
        }
    }
    #endregion

    public BuildProjectDialogViewModel()
    {
        #region Signals
        SignalManager.Get<BrowseFolderSuccessSignal>().Listener += BrowseFolderSuccess;
        SignalManager.Get<CloseDialogSignal>().Listener += OnCloseDialog;
        #endregion

        ProjectModel project = ModelManager.Get<ProjectModel>();

        FolderSourcePath = project.Build.GeneratedSourcePath;
        FolderAssetsPath = project.Build.GeneratedAssetsPath;
    }

    private void OnCloseDialog()
    {
        #region Signals
        SignalManager.Get<BrowseFolderSuccessSignal>().Listener -= BrowseFolderSuccess;
        SignalManager.Get<CloseDialogSignal>().Listener -= OnCloseDialog;
        #endregion
    }

    private void BrowseFolderSuccess(Control owner, string folderPath)
    {
        ProjectModel project = ModelManager.Get<ProjectModel>();

        if (owner.Name == "btnSourcePath")
        {
            if (project.Build.GeneratedSourcePath != folderPath)
            {
                FolderSourcePath = folderPath;

                project.Build.GeneratedSourcePath = folderPath;
            }
        }
        else if (owner.Name == "btnAssetsPath")
        {
            if (project.Build.GeneratedAssetsPath != folderPath)
            {
                FolderAssetsPath = folderPath;

                project.Build.GeneratedAssetsPath = folderPath;
            }
        }

        project.Save();
    }
}
