using ArchitectureLibrary.Commands;
using ArchitectureLibrary.Signals;
using GBATool.Signals;
using GBATool.Utils;
using GBATool.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace GBATool.Commands
{
    public class DragEnterCommand : Command
    {
        public override bool CanExecute(object? parameter)
        {
            if (parameter is not DragEventArgs dragEvent)
            {
                return false;
            }

            ProjectItem? draggingItem = dragEvent?.Data.GetData(typeof(ProjectItem)) as ProjectItem;

            DependencyObject? dependency = (DependencyObject?)dragEvent?.OriginalSource;

            TreeViewItem? treeViewItem = Util.FindAncestor<TreeViewItem>(dependency);

            if (treeViewItem == null)
            {
                return false;
            }

            if (treeViewItem.DataContext is ProjectItem item && item.Type != draggingItem?.Type)
            {
                if (dragEvent != null)
                {
                    dragEvent.Handled = true;
                    dragEvent.Effects = DragDropEffects.None;
                }

                SignalManager.Get<DetachAdornersSignal>().Dispatch();

                return false;
            }

            return true;
        }

        public override void Execute(object? parameter)
        {
            if (parameter == null)
            {
                return;
            }

            if (parameter is not DragEventArgs dragEvent)
            {
                return;
            }

            TreeViewItem? treeViewItem = Util.FindAncestor<TreeViewItem>((DependencyObject)dragEvent.OriginalSource);

            if (treeViewItem != null)
            {
                dragEvent.Effects = DragDropEffects.Move;

                SignalManager.Get<InitializeAdornersSignal>().Dispatch(treeViewItem, dragEvent);
            }

            dragEvent.Handled = true;
        }
    }
}
