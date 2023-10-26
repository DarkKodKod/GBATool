using ArchitectureLibrary.History.Signals;
using ArchitectureLibrary.Signals;
using GBATool.FileSystem;
using GBATool.HistoryActions;
using GBATool.Signals;
using GBATool.Utils;
using GBATool.ViewModels;

namespace GBATool.Commands
{
    public class DuplicateElementCommand : ItemSelectedCommand
    {
        public override bool CanExecute(object? parameter)
        {
            if (ItemSelected == null)
            {
                return false;
            }

            if (ItemSelected.IsRoot || ItemSelected.IsFolder)
            {
                return false;
            }

            return true;
        }

        public override void Execute(object? parameter)
        {
            if (ItemSelected == null || ItemSelected.FileHandler == null)
            {
                return;
            }

            ProjectItem newItem = new(ItemSelected.GetContent());

            string extension = Util.GetExtensionByType(ItemSelected.Type);
            string newItemPath = ItemSelected.FileHandler.Path;
            string name = ProjectItemFileSystem.GetValidFileName(newItemPath, ItemSelected.DisplayName, extension);

            if (newItem.FileHandler != null)
            {
                newItem.FileHandler.Name = name;
                newItem.FileHandler.Path = newItemPath;
            }

            newItem.RenamedFromAction = true;
            newItem.DisplayName = name;
            newItem.IsLoaded = true;

            SignalManager.Get<RegisterHistoryActionSignal>().Dispatch(new DuplicateProjectItemHistoryAction(newItem));

            SignalManager.Get<PasteElementSignal>().Dispatch(ItemSelected, newItem);
        }
    }
}
