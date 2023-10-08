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

        public (bool, string) RegisterSprite(SpriteModel sprite)
        {
            int selectedIndex = -1;

            // first check if the sprite already exists
            for (int i = 0; i < Sprites.Length; ++i)
            {
                if (string.IsNullOrEmpty(Sprites[i].ID) && selectedIndex == -1)
                {
                    selectedIndex = i;
                }

                if (Sprites[i] == sprite)
                {
                    return (false, "This sprite is already in the bank");
                }
            }

            if (selectedIndex == -1)
            {
                IsFull = true;
                return (false, "This bank has all its cell ocupied");
            }
            else
            {
                Sprites[selectedIndex].ID = sprite.ID;
                Sprites[selectedIndex].Shape = sprite.Shape;
                Sprites[selectedIndex].Size = sprite.Size;
                Sprites[selectedIndex].PosX = sprite.PosX;
                Sprites[selectedIndex].PosY = sprite.PosY;
                Sprites[selectedIndex].TileSetID = sprite.TileSetID;
            }

            return (true, "");
        }
    }
}
