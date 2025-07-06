using ArchitectureLibrary.Signals;
using GBATool.Enums;
using GBATool.Signals;
using GBATool.Utils;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace GBATool.Views
{
    /// <summary>
    /// Interaction logic for BuildProjectDialog.xaml
    /// </summary>
    public partial class BuildProjectDialog : Window, ICleanable
    {
        public BuildProjectDialog()
        {
            InitializeComponent();

            #region Signals
            SignalManager.Get<WriteBuildOutputSignal>().Listener += OnWriteBuildOutput;
            #endregion
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            WindowUtility.RemoveIcon(this);
        }

        private void OnWriteBuildOutput(string newLine, OutputMessageType messageType, string color = "")
        {
            BrushConverter bc = new();

            TextRange tr = new(tbOutput.Document.ContentEnd, tbOutput.Document.ContentEnd)
            {
                Text = newLine + Environment.NewLine
            };

            if (string.IsNullOrEmpty(color))
            {
                switch (messageType)
                {
                    case OutputMessageType.Information: color = "Black"; break;
                    case OutputMessageType.Warning: color = "Yellow"; break;
                    case OutputMessageType.Error: color = "Red"; break;
                }
            }

            tr.ApplyPropertyValue(TextElement.ForegroundProperty, bc.ConvertFromString(color));

            tbOutput.ScrollToEnd();
        }

        public void CleanUp()
        {
            #region Signals
            SignalManager.Get<WriteBuildOutputSignal>().Listener -= OnWriteBuildOutput;
            #endregion
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            CleanUp();
        }
    }
}
