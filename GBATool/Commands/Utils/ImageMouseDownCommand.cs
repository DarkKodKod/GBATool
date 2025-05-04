using ArchitectureLibrary.Commands;
using ArchitectureLibrary.Signals;
using GBATool.Signals;
using GBATool.Utils;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GBATool.Commands;

public class ImageMouseDownCommand : Command
{
    public override void Execute(object? parameter)
    {
        MouseButtonEventArgs? mouseEvent = parameter as MouseButtonEventArgs;

        if (mouseEvent?.Source == null)
        {
            return;
        }

        Canvas? canvas = Util.FindAncestor<Canvas>((DependencyObject)mouseEvent.Source);

        if (canvas == null)
        {
            return;
        }

        if (canvas.ActualWidth == 0 || canvas.ActualHeight == 0)
        {
            return;
        }

        Image? image = Util.FindChild<Image>(canvas);

        if (image == null)
        {
            return;
        }

        Point p = mouseEvent.GetPosition(canvas);

        SignalManager.Get<MouseImageSelectedSignal>().Dispatch(image, p);
    }
}
