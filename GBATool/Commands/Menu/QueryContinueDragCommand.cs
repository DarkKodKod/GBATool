using ArchitectureLibrary.Commands;
using ArchitectureLibrary.Signals;
using GBATool.Signals;
using System.Windows;

namespace GBATool.Commands
{
    public class QueryContinueDragCommand : Command
    {
        public override void Execute(object parameter)
        {
            QueryContinueDragEventArgs? dragEvent = parameter as QueryContinueDragEventArgs;

            if (dragEvent != null && dragEvent.EscapePressed)
            {
                dragEvent.Action = DragAction.Cancel;

                SignalManager.Get<DetachAdornersSignal>().Dispatch();

                dragEvent.Handled = true;
            }
        }
    }
}
