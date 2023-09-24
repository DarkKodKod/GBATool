using ArchitectureLibrary.Commands;
using ArchitectureLibrary.Signals;
using GBATool.Signals;

namespace GBATool.Commands
{
    public class SelectTileSetCommand : Command
    {
        public override void Execute(object parameter)
        {
            if (parameter is string id)
            {
                SignalManager.Get<SelectTileSetSignal>().Dispatch(id);
            }
        }
    }
}
