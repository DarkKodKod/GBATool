using ArchitectureLibrary.History;
using ArchitectureLibrary.Signals;
using GBATool.FileSystem;
using GBATool.Signals;
using GBATool.ViewModels;

namespace GBATool.HistoryActions
{
    public class DeleteProjectItemHitoryAction : IHistoryAction
    {
        private readonly ProjectItem _item;

        public DeleteProjectItemHitoryAction(ProjectItem item)
        {
            _item = item;
        }

        public void Redo()
        {
            ProjectItemFileSystem.DeteElement(_item);

            SignalManager.Get<DeleteElementSignal>().Dispatch(_item);
        }

        public void Undo()
        {
            if (_item.Parent != null) 
            {
                SignalManager.Get<PasteElementSignal>().Dispatch(_item.Parent, _item);
            }

            if (_item.FileHandler != null)
            {
                ProjectItemFileSystem.CreateElement(_item, _item.FileHandler.Path, _item.DisplayName);
            }   
        }
    }
}
