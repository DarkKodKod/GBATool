using ArchitectureLibrary.Commands;
using ArchitectureLibrary.Signals;
using GBATool.Signals;
using System.Windows.Forms;
using System.Windows.Media;

namespace GBATool.Commands.Character;

public class ChangeCollisionColorCommand : Command
{
    public override void Execute(object? parameter)
    {
        if (parameter == null)
        {
            return;
        }

        object[] values = (object[])parameter;

        if (values[0] is not SolidColorBrush color)
        {
            return;
        }

        ColorDialog colorDialog = new()
        {
            AnyColor = true,
            SolidColorOnly = true,
            AllowFullOpen = true,
            FullOpen = true,
            Color = System.Drawing.Color.FromArgb(color.Color.R, color.Color.G, color.Color.B)
        };

        if (colorDialog.ShowDialog() == DialogResult.OK)
        {
            Color colorBrush = Color.FromArgb(colorDialog.Color.A, colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B);

            SignalManager.Get<CollisionColorSelectedSignal>().Dispatch(colorBrush);
        }
    }
}
