using ArchitectureLibrary.Commands;
using ArchitectureLibrary.Signals;
using GBATool.Models;

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

            if (values[2] == null)
            {
                return false;
            }

            return model != null && !model.IsFull();
        }

        public override void Execute(object parameter)
        {
            object[] values = (object[])parameter;

            BankModel model = (BankModel)values[0];
            string tileSetId = (string)values[1];
            SpriteModel sprite = (SpriteModel)values[2];

            model.RegisterSprite(tileSetId, sprite);

            SignalManager.Get<BankImageUpdatedSignal>().Dispatch();
        }
    }
}
