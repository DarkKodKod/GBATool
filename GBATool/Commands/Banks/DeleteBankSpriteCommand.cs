using ArchitectureLibrary.Commands;
using ArchitectureLibrary.Signals;
using GBATool.Signals;

namespace GBATool.Commands
{
    public class DeleteBankSpriteCommand : Command
    {
        public override void Execute(object parameter)
        {
            SignalManager.Get<BankSpriteDeletedSignal>().Dispatch();
        }
    }
}
