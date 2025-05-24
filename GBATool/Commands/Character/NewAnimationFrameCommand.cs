using ArchitectureLibrary.Commands;
using ArchitectureLibrary.Signals;
using GBATool.Models;
using GBATool.Signals;
using System;

namespace GBATool.Commands.Character;

public class NewAnimationFrameCommand : Command
{
    public override void Execute(object? parameter)
    {
        if (parameter == null)
        {
            return;
        }

        object[] values = (object[])parameter;

        FileHandler fileHandler = (FileHandler)values[0];
        string tabID = (string)values[1];

        if (fileHandler.FileModel is not CharacterModel model)
        {
            return;
        }

        foreach (var item in model.Animations)
        {
            CharacterAnimation animation = item.Value;

            if (animation.ID == tabID)
            {
                animation.Frames ??= [];

                FrameModel frame = new()
                {
                    ID = Guid.NewGuid().ToString(),
                };

                animation.Frames.Add(frame.ID, frame);

                fileHandler.Save();

                SignalManager.Get<NewAnimationFrameSignal>().Dispatch(tabID, frame.ID);

                return;
            }
        }
    }
}
