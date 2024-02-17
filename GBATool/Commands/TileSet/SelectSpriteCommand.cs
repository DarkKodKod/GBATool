using ArchitectureLibrary.Commands;
using ArchitectureLibrary.Signals;
using GBATool.Signals;
using GBATool.VOs;

namespace GBATool.Commands;

public class SelectSpriteCommand : Command
{
    public override void Execute(object? parameter)
    {
        SpriteVO? spriteVO = parameter as SpriteVO;

        if (spriteVO != null)
        {
            SignalManager.Get<SelectSpriteSignal>().Dispatch(spriteVO);
        }
    }
}
