using ArchitectureLibrary.Commands;
using ArchitectureLibrary.Signals;
using GBATool.Enums;
using GBATool.Signals;
using GBATool.Utils;
using GBATool.VOs;
using System;
using System.Windows.Media;

namespace GBATool.Commands.Character;

public class PasteCollisionsCommand : Command
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

        SignalManager.Get<PasteCollisionsSignal>().Dispatch(animationID, frameID);
    }
}
