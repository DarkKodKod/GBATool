﻿using ArchitectureLibrary.Commands;
using ArchitectureLibrary.Signals;
using GBATool.Signals;

namespace GBATool.Commands.Banks;

public class GeneratePaletteFromBankCommand : Command
{
    public override void Execute(object? parameter)
    {
        SignalManager.Get<GeneratePaletteFromBankSignal>().Dispatch();
    }
}
