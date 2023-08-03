using GBATool.Utils;
using System;
using System.Windows;

namespace GBATool.Views
{
    /// <summary>
    /// Interaction logic for ImportImageDialog.xaml
    /// </summary>
    public partial class ImportImageDialog : Window
    {
        public ImportImageDialog()
        {
            InitializeComponent();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            WindowUtility.RemoveIcon(this);
        }

        private void OnClickOK(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
