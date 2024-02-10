using GBATool.Utils;
using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace GBATool.Views
{
    /// <summary>
    /// Interaction logic for ProjectPropertiesDialog.xaml
    /// </summary>
    public partial class ProjectPropertiesDialog : Window
    {
        public ProjectPropertiesDialog()
        {
            InitializeComponent();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            WindowUtility.RemoveIcon(this);
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex _regex = new("[^0-9.-]+"); //regex that matches disallowed text

            e.Handled = _regex.IsMatch(e.Text);
        }
    }
}
