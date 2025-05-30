﻿using ArchitectureLibrary.Clipboard;
using ArchitectureLibrary.History.Signals;
using ArchitectureLibrary.Signals;
using GBATool.FileSystem;
using GBATool.HistoryActions;
using GBATool.Signals;
using GBATool.Utils;
using GBATool.ViewModels;
using System.IO;

namespace GBATool.Commands.Menu;

public class PasteElementCommand : ItemSelectedCommand
{
    public override bool CanExecute(object? parameter)
    {
        if (ClipboardManager.IsEmpty() || ItemSelected == null)
        {
            return false;
        }

        // Only is possible the paste element if is in a place with the same type
        if (ClipboardManager.GetData() is ProjectItem clipbr && clipbr.Type != ItemSelected.Type)
        {
            return false;
        }

        return true;
    }

    public override void Execute(object? parameter)
    {
        if (ItemSelected?.FileHandler == null)
        {
            return;
        }

        if (ClipboardManager.GetData() is ProjectItem newItem)
        {
            string newItemPath;
            if (ItemSelected.IsFolder)
            {
                newItemPath = Path.Combine(ItemSelected.FileHandler.Path, ItemSelected.FileHandler.Name);
            }
            else
            {
                newItemPath = ItemSelected.FileHandler.Path;
            }

            string name;
            if (newItem.IsFolder)
            {
                name = ProjectItemFileSystem.GetValidFolderName(newItemPath, newItem.DisplayName);
            }
            else
            {
                string extension = Util.GetExtensionByType(ItemSelected.Type);

                name = ProjectItemFileSystem.GetValidFileName(newItemPath, newItem.DisplayName, extension);
            }

            if (newItem.FileHandler != null)
            {
                newItem.FileHandler.Name = name;
                newItem.FileHandler.Path = newItemPath;
            }

            newItem.RenamedFromAction = true;
            newItem.DisplayName = name;
            newItem.IsLoaded = true;

            SignalManager.Get<RegisterHistoryActionSignal>().Dispatch(new PasteProjectItemHistoryAction(newItem));

            SignalManager.Get<PasteElementSignal>().Dispatch(ItemSelected, newItem);
        }
    }
}
