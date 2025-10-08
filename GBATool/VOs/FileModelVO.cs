using GBATool.Enums;
using GBATool.Models;
using GBATool.Utils;
using System.ComponentModel;
using System.Text;

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

                OnPropertyChanged(nameof(Name));
            }
        }
    }
    public ProjectItemType? Type { get; set; }
    public AFileModel? Model { get; set; }
    public string FullName
    {
        get
        {
            if (Type == null || Path == null)
            {
                return string.Empty;
            }

            string? folderName = Util.GetFolderByType(Type);

            if (folderName == null)
            {
                return string.Empty;
            }

            int found = Path.IndexOf(folderName);
            string subStr = Path[(found + folderName.Length)..];

            StringBuilder sb = new();
            if (!string.IsNullOrEmpty(subStr))
            {
                subStr = subStr[1..]; // Removing the first path separator

                sb.Append(subStr);
                sb.Append(System.IO.Path.DirectorySeparatorChar);
            }

            sb.Append(Name);

            return sb.ToString();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propname)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));
    }
}
