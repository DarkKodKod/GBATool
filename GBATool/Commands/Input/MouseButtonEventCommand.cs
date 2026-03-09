using ArchitectureLibrary.Commands;
using ArchitectureLibrary.Signals;
using GBATool.VOs;
using System.Windows.Input;

namespace GBATool.Commands.Input;

public class MouseButtonEventCommand<T> : Command where T : ISignal1<MouseButtonVO>, new()
{
    public override void Execute(object? parameter)
    {
        if (parameter is not MouseButtonEventArgs mouseEvent)
        {
            return;
        }

        MouseButtonVO vo = new(mouseEvent)
        {
            OriginalSource = mouseEvent.OriginalSource,
            Sender = mouseEvent.Source
        };

        SignalManager.Get<T>().Dispatch(vo);
    }
}
