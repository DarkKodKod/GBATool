using ArchitectureLibrary.Commands;
using ArchitectureLibrary.Signals;
using GBATool.Models;
using GBATool.Signals;

namespace GBATool.Commands.Character;

public class PauseCharacterAnimationCommand : Command
{
    public override bool CanExecute(object? parameter)
    {
        if (parameter == null)
        {
            return false;
        }

        object[] values = (object[])parameter;
        CharacterModel model = (CharacterModel)values[0];

        if (model == null)
        {
            return false;
        }

        string tabID = (string)values[1];

        if (model.Animations.TryGetValue(tabID, out CharacterAnimation? animation))
        {
            return animation.Frames.Count > 1;
        }

        return false;
    }

    public override void Execute(object? parameter)
    {
        if (parameter == null)
            return;

        object[] values = (object[])parameter;
        string tabID = (string)values[1];

        SignalManager.Get<PauseCharacterAnimationSignal>().Dispatch(tabID);
    }
}
