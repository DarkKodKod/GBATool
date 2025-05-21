using ArchitectureLibrary.Signals;
using GBATool.Signals;
using GBATool.Utils;
using System.Windows.Controls;
using System.Windows.Media;

namespace GBATool.Views
{
    /// <summary>
    /// Interaction logic for Character.xaml
    /// </summary>
    public partial class Character : UserControl, ICleanable
    {
        public Character()
        {
            InitializeComponent();

            #region Signals
            SignalManager.Get<CleanColorPaletteControlSelectedSignal>().Listener += OnCleanColorPaletteControlSelected;
            SignalManager.Get<ColorPaletteControlSelectedSignal>().Listener += OnColorPaletteControlSelected;
            #endregion
        }

        private void OnCleanColorPaletteControlSelected()
        {
            ColorPaletteCleanup();
        }

        private void OnColorPaletteControlSelected(int[] colors)
        {
            SolidColorBrush[] tempList = new SolidColorBrush[16];

            for (int i = 0; i < 16; i++)
            {
                tempList[i] = new SolidColorBrush(Util.GetColorFromInt(colors[i]));
            }

            palette.SolidColorBrushList = tempList;
        }

        private void ColorPaletteCleanup()
        {
            SolidColorBrush[] tempList = new SolidColorBrush[16];
            SolidColorBrush brush = new(Util.NullColor);

            for (int i = 0; i < 16; i++)
            {
                tempList[i] = brush;
            }

            palette.SolidColorBrushList = tempList;
        }

        public void CleanUp()
        {
            ColorPaletteCleanup();

            #region Signals
            SignalManager.Get<CleanColorPaletteControlSelectedSignal>().Listener -= OnCleanColorPaletteControlSelected;
            SignalManager.Get<ColorPaletteControlSelectedSignal>().Listener -= OnColorPaletteControlSelected;
            #endregion
        }
    }
}
