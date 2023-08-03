using ArchitectureLibrary.Model;
using ArchitectureLibrary.Signals;
using ArchitectureLibrary.ViewModel;
using GBATool.Commands;
using GBATool.Models;
using GBATool.Signals;

namespace GBATool.ViewModels
{
    public class BuildProjectDialogViewModel : ViewModel
    {
        private string _folderPath = "";

        #region Commands
        public BuildProjectCommand BuildProjectCommand { get; } = new BuildProjectCommand();
        public BrowseFolderCommand BrowseFolderCommand { get; } = new BrowseFolderCommand();
        public DispatchSignalCommand<CloseDialogSignal> CloseDialogCommand { get; } = new DispatchSignalCommand<CloseDialogSignal>();
        #endregion

        #region get/set
        public string FolderPath
        {
            get => _folderPath;
            set
            {
                _folderPath = value;
                OnPropertyChanged("FolderPath");
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

            FolderPath = project.Build.OutputFilePath;
        }

        private void OnCloseDialog()
        {
            #region Signals
            SignalManager.Get<BrowseFolderSuccessSignal>().Listener -= BrowseFolderSuccess;
            SignalManager.Get<CloseDialogSignal>().Listener -= OnCloseDialog;
            #endregion
        }

        private void BrowseFolderSuccess(string folderPath)
        {
            ProjectModel project = ModelManager.Get<ProjectModel>();

            if (project.Build.OutputFilePath != folderPath)
            {
                FolderPath = folderPath;

                project.Build.OutputFilePath = folderPath;

                project.Save();
            }
        }
    }
}
