using ArchitectureLibrary.Commands;
using ArchitectureLibrary.Signals;
using GBATool.Models;
using GBATool.Signals;

namespace GBATool.Commands
{
    public class MoveDownSelectedSpriteElement : Command
    {
        public override bool CanExecute(object? parameter)
        {
            if (parameter == null)
            {
                return false;
            }

            if (parameter is not SpriteModel)
            {
                return false;
            }

            return true;
        }

        public override void Execute(object? parameter)
        {
            if (parameter is not SpriteModel model)
            {
                return;
            }

            SignalManager.Get<MoveDownSelectedSpriteElementSignal>().Dispatch(model);
        }
    }
}
