using ArchitectureLibrary.Commands;
using ArchitectureLibrary.Signals;
using GBATool.Models;
using GBATool.Signals;
using System.Windows;

namespace GBATool.Commands
{
    public class MoveSpriteToBankCommand : Command
    {
        public override bool CanExecute(object? parameter)
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

        public override void Execute(object? parameter)
        {
            if (parameter == null)
            {
                return;
            }

            object[] values = (object[])parameter;

            BankModel model = (BankModel)values[0];
            SpriteModel sprite = (SpriteModel)values[1];

            (bool, string) ret = model.RegisterSprite(sprite);

            if (ret.Item1 == true)
            {
                SignalManager.Get<BankImageUpdatedSignal>().Dispatch();
            }
            else
            {
                MessageBox.Show(ret.Item2, "Error", MessageBoxButton.OK);
            }
        }
    }
}
