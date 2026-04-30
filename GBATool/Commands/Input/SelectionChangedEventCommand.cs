using ArchitectureLibrary.Commands;
using ArchitectureLibrary.Signals;
using GBATool.VOs;
using System.Windows.Controls;

namespace GBATool.Commands.Input;

public class SelectionChangedEventCommand<T> : Command where T : ISignal1<SelectionChangedVO>, new()
{
    public override void Execute(object? parameter)
    {
        if (parameter is not SelectionChangedEventArgs selectionChangedEvent)
        {
            return;
        }

        SelectionChangedVO vo = new(selectionChangedEvent)
        {
            OriginalSource = selectionChangedEvent.OriginalSource,
            Sender = selectionChangedEvent.Source
        };

        SignalManager.Get<T>().Dispatch(vo);
    }
}
