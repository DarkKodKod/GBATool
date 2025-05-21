using ArchitectureLibrary.Commands;
using ArchitectureLibrary.Signals;

namespace GBATool.Commands.Utils;

public class DispatchSignalCommand<T> : Command where T : ISignal, new()
{
    public override void Execute(object? parameter)
    {
        SignalManager.Get<T>().Dispatch();
    }
}
