﻿using ArchitectureLibrary.History.Signals;
using ArchitectureLibrary.Signals;
using GBATool.FileSystem;
using GBATool.HistoryActions;
using GBATool.Signals;
using GBATool.ViewModels;
using System.IO;
using System.Windows;

namespace GBATool.Commands.Menu;

public class CreateFolderCommand : ItemSelectedCommand
{
    private const string _folderNameKey = "NewFolderName";

    private readonly string _newFolderName;

    public CreateFolderCommand()
    {
        _newFolderName = (string)Application.Current.FindResource(_folderNameKey);
    }

    public override bool CanExecute(object? parameter)
    {
        if (ItemSelected != null)
        {
            if (ItemSelected.IsFolder)
            {
                return true;
            }
        }

        return false;
    }

    public override void Execute(object? parameter)
    {
        if (ItemSelected == null || ItemSelected.FileHandler == null)
        {
            return;
        }

        string path = Path.Combine(ItemSelected.FileHandler.Path, ItemSelected.FileHandler.Name);

        string name = ProjectItemFileSystem.GetValidFolderName(path, _newFolderName);

        ProjectItem newFolder = new()
        {
            DisplayName = name,
            IsFolder = true,
            Parent = ItemSelected,
            IsRoot = false,
            Type = ItemSelected.Type
        };

        ItemSelected.Items.Add(newFolder);

        SignalManager.Get<RegisterHistoryActionSignal>().Dispatch(new CreateNewElementHistoryAction(newFolder));

        SignalManager.Get<CreateNewElementSignal>().Dispatch(newFolder);

        ProjectItemFileSystem.CreateFileElement(newFolder, path, name);
    }
}
