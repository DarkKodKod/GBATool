using GBATool.Utils;
using System.Windows.Controls;

namespace GBATool.Views
{
    /// <summary>
    /// Interaction logic for Palette.xaml
    /// </summary>
    public partial class Palette : UserControl, ICleanable
    {
        public Palette()
        {
            InitializeComponent();

            palette.OnActivate();
        }

        public void CleanUp()
        {
            palette.OnDeactivate();
        }
    }
}
