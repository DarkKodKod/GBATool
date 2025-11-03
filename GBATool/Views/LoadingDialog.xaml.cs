using System.ComponentModel;
using System.Windows;

namespace GBATool.Views
{
    /// <summary>
    /// Interaction logic for LoadingDialog.xaml
    /// </summary>
    public partial class LoadingDialog : Window
    {
        public LoadingDialog()
        {
            InitializeComponent();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Visibility = Visibility.Collapsed;
            e.Cancel = true;
        }
    }
}
