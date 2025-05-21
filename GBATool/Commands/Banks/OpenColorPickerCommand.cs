using ArchitectureLibrary.Commands;
using ArchitectureLibrary.Signals;
using GBATool.Signals;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media;

namespace GBATool.Commands.Banks;

public class OpenColorPickerCommand : Command
{
    public override void Execute(object? parameter)
    {
        if (parameter == null)
        {
            return;
        }

        object[] values = (object[])parameter;

        System.Windows.Controls.Control ownerControl = (System.Windows.Controls.Control)values[0];
        Color inputColor = (Color)values[1];

        ColorDialog dialog = new()
        {
            Color = System.Drawing.Color.FromArgb(inputColor.R, inputColor.G, inputColor.B),
            AllowFullOpen = true,
            FullOpen = true,
            SolidColorOnly = true,
            AnyColor = true
        };

        NativeWindow win32Parent = new();
        win32Parent.AssignHandle(new WindowInteropHelper(System.Windows.Application.Current.MainWindow).Handle);

        if (dialog.ShowDialog(win32Parent) == DialogResult.OK)
        {
            Color color = Color.FromRgb(dialog.Color.R, dialog.Color.G, dialog.Color.B);

            SignalManager.Get<SetColorFromColorPickerSignal>().Dispatch(ownerControl, color);
        }

        win32Parent.ReleaseHandle();
    }
}
