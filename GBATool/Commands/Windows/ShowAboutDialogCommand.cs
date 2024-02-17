using ArchitectureLibrary.Commands;
using GBATool.Views;
using System.Windows;

namespace GBATool.Commands;

public class ShowAboutDialogCommand : Command
{
    public override void Execute(object? parameter)
    {
        Window? window = parameter as Window;

        AboutDialog dialog = new()
        {
            Owner = window
        };
        dialog.ShowDialog();
    }
}
