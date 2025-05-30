﻿using ArchitectureLibrary.Clipboard;

namespace GBATool.Commands.Menu;

public class CopyElementCommand : ItemSelectedCommand
{
    public override bool CanExecute(object? parameter)
    {
        if (parameter == null)
        {
            return false;
        }

        if (ItemSelected == null)
        {
            return false;
        }

        if (ItemSelected.IsRoot)
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

        if (ItemSelected == null)
        {
            return;
        }

        ClipboardManager.SetData(ItemSelected);
    }
}
