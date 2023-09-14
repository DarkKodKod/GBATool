using ArchitectureLibrary.Commands;
using ArchitectureLibrary.Signals;
using GBATool.Signals;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GBATool.Commands
{
    public class ImageMouseDownCommand : Command
    {
        public override void Execute(object parameter)
        {
            MouseButtonEventArgs? mouseEvent = parameter as MouseButtonEventArgs;

            if (mouseEvent?.Source is Image image)
            {
                if (image.ActualWidth == 0 || image.ActualHeight == 0)
                {
                    return;
                }

                Point p = mouseEvent.GetPosition(image);

                SignalManager.Get<MouseImageSelectedSignal>().Dispatch(image, p);
            }
        }
    }
}
