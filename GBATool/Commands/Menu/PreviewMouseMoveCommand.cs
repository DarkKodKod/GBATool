using ArchitectureLibrary.Commands;
using ArchitectureLibrary.Signals;
using GBATool.Signals;
using GBATool.Utils;
using GBATool.VOs;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GBATool.Commands
{
    public class PreviewMouseMoveCommand : Command
    {
        public override void Execute(object parameter)
        {
            MouseEventArgs? mouseEvent = parameter as MouseEventArgs;

            if (mouseEvent?.LeftButton == MouseButtonState.Pressed)
            {
                TreeViewItem? treeViewItem = Util.FindAncestor<TreeViewItem>((DependencyObject)mouseEvent.OriginalSource);

                MouseMoveVO vo = new()
                {
                    Position = mouseEvent.GetPosition(treeViewItem),
                    OriginalSource = mouseEvent.OriginalSource,
                    Sender = mouseEvent.Source
                };

                SignalManager.Get<MouseMoveSignal>().Dispatch(vo);
            }
        }
    }
}
