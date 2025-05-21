using ArchitectureLibrary.Commands;
using ArchitectureLibrary.Signals;
using GBATool.Signals;
using System.Windows;

namespace GBATool.Commands.Character;

public class CharacterNewTabCommand : Command
{
    public override void Execute(object? parameter)
    {
        if (parameter == null)
            return;

        MessageBoxResult result = MessageBox.Show("Do you want to create a new animation?", "Create animation", MessageBoxButton.YesNo);

        if (result == MessageBoxResult.Yes)
        {
            SignalManager.Get<AnimationTabNewSignal>().Dispatch();
        }
    }
}
