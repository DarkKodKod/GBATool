using ArchitectureLibrary.Commands;
using ArchitectureLibrary.Signals;
using GBATool.Signals;

namespace GBATool.Commands.Banks;

public class SelectTileSetCommand : Command
{
    public override void Execute(object? parameter)
    {
        if (parameter == null)
        {
            return;
        }

        if (parameter is string id)
        {
            SignalManager.Get<SelectTileSetSignal>().Dispatch(id);
        }
    }
}
