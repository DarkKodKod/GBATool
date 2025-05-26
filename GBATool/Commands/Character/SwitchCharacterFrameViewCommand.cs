using ArchitectureLibrary.Commands;
using ArchitectureLibrary.Signals;
using GBATool.Signals;

namespace GBATool.Commands.Character;

public class SwitchCharacterFrameViewCommand : Command
{
    public override void Execute(object? parameter)
    {
        if (parameter == null)
            return;

        object[] values = (object[])parameter;
        string tabID = (string)values[0];
        string frameID = (string)values[1];
        int frameIndex = (int)values[2];

        SignalManager.Get<SwitchCharacterFrameViewSignal>().Dispatch(tabID, frameID, frameIndex);
    }
}