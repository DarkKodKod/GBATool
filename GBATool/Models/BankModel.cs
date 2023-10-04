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

        [TomlIgnore]
        public bool IsFull { get; private set; }

        public bool RegisterSprite(SpriteModel sprite)
        {
            for (int i = 0; i < Sprites.Length; ++i)
            {
                if (string.IsNullOrEmpty(Sprites[i].ID))
                {
                    Sprites[i].ID = sprite.ID;
                    Sprites[i].Shape = sprite.Shape;
                    Sprites[i].Size = sprite.Size;
                    Sprites[i].PosX = sprite.PosX;
                    Sprites[i].PosY = sprite.PosY;
                    Sprites[i].TileSetID = sprite.TileSetID;

                    return true;
                }
            }

            IsFull = true;

            return false;
        }
    }
}
