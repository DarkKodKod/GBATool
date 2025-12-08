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

        string collisionID = (string)values[0];
        SolidColorBrush oldColor = (SolidColorBrush)values[1];

        ColorDialog colorDialog = new()
        {
            AnyColor = true,
            SolidColorOnly = true,
            AllowFullOpen = true,
            FullOpen = true,
            Color = System.Drawing.Color.FromArgb(oldColor.Color.R, oldColor.Color.G, oldColor.Color.B)
        };

        if (colorDialog.ShowDialog() == DialogResult.OK)
        {
            Color colorBrush = Color.FromArgb(colorDialog.Color.A, colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B);

            if (oldColor.Color == colorBrush)
            {
                return;
            }

            SignalManager.Get<CollisionColorSelectedSignal>().Dispatch(collisionID, colorBrush);
        }
    }
}
