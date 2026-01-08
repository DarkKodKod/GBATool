using ArchitectureLibrary.Commands;
using ArchitectureLibrary.Signals;
using GBATool.Signals;

namespace GBATool.Commands.Character;

public class AddNewCollisionIntoSpriteFrameCommand : Command
{
    public override void Execute(object? parameter)
    {
        if (parameter == null)
        {
            return;
        }

        object[] values = (object[])parameter;

        string animationID = (string)values[0];
        string frameID = (string)values[1];

        SignalManager.Get<NewCollisionIntoSpriteSignal>().Dispatch(animationID, frameID);
    }
}
