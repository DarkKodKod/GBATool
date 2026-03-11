using System.Windows;
using System.Windows.Controls;

namespace GBATool.Views
{
    /// <summary>
    /// Interaction logic for Map.xaml
    /// </summary>
    public partial class Map : UserControl
    {
        public Map()
        {
            InitializeComponent();
        }

        private void MapView_Loaded(object sender, RoutedEventArgs e)
        {
            bankViewer.OnActivate();
        }

        private void MapView_Unloaded(object sender, RoutedEventArgs e)
        {
            bankViewer.OnDeactivate();
        }
    }
}
