using ArchitectureLibrary.Commands;
using ArchitectureLibrary.Signals;
using GBATool.Models;
using GBATool.Signals;
using System.Linq;

namespace GBATool.Commands.Character;

public class SwitchCharacterFrameViewCommand : Command
{
    public override void Execute(object? parameter)
    {
        if (parameter == null)
            return;

        object[] values = (object[])parameter;
        string AnimationID = (string)values[0];
        string frameID = (string)values[1];
        int frameIndex = (int)values[2];

        if (values.Length > 3 && values[3] is CharacterModel model)
        {
            // dont do anything if this is not an editable frame
            if (model.Animations[AnimationID].Frames[frameID].IsHeldFrame)
            {
                return;
            }
        }

        SignalManager.Get<SwitchCharacterFrameViewSignal>().Dispatch(AnimationID, frameID, frameIndex);
    }
}