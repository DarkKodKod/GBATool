using GBATool.Models;
using System.ComponentModel;

namespace GBATool.VOs;

public class FileModelVO : INotifyPropertyChanged
{
    private string? _name;

    public int Index { get; set; }
    public string? Path { get; set; }
    public string? Name
    {
        get { return _name; }
        set
        {
            if (_name != value)
            {
                _name = value;

                OnPropertyChanged("Name");
            }
        }
    }
    public AFileModel? Model { get; set; }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propname)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));
    }
}
