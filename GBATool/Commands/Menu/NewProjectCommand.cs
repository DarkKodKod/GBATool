using ArchitectureLibrary.Commands;
using GBATool.Views;
using System.Windows;

namespace GBATool.Commands.Menu;

public class NewProjectCommand : Command
{
    public override void Execute(object? parameter)
    {
        Window? window = parameter as Window;

        ProjectDialog dialog = new()
        {
            Owner = window
        };
        dialog.ShowDialog();
    }
}
