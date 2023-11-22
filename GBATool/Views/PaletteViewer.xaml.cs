using System.Windows;
using System.Windows.Controls;

namespace GBATool.Views
{
    /// <summary>
    /// Interaction logic for PaletteViewer.xaml
    /// </summary>
    public partial class PaletteViewer : UserControl
    {
        public static readonly DependencyProperty PaletteIndexProperty =
            DependencyProperty.Register("PaletteIndex",
                typeof(int),
                typeof(PaletteViewer),
                new PropertyMetadata(0));

        public int PaletteIndex
        {
            get => (int)GetValue(PaletteIndexProperty);
            set => SetValue(PaletteIndexProperty, value);
        }

        public PaletteViewer()
        {
            InitializeComponent();
        }
    }
}
