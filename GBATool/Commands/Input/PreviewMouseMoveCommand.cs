using ArchitectureLibrary.Commands;
using ArchitectureLibrary.Signals;
using GBATool.Signals;
using GBATool.VOs;
using System.Windows;
using System.Windows.Input;

namespace GBATool.Commands.Input;

public class PreviewMouseMoveCommand : Command
{
    public override void Execute(object? parameter)
    {
        if (parameter is MouseEventArgs mouseEvent)
        {
            MouseMoveVO vo = new()
            {
                AbsolutePosition = mouseEvent.GetPosition(null),
                RelativePosition = Mouse.GetPosition((IInputElement)mouseEvent.Source),
                OriginalSource = mouseEvent.OriginalSource,
                Sender = mouseEvent.Source,
                LeftButton = mouseEvent.LeftButton
            };

            SignalManager.Get<MouseMoveSignal>().Dispatch(vo);
        }
    }
}
