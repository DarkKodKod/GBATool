using ArchitectureLibrary.Commands;
using ArchitectureLibrary.Model;
using ArchitectureLibrary.Signals;
using GBATool.Models;
using GBATool.Signals;

namespace GBATool.Commands
{
    public class CloseProjectCommand : Command
    {
        public override bool CanExecute(object parameter)
        {
            string? projectName = parameter as string;

            if (string.IsNullOrEmpty(projectName))
            {
                return false;
            }

            return true;
        }

        public override void Execute(object parameter)
        {
            ProjectModel model = ModelManager.Get<ProjectModel>();

            if (model != null && !string.IsNullOrEmpty(model.Name))
            {
                model.Reset();

                SignalManager.Get<CloseProjectSuccessSignal>().Dispatch();
            }
        }
    }
}
