using ArchitectureLibrary.Commands;
using ArchitectureLibrary.Signals;
using GBATool.VOs;
using System.Windows;
using System.Windows.Input;

namespace GBATool.Commands.Input;

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
            AbsolutePosition = mouseEvent.GetPosition(null),
            RelativePosition = Mouse.GetPosition((IInputElement)mouseEvent.Source),
            OriginalSource = mouseEvent.OriginalSource,
            Sender = mouseEvent.Source,
            LeftButton = mouseEvent.LeftButton
        };

        SignalManager.Get<T>().Dispatch(vo);
    }
}
