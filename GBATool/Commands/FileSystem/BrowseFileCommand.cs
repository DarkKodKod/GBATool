﻿using ArchitectureLibrary.Commands;
using ArchitectureLibrary.Signals;
using GBATool.Signals;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.IO;

namespace GBATool.Commands.FileSystem;

public class BrowseFileCommand : Command
{
    public override void Execute(object? parameter)
    {
        if (parameter == null)
        {
            return;
        }

        object[] values = (object[])parameter;
        string? path = (string)values[0];
        string[]? filters = null;
        // New file here is a hardcoded way to identify the origin of the call, so I dont execute new file in multiple locations
        // When newFile is false, that means it was called by the TileSetViewModel and when is true it was called by
        // ImportImageDialogViewModel (of course this is not ok).
        bool newFile = (bool)values[2];

        if (values.Length > 1)
        {
            filters = (string[])values[1];
        }

        if (!string.IsNullOrEmpty(path))
        {
            path = Path.GetFullPath(path);
            path = Path.GetDirectoryName(path);
        }

        CommonOpenFileDialog dialog = new()
        {
            Title = "Select File",
            IsFolderPicker = false,
            InitialDirectory = path,
            AddToMostRecentlyUsedList = false,
            AllowNonFileSystemItems = false,
            DefaultDirectory = path,
            EnsureFileExists = true,
            EnsurePathExists = true,
            EnsureReadOnly = false,
            EnsureValidNames = true,
            Multiselect = false,
            ShowPlacesList = true
        };

        if (filters != null && filters.Length > 0)
        {
            for (int i = 0; i < filters.Length; i += 2)
            {
                dialog.Filters.Add(new CommonFileDialogFilter(filters[i], filters[i + 1]));
            }
        }

        if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
        {
            SignalManager.Get<BrowseFileSuccessSignal>().Dispatch(dialog.FileName, newFile);
        }
    }
}
