using ArchitectureLibrary.Commands;
using ArchitectureLibrary.Signals;
using GBATool.Signals;
using GBATool.VOs;
using System.Windows;
using System.Windows.Input;

namespace GBATool.Commands.Character;

public class DeleteCollisionCommand : Command
{
    public override void Execute(object? parameter)
    {
        MessageBoxResult result = MessageBox.Show("Are you sure you want to delete this collision?", "Delete", MessageBoxButton.YesNo);

        if (result != MessageBoxResult.Yes)
        {
            return;
        }

        MouseButtonEventArgs? args = parameter as MouseButtonEventArgs;

        FrameworkElement? source = args?.OriginalSource as FrameworkElement;

        if (source?.DataContext is SpriteCollisionVO collision)
        {
            SignalManager.Get<DeleteCollisionSignal>().Dispatch(collision);
        }
    }
}
