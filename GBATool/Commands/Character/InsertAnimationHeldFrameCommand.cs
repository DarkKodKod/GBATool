using ArchitectureLibrary.Commands;
using ArchitectureLibrary.Signals;
using GBATool.Models;
using GBATool.Signals;
using System;
using System.Collections.Generic;

namespace GBATool.Commands.Character;

public class InsertAnimationHeldFrameCommand : Command
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
        FileHandler fileHandler = (FileHandler)values[2];

        if (fileHandler.FileModel is not CharacterModel model)
        {
            return;
        }

        Dictionary<string, FrameModel> newFrames = [];
        string newFrameID = string.Empty;

        if (!model.Animations.TryGetValue(animationID, out CharacterAnimation? animation))
        {
            return;
        }

        int whereIsNewFrame = 0;
        int countFrames = 0;

        foreach (KeyValuePair<string, FrameModel> item in animation.Frames)
        {
            newFrames.Add(item.Key, item.Value);

            if (item.Value.ID == frameID)
            {
                whereIsNewFrame = countFrames + 1;

                newFrameID = Guid.NewGuid().ToString();

                // insert here
                FrameModel frame = new()
                {
                    ID = newFrameID,
                    IsHeldFrame = true
                };

                newFrames.Add(newFrameID, frame);
            }

            countFrames++;
        }

        if (!string.IsNullOrEmpty(newFrameID))
        {
            animation.Frames = newFrames;

            fileHandler.Save();

            SignalManager.Get<NewAnimationFrameSignal>().Dispatch(animation.ID, newFrameID, whereIsNewFrame, true);
        }
    }
}
