using ArchitectureLibrary.Commands;
using ArchitectureLibrary.Signals;
using GBATool.Models;
using GBATool.Signals;

namespace GBATool.Commands.Character;

public class NewAnimationFrameCommand : Command
{
    public override void Execute(object? parameter)
    {
        if (parameter == null)
            return;

        object[] values = (object[])parameter;

        FileHandler fileHandler = (FileHandler)values[0];
        string tabID = (string)values[1];

        if (fileHandler.FileModel is not CharacterModel model)
            return;

        for (int i = 0; i < model.Animations.Count; ++i)
        {
            CharacterAnimation animation = model.Animations[i];

            if (animation.ID == tabID)
            {
                animation.Frames ??= [];

                FrameModel frame = new()
                {
                    Tiles = [],
                    FixToGrid = true
                };

                animation.Frames.Add(frame);

                fileHandler.Save();

                SignalManager.Get<NewAnimationFrameSignal>().Dispatch(tabID);

                return;
            }
        }
    }
}
