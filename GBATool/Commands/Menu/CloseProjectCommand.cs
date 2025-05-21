using ArchitectureLibrary.Commands;
using ArchitectureLibrary.Model;
using ArchitectureLibrary.Signals;
using GBATool.Models;
using GBATool.Signals;

namespace GBATool.Commands.Menu;

public class CloseProjectCommand : Command
{
    public override bool CanExecute(object? parameter)
    {
        if (parameter == null)
        {
            return false;
        }

        string? projectName = parameter as string;

        if (string.IsNullOrEmpty(projectName))
        {
            return false;
        }

        return true;
    }

    public override void Execute(object? parameter)
    {
        if (parameter == null)
        {
            return;
        }

        ProjectModel model = ModelManager.Get<ProjectModel>();

        if (model != null && !string.IsNullOrEmpty(model.Name))
        {
            model.Reset();

            SignalManager.Get<CloseProjectSuccessSignal>().Dispatch();
        }
    }
}
