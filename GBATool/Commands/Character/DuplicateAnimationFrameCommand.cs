using ArchitectureLibrary.Commands;
using ArchitectureLibrary.Signals;
using GBATool.Models;
using GBATool.Signals;
using System;
using System.Collections.Generic;

namespace GBATool.Commands.Character;

public class DuplicateAnimationFrameCommand : Command
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
        bool isNewFrameHeldFrame = false;

        if (model.Animations.TryGetValue(animationID, out CharacterAnimation? animation))
        {
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
                        BankID = item.Value.BankID,
                        IsHeldFrame = item.Value.IsHeldFrame,
                        CollisionInfo = new Dictionary<string, CharacterCollision?>(item.Value.CollisionInfo),
                        Tiles = new Dictionary<string, CharacterSprite>(item.Value.Tiles)
                    };

                    isNewFrameHeldFrame = item.Value.IsHeldFrame;

                    newFrames.Add(newFrameID, frame);
                }

                countFrames++;
            }

            if (!string.IsNullOrEmpty(newFrameID))
            {
                animation.Frames = newFrames;

                fileHandler.Save();

                SignalManager.Get<NewAnimationFrameSignal>().Dispatch(animation.ID, newFrameID, whereIsNewFrame, isNewFrameHeldFrame);
            }
        }
    }
}
