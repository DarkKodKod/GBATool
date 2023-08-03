using ArchitectureLibrary.Commands;
using ArchitectureLibrary.Signals;
using GBATool.Signals;
using System.Windows;

namespace GBATool.Commands
{
    public class DragLeaveCommand : Command
    {
        public override void Execute(object parameter)
        {
            if (parameter is DragEventArgs dragEvent)
            {
                SignalManager.Get<DetachAdornersSignal>().Dispatch();

                dragEvent.Handled = true;
            }
        }
    }
}
