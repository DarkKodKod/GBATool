using Nett;
using System.Windows;

namespace GBATool.Models
{
    public class BankModel : AFileModel
    {
        public static readonly int MaxSpriteSize = 256;

        private const string _extensionKey = "extensionBanks";

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

        public bool Use256Colors { get; set; }
        public SpriteModel[] Sprites { get; set; } = new SpriteModel[MaxSpriteSize];

        public bool IsFull()
        {
            return false;
        }

        public void RegisterSprite(string tileSetID, SpriteModel sprite)
        {
            //
        }
    }
}
