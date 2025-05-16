using ArchitectureLibrary.Signals;
using GBATool.Models;
using GBATool.Signals;
using GBATool.Utils;
using System.Windows.Media;

namespace GBATool.ViewModels;

public class PaletteViewModel : ItemViewModel
{
    public override void OnActivate()
    {
        #region Signals
        SignalManager.Get<ColorPaletteSelectedSignal>().Listener += OnColorPaletteSelected;
        #endregion

        PaletteModel? model = GetModel<PaletteModel>();

        if (model != null)
        {
            SignalManager.Get<PaletteColorArrayChangeSignal>().Dispatch(model.Colors);
        }
    }

    public override void OnDeactivate()
    {
        #region Signals
        SignalManager.Get<ColorPaletteSelectedSignal>().Listener -= OnColorPaletteSelected;
        #endregion
    }

    private void OnColorPaletteSelected(Color color, int colorIndex, int _)
    {
        PaletteModel? model = GetModel<PaletteModel>();

        if (model == null)
        {
            return;
        }

        int colorInt = PaletteUtils.ConvertColorToInt(color);

        int[] colorList = model.Colors;

        colorList[colorIndex] = colorInt;

        model.Colors = colorList;

        ProjectItem?.FileHandler?.Save();
    }
}
