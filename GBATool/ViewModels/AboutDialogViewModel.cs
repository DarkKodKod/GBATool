using ArchitectureLibrary.ViewModel;
using GBATool.Commands;
using System;
using System.Reflection;

namespace GBATool.ViewModels
{
    public class AboutDialogViewModel : ViewModel
    {
        public OpenLinkCommand OpenLinkCommand { get; } = new();

        public string Version
        {
            get => _version;
            set
            {
                _version = value;
                OnPropertyChanged("Version");
            }
        }

        private string _version = "";

        public AboutDialogViewModel()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            Version? version = assembly.GetName().Version;

            if (version != null)
            {
                Version = version.ToString();
            }
        }
    }
}
