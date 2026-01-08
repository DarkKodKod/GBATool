using ArchitectureLibrary.Commands;
using ArchitectureLibrary.Signals;
using GBATool.Signals;
using System.Windows;

namespace GBATool.Commands.Character;

public class DeleteCollisionCommand : Command
{
    public override void Execute(object? parameter)
    {
        MessageBoxResult result = MessageBox.Show("Are you sure you want to delete this collision?", "Delete", MessageBoxButton.YesNo);

        if (result != MessageBoxResult.Yes)
        {
            return;
        }

        if (parameter == null)
        {
            return;
        }

        object[] values = (object[])parameter;

        string animationID = (string)values[0];
        string frameID = (string)values[1];
        string collisionID = (string)values[2];

        SignalManager.Get<DeleteCollisionSignal>().Dispatch(animationID, frameID, collisionID);
    }
}
