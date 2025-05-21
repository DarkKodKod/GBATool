using ArchitectureLibrary.Commands;
using ArchitectureLibrary.Signals;
using GBATool.Signals;
using GBATool.VOs;
using System.Windows.Input;

namespace GBATool.Commands.Input;

public class MouseLeaveCommand : Command
{
    public override void Execute(object? parameter)
    {
        if (parameter is MouseEventArgs mouseEvent)
        {
            MouseLeaveVO vo = new()
            {
                OriginalSource = mouseEvent.OriginalSource,
                Sender = mouseEvent.Source
            };

            SignalManager.Get<MouseLeaveSignal>().Dispatch(vo);
        }
    }
}
