using ArchitectureLibrary.Commands;
using ArchitectureLibrary.Signals;
using GBATool.Signals;
using GBATool.Utils;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GBATool.Commands
{
    public class PreviewMouseLeftButtonDownCommand : Command
    {
        public override void Execute(object parameter)
        {
            MouseButtonEventArgs? mouseEvent = parameter as MouseButtonEventArgs;

            if (mouseEvent != null)
            {
                TreeViewItem? treeViewItem = Util.FindAncestor<TreeViewItem>((DependencyObject)mouseEvent.OriginalSource);

                Point position = mouseEvent.GetPosition(treeViewItem);

                SignalManager.Get<MouseLeftButtonDownSignal>().Dispatch(position);
            }
        }
    }
}
