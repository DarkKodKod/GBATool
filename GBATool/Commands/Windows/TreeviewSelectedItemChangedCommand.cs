using ArchitectureLibrary.Commands;
using ArchitectureLibrary.Signals;
using GBATool.Signals;
using GBATool.ViewModels;
using System.Windows;

namespace GBATool.Commands
{
    public class TreeviewSelectedItemChangedCommand : Command
    {
        public override void Execute(object parameter)
        {
            RoutedPropertyChangedEventArgs<object>? selectedItemChangedEvent = parameter as RoutedPropertyChangedEventArgs<object>;

            if (selectedItemChangedEvent?.NewValue is ProjectItem item)
            {
                SignalManager.Get<LoadProjectItemSignal>().Dispatch(item);
            }
        }
    }
}
