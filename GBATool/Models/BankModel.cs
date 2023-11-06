using GBATool.FileSystem;
using Nett;
using System.Collections.Generic;
using System.Windows;

namespace GBATool.Models
{
    public record SpriteRef
    {
        public string? SpriteID { get; set; }
        public string? TileSetID { get; set; }
    }

    public class BankModel : AFileModel
    {
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
        public bool IsBackground { get; set; }
        public List<SpriteRef> Sprites { get; set; } = new();

        [TomlIgnore]
        public bool IsFull { get; private set; }

        public (bool, string) RegisterSprite(SpriteModel sprite)
        {
            SpriteRef? find = Sprites.Find((spriteRef) => spriteRef.SpriteID == sprite.ID);

            if (!string.IsNullOrEmpty(find?.SpriteID))
            {
                return (false, "This sprite is already in the bank");
            }

            Sprites.Add(new SpriteRef() { SpriteID = sprite.ID, TileSetID = sprite.TileSetID });

            return (true, "");
        }

        public bool RemoveSprite(SpriteModel sprite)
        {
            SpriteRef? find = Sprites.Find((spriteRef) => spriteRef.SpriteID == sprite.ID);

            if (string.IsNullOrEmpty(find?.SpriteID))
            {
                return false;
            }

            return Sprites.Remove(new SpriteRef() { SpriteID = sprite.ID, TileSetID = sprite.TileSetID });
        }

        public void ReplaceSpriteList(List<SpriteRef> list)
        {
            Sprites = list;
        }

        public void CleanUpDeletedSprites()
        {
            List<SpriteRef> idsToRemove = new();

            foreach (SpriteRef spriteRef in Sprites)
            {
                if (spriteRef == null ||
                    string.IsNullOrEmpty(spriteRef.SpriteID) ||
                    string.IsNullOrEmpty(spriteRef.TileSetID))
                    continue;

                TileSetModel? tileSetModel = ProjectFiles.GetModel<TileSetModel>(spriteRef.TileSetID);

                if (tileSetModel == null)
                {
                    idsToRemove.Add(spriteRef);
                    continue;
                }

                SpriteModel? sprite = tileSetModel.Sprites.Find((item) => item.ID == spriteRef.SpriteID);

                if (string.IsNullOrEmpty(sprite?.ID))
                {
                    idsToRemove.Add(spriteRef);
                }
            }

            foreach (SpriteRef spriteToRemove in idsToRemove)
            {
                Sprites.Remove(spriteToRemove);
            }
        }
    }
}
