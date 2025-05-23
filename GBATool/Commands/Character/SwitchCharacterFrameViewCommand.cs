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
        int frameIndex = (int)values[1];

        SignalManager.Get<SwitchCharacterFrameViewSignal>().Dispatch(tabID, frameIndex);
    }
}