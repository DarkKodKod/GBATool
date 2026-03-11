using System.Windows.Controls;
using System.Windows;

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
