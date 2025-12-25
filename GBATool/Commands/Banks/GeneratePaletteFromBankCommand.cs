using ArchitectureLibrary.Commands;
using ArchitectureLibrary.Signals;
using GBATool.Enums;
using GBATool.Models;
using GBATool.Signals;
using System.Collections.Generic;
using System.Windows.Media;

namespace GBATool.Commands.Banks;

public class GeneratePaletteFromBankCommand : Command
{
    public override bool CanExecute(object? parameter)
    {
        if (parameter == null)
        {
            return false;
        }

        object[] values = (object[])parameter;

        if (values[1] is not IEnumerable<SpriteModel> _)
        {
            return false;
        }

        return true;
    }

    public override void Execute(object? parameter)
    {
        if (parameter == null)
        {
            return;
        }

        object[] values = (object[])parameter;

        if (values[0] is not string name)
        {
            return;
        }

        if (values[1] is not IEnumerable<SpriteModel> listOfSprites)
        {
            return;
        }

        if (values[2] is not Color transparentColor)
        {
            return;
        }

        if (values[3] is not BitsPerPixel bpp)
        {
            return;
        }

        SignalManager.Get<GeneratePaletteFromBankSignal>().Dispatch(name, listOfSprites, transparentColor, bpp);
    }
}
