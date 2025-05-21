using ArchitectureLibrary.Commands;
using ArchitectureLibrary.Signals;
using GBATool.Signals;

namespace GBATool.Commands.Windows;

public class GoToProjectItemCommand : Command
{
    public override void Execute(object? parameter)
    {
        if (parameter is string id)
        {
            SignalManager.Get<GotoProjectItemSignal>().Dispatch(id);
        }
    }
}
