using GBATool.Enums;
using GBATool.Models;

namespace GBATool.Utils
{
    public static class Util
    {
        public static AFileModel? FileModelFactory(ProjectItemType type)
        {
            return type switch
            {
                ProjectItemType.Bank => new BankModel(),
                ProjectItemType.Character => new CharacterModel(),
                ProjectItemType.Map => new MapModel(),
                ProjectItemType.TileSet => new TileSetModel(),
                ProjectItemType.Palette => new PaletteModel(),
                ProjectItemType.World => new WorldModel(),
                ProjectItemType.Entity => new EntityModel(),
                _ => null,
            };
        }
    }
}
