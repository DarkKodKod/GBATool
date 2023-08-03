using ArchitectureLibrary.Model;
using ArchitectureLibrary.Signals;
using ArchitectureLibrary.ViewModel;
using GBATool.Commands;
using GBATool.Models;
using GBATool.Signals;

namespace GBATool.ViewModels
{
    public class ProjectPropertiesDialogViewModel : ViewModel
    {
        public DispatchSignalCommand<CloseDialogSignal> CloseDialogCommand { get; } = new();

        private bool _changed = false;

        public ProjectPropertiesDialogViewModel()
        {
            ReadProjectData();

            SignalManager.Get<CloseDialogSignal>().Listener += OnCloseDialog;

            _changed = false;
        }

        private void OnCloseDialog()
        {
            if (_changed)
            {
                // Save all changes
                ProjectModel project = ModelManager.Get<ProjectModel>();

                project.Save();
            }

            SignalManager.Get<CloseDialogSignal>().Listener -= OnCloseDialog;
        }

        private void ReadProjectData()
        {
            ProjectModel project = ModelManager.Get<ProjectModel>();
        }
    }
}
