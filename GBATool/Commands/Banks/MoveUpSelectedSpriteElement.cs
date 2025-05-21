﻿using ArchitectureLibrary.Commands;
using ArchitectureLibrary.Signals;
using GBATool.Models;
using GBATool.Signals;
using System.Collections.Generic;
using System.Linq;

namespace GBATool.Commands.Banks;

public class MoveUpSelectedSpriteElement : Command
{
    public override bool CanExecute(object? parameter)
    {
        if (parameter == null)
        {
            return false;
        }

        object[] values = (object[])parameter;

        if (values[0] is not SpriteModel selectedModel)
        {
            return false;
        }

        if (values[1] is not IEnumerable<SpriteModel> listOfSprites)
        {
            return false;
        }

        if (selectedModel == listOfSprites.First())
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

        if (values[0] is not SpriteModel model)
        {
            return;
        }

        if (values[1] is not IEnumerable<SpriteModel> listOfSprites)
        {
            return;
        }

        int itemAtIndex = 0;

        for (int i = 0; i < listOfSprites.Count(); i++)
        {
            if (model == listOfSprites.ElementAt(i))
            {
                itemAtIndex = i;
                break;
            }
        }

        SignalManager.Get<MoveUpSelectedSpriteElementSignal>().Dispatch(itemAtIndex);
    }
}
