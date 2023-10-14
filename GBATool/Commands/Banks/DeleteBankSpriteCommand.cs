using ArchitectureLibrary.Commands;
using ArchitectureLibrary.Signals;
using GBATool.Models;
using GBATool.Signals;

namespace GBATool.Commands
{
    public class DeleteBankSpriteCommand : Command
    {
        public override bool CanExecute(object parameter)
        {
            if (parameter == null)
            {
                return false;
            }

            return parameter is SpriteModel;
        }

        public override void Execute(object parameter)
        {
            SpriteModel sprite = (SpriteModel)parameter;

            SignalManager.Get<BankSpriteDeletedSignal>().Dispatch(sprite);
        }
    }
}
