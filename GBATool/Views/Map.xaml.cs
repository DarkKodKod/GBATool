using GBATool.Utils;
using System.Windows;
using System.Windows.Controls;

namespace GBATool.Views
{
    /// <summary>
    /// Interaction logic for Map.xaml
    /// </summary>
    public partial class Map : UserControl, ICleanable
    {
        public Map()
        {
            InitializeComponent();

            bankViewer.OnActivate();

            palette0.OnActivate();
            palette1.OnActivate();
            palette2.OnActivate();
            palette3.OnActivate();
            palette4.OnActivate();
            palette5.OnActivate();
            palette6.OnActivate();
            palette7.OnActivate();
            palette8.OnActivate();
            palette9.OnActivate();
            palette10.OnActivate();
            palette11.OnActivate();
            palette12.OnActivate();
            palette13.OnActivate();
            palette14.OnActivate();
            palette15.OnActivate();
        }

        public void CleanUp()
        {
            bankViewer.OnDeactivate();

            palette0.OnDeactivate();
            palette1.OnDeactivate();
            palette2.OnDeactivate();
            palette3.OnDeactivate();
            palette4.OnDeactivate();
            palette5.OnDeactivate();
            palette6.OnDeactivate();
            palette7.OnDeactivate();
            palette8.OnDeactivate();
            palette9.OnDeactivate();
            palette10.OnDeactivate();
            palette11.OnDeactivate();
            palette12.OnDeactivate();
            palette13.OnDeactivate();
            palette14.OnDeactivate();
            palette15.OnDeactivate();
        }
    }
}
