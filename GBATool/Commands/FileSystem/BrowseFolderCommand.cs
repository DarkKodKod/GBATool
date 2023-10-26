using ArchitectureLibrary.Commands;
using ArchitectureLibrary.Signals;
using GBATool.Signals;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace GBATool.Commands
{
    public class BrowseFolderCommand : Command
    {
        public override void Execute(object? parameter)
        {
            if (parameter == null)
            {
                return;
            }

            CommonOpenFileDialog dialog = new()
            {
                Title = "Select folder",
                IsFolderPicker = true,
                InitialDirectory = parameter as string,
                AddToMostRecentlyUsedList = false,
                AllowNonFileSystemItems = false,
                DefaultDirectory = parameter as string,
                EnsureFileExists = true,
                EnsurePathExists = true,
                EnsureReadOnly = false,
                EnsureValidNames = true,
                Multiselect = false,
                ShowPlacesList = true
            };

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                SignalManager.Get<BrowseFolderSuccessSignal>().Dispatch(dialog.FileName);
            }
        }
    }
}
