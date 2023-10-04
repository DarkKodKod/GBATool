using ArchitectureLibrary.Commands;
using ArchitectureLibrary.Signals;
using GBATool.Models;
using GBATool.Signals;

namespace GBATool.Commands
{
    public class MoveSpriteToBankCommand : Command
    {
        public override bool CanExecute(object parameter)
        {
            if (parameter == null)
            {
                return false;
            }

            object[] values = (object[])parameter;
            BankModel? model = (BankModel?)values[0];

            if (values[1] == null)
            {
                return false;
            }

            return model != null && !model.IsFull;
        }

        public override void Execute(object parameter)
        {
            object[] values = (object[])parameter;

            BankModel model = (BankModel)values[0];
            SpriteModel sprite = (SpriteModel)values[1];

            model.RegisterSprite(sprite);

            SignalManager.Get<BankImageUpdatedSignal>().Dispatch();
        }
    }
}
