using ArchitectureLibrary.Commands;
using ArchitectureLibrary.Signals;
using GBATool.Signals;
using GBATool.VOs;
using System.Windows.Input;

namespace GBATool.Commands.Input;

public class MouseWheelEventCommand : Command
{
    public override void Execute(object? parameter)
    {
        if (parameter is not MouseWheelEventArgs wheelEvent)
        {
            return;
        }

        MouseWheelVO vo = new(wheelEvent)
        {
            Delta = wheelEvent.Delta,
            OriginalSource = wheelEvent.OriginalSource,
            Sender = wheelEvent.Source
        };

        SignalManager.Get<MouseWheelSignal>().Dispatch(vo);
    }
}
