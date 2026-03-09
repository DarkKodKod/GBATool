using ArchitectureLibrary.Commands;
using ArchitectureLibrary.Signals;
using GBATool.VOs;
using System.Windows;

namespace GBATool.Commands.Input;

public class DragEventCommand<T> : Command where T : ISignal1<DragLeaveVO>, new()
{
    public override void Execute(object? parameter)
    {
        if (parameter is not DragEventArgs mouseEvent)
        {
            return;
        }

        DragLeaveVO vo = new(mouseEvent)
        {
            OriginalSource = mouseEvent.OriginalSource,
            Sender = mouseEvent.Source
        };

        SignalManager.Get<T>().Dispatch(vo);
    }
}
