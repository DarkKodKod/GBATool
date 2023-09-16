using Nett;
using System.Windows;

namespace GBATool.Models
{
    public class WorldModel : AFileModel
    {
        private const string _extensionKey = "extensionWorlds";

        [TomlIgnore]
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

        public override void CopyModel(AFileModel? model)
        {
            //
        }
    }
}
