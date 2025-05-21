using ArchitectureLibrary.Commands;
using ArchitectureLibrary.Signals;
using GBATool.Signals;
using GBATool.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace GBATool.Commands.Character;

public class CharacterCloseTabCommand : Command
{
    public override void Execute(object? parameter)
    {
        if (parameter == null)
            return;

        MessageBoxResult result = MessageBox.Show("Are you sure you want to delete the animation tab?", "Delete", MessageBoxButton.YesNo);

        if (result == MessageBoxResult.Yes)
        {
            MouseButtonEventArgs? args = parameter as MouseButtonEventArgs;

            FrameworkElement? source = (FrameworkElement?)args?.OriginalSource;

            if (source?.DataContext is ActionTabItem tabItem)
            {
                SignalManager.Get<AnimationTabDeletedSignal>().Dispatch(tabItem);
            }
        }
    }
}
