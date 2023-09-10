using ArchitectureLibrary.Signals;
using ArchitectureLibrary.ViewModel;
using GBATool.Commands;
using GBATool.Signals;
using GBATool.Utils;

namespace GBATool.ViewModels
{
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
                if (!Util.ValidFileName(value))
                    return;

                _projectName = value;
                _previousValidName = value;
                OnPropertyChanged("ProjectName");
            }
        }

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

        private string _previousValidName = string.Empty;
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

        private void BrowseFolderSuccess(string folderPath) => FolderPath = folderPath;
    }
}
