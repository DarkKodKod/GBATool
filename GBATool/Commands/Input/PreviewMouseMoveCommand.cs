using ArchitectureLibrary.Commands;
using ArchitectureLibrary.Signals;
using GBATool.Signals;
using GBATool.VOs;
using System.Windows.Input;

namespace GBATool.Commands;

public class PreviewMouseMoveCommand : Command
{
    public override void Execute(object? parameter)
    {
        MouseEventArgs? mouseEvent = parameter as MouseEventArgs;

        if (mouseEvent?.LeftButton == MouseButtonState.Pressed)
        {
            MouseMoveVO vo = new()
            {
                Position = mouseEvent.GetPosition(null),
                OriginalSource = mouseEvent.OriginalSource,
                Sender = mouseEvent.Source
            };

            SignalManager.Get<MouseMoveSignal>().Dispatch(vo);
        }
    }
}
