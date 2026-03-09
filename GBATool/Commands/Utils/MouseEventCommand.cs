using ArchitectureLibrary.Commands;
using ArchitectureLibrary.Signals;
using GBATool.VOs;
using System.Windows.Input;

namespace GBATool.Commands.Utils;

public class MouseEventCommand<T> : Command where T : ISignal1<MouseEventVO>, new()
{
    public override void Execute(object? parameter)
    {
        if (parameter is not MouseEventArgs mouseEvent)
        {
            return;
        }

        MouseEventVO vo = new(mouseEvent)
        {
            OriginalSource = mouseEvent.OriginalSource,
            Sender = mouseEvent.Source
        };

        SignalManager.Get<T>().Dispatch(vo);
    }
}
