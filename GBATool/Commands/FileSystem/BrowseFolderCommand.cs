using ArchitectureLibrary.Commands;
using ArchitectureLibrary.Signals;
using GBATool.Signals;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Windows;
using System.Windows.Controls;

namespace GBATool.Commands;

public class BrowseFolderCommand : Command
{
    public override void Execute(object? parameter)
    {
        if (parameter == null)
        {
            return;
        }

        object[] values = (object[])parameter;

        Control ownerControl = (Control)values[0];
        string path = (string)values[1];

        CommonOpenFileDialog dialog = new()
        {
            Title = "Select folder",
            IsFolderPicker = true,
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

        if (dialog.ShowDialog(Application.Current.MainWindow) == CommonFileDialogResult.Ok)
        {
            SignalManager.Get<BrowseFolderSuccessSignal>().Dispatch(ownerControl, dialog.FileName);
        }
    }
}
