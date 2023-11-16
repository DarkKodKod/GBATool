using ArchitectureLibrary.Commands;
using System.Windows.Forms;
using System.Windows.Media;

namespace GBATool.Commands
{
    public class ShowColorPaletteCommand : Command
    {
        public override void Execute(object? parameter)
        {
            if (parameter == null)
            {
                return;
            }

            object[] values = (object[])parameter;

            ColorDialog colorDialog = new()
            {
                AnyColor = true,
                SolidColorOnly = true,
                Color = System.Drawing.Color.Red
            };

            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                Color color = Color.FromArgb(colorDialog.Color.A, colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B);

                var output = new SolidColorBrush(color);
            }
        }
    }
}
