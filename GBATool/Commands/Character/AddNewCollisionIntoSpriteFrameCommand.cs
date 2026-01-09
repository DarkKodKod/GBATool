using ArchitectureLibrary.Commands;
using ArchitectureLibrary.Signals;
using GBATool.Enums;
using GBATool.Signals;
using GBATool.Utils;
using GBATool.VOs;
using System;
using System.Windows.Media;

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

        SpriteCollisionVO collisionVO = new()
        {
            ID = Guid.NewGuid().ToString(),
            Width = 50,
            Height = 50,
            PosX = (CharacterUtils.CanvasWidth / 2) / 2,
            PosY = (CharacterUtils.CanvasHeight / 2) / 2,
            Color = new SolidColorBrush(Color.FromArgb(50, 255, 0, 0)),
            Mask = CollisionMask.Hitbox,
            CustomMask = 0,
            AnimationID = animationID,
            FrameID = frameID
        };

        SignalManager.Get<NewCollisionIntoSpriteSignal>().Dispatch(animationID, frameID, collisionVO);
    }
}
