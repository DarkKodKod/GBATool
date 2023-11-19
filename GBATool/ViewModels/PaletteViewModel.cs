using ArchitectureLibrary.Signals;
using GBATool.Models;
using GBATool.Signals;
using System.Windows.Media;

namespace GBATool.ViewModels
{
    public class PaletteViewModel : ItemViewModel
    {
        public override void OnActivate()
        {
            SignalManager.Get<ColorPaletteSelectedSignal>().Listener += OnColorPaletteSelected;

            PaletteModel? model = GetModel<PaletteModel>();

            if (model == null)
            {
                return;
            }

            // load model.Colors into the paletteviewer
        }

        public override void OnDeactivate()
        {
            SignalManager.Get<ColorPaletteSelectedSignal>().Listener -= OnColorPaletteSelected;
        }

        private void OnColorPaletteSelected(Color color, int colorIndex)
        {
            PaletteModel? model = GetModel<PaletteModel>();

            if (model == null)
            {
                return;
            }

            int colorInt = ((color.R & 0xff) << 16) | ((color.G & 0xff) << 8) | (color.B & 0xff);

            int[] colorList = model.Colors;

            colorList[colorIndex] = colorInt;

            model.Colors = colorList;

            ProjectItem?.FileHandler?.Save();
        }
    }
}
