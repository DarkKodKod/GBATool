using ArchitectureLibrary.History;
using GBATool.ViewModels;

namespace GBATool.HistoryActions;

public class RenameProjectItemHistoryAction(ProjectItem item, string oldName) : IHistoryAction
{
    private readonly ProjectItem _item = item;
    private readonly string _oldName = oldName;
    private readonly string _newName = item.DisplayName;

    public void Redo()
    {
        _item.RenamedFromAction = true;
        _item.DisplayName = _newName;
    }

    public void Undo()
    {
        _item.RenamedFromAction = true;
        _item.DisplayName = _oldName;
    }
}
