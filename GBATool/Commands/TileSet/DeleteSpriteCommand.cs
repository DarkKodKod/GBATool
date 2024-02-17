using ArchitectureLibrary.Commands;
using ArchitectureLibrary.Signals;
using GBATool.Signals;
using GBATool.VOs;
using System.Windows;
using System.Windows.Input;

namespace GBATool.Commands;

public class DeleteSpriteCommand : Command
{
    public override void Execute(object? parameter)
    {
        MessageBoxResult result = MessageBox.Show("Are you sure you want to delete the sprite?", "Delete", MessageBoxButton.YesNo);

        if (result == MessageBoxResult.Yes)
        {
            MouseButtonEventArgs? args = parameter as MouseButtonEventArgs;

            FrameworkElement? source = args?.OriginalSource as FrameworkElement;

            SpriteVO? sprite = source?.DataContext as SpriteVO;

            if (sprite != null)
            {
                SignalManager.Get<DeletingSpriteSignal>().Dispatch(sprite);
            }
        }
    }
}
