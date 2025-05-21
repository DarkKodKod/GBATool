using ArchitectureLibrary.Commands;
using ArchitectureLibrary.Signals;
using GBATool.Signals;
using System.Windows;

namespace GBATool.Commands.Menu;

public class QueryContinueDragCommand : Command
{
    public override void Execute(object? parameter)
    {
        if (parameter is QueryContinueDragEventArgs dragEvent && dragEvent.EscapePressed)
        {
            dragEvent.Action = DragAction.Cancel;

            SignalManager.Get<DetachAdornersSignal>().Dispatch();

            dragEvent.Handled = true;
        }
    }
}
