using ArchitectureLibrary.History;
using ArchitectureLibrary.Signals;
using GBATool.FileSystem;
using GBATool.Signals;
using GBATool.ViewModels;

namespace GBATool.HistoryActions
{
    public class DuplicateProjectItemHistoryAction : IHistoryAction
    {
        private readonly ProjectItem _item;

        public DuplicateProjectItemHistoryAction(ProjectItem item)
        {
            _item = item;
        }

        public void Redo()
        {
            if (_item.Parent != null)
            {
                SignalManager.Get<PasteElementSignal>().Dispatch(_item.Parent, _item);
            }
        }

        public void Undo()
        {
            ProjectItemFileSystem.DeteElement(_item);

            SignalManager.Get<DeleteElementSignal>().Dispatch(_item);
        }
    }
}
