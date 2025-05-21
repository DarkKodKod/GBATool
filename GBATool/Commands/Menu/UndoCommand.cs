using ArchitectureLibrary.Commands;
using ArchitectureLibrary.History;

namespace GBATool.Commands.Menu;

public class UndoCommand : Command
{
    public override bool CanExecute(object? parameter) => HistoryManager.IsUndoPossible();

    public override void Execute(object? parameter)
    {
        HistoryManager.Undo();
    }
}
