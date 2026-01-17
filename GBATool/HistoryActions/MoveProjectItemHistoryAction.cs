using ArchitectureLibrary.History;
using ArchitectureLibrary.Signals;
using GBATool.Signals;
using GBATool.ViewModels;

namespace GBATool.HistoryActions;

public class MoveProjectItemHistoryAction(ProjectItem dropTarget, ProjectItem item, ProjectItem origin, string newName, string oldName) : IHistoryAction
{
    private readonly ProjectItem _item = item;
    private readonly ProjectItem _dropTarget = dropTarget;
    private readonly ProjectItem _origin = origin;
    private readonly string _newName = newName;
    private readonly string _oldName = oldName;

    public void Redo()
    {
        _item.IsSelected = false;

        _item.RenamedFromAction = true;
        _item.DisplayName = _newName;

        SignalManager.Get<DropElementSignal>().Dispatch(_dropTarget, _item);
        SignalManager.Get<MoveElementSignal>().Dispatch(_dropTarget, _item);
    }

    public void Undo()
    {
        _item.IsSelected = false;

        _item.RenamedFromAction = true;
        _item.DisplayName = _oldName;

        SignalManager.Get<DropElementSignal>().Dispatch(_origin, _item);
        SignalManager.Get<MoveElementSignal>().Dispatch(_origin, _item);
    }
}
