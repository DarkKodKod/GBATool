using System.Text.Json.Serialization;
using System.Windows;

namespace GBATool.Models;

public class WorldModel : AFileModel
{
    private const string _extensionKey = "extensionWorlds";

    [JsonIgnore]
    public override string FileExtension
    {
        get
        {
            if (string.IsNullOrEmpty(_fileExtension))
            {
                _fileExtension = (string)Application.Current.FindResource(_extensionKey);
            }

            return _fileExtension;
        }
    }
}
