using GBATool.Enums;
using GBATool.Models;
using Nett;
using System.IO;
using System.Threading.Tasks;
using System;

namespace GBATool.Utils
{
    public static class FileUtils
    {
        public static async Task<byte[]> ReadTextAsync(string filePath)
        {
            byte[] result;

            using (FileStream sourceStream = File.Open(filePath, FileMode.Open))
            {
                result = new byte[sourceStream.Length];

                _ = await sourceStream.ReadAsync(result.AsMemory(0, (int)sourceStream.Length)).ConfigureAwait(false);
            }

            return result;
        }

        public static async Task<AFileModel?> ReadFileAndLoadModelAsync(string filePath, ProjectItemType type)
        {
            byte[] content = await ReadTextAsync(filePath).ConfigureAwait(false);

            AFileModel? model = null;

            switch (type)
            {
                case ProjectItemType.Bank:
                    model = Toml.ReadStream<BankModel>(new MemoryStream(content));
                    break;
                case ProjectItemType.Character:
                    model = Toml.ReadStream<CharacterModel>(new MemoryStream(content));
                    break;
                case ProjectItemType.Map:
                    model = Toml.ReadStream<MapModel>(new MemoryStream(content));
                    break;
                case ProjectItemType.TileSet:
                    model = Toml.ReadStream<TileSetModel>(new MemoryStream(content));
                    break;
                case ProjectItemType.Palette:
                    model = Toml.ReadStream<PaletteModel>(new MemoryStream(content));
                    break;
                case ProjectItemType.World:
                    model = Toml.ReadStream<WorldModel>(new MemoryStream(content));
                    break;
                case ProjectItemType.Entity:
                    model = Toml.ReadStream<EntityModel>(new MemoryStream(content));
                    break;
            }

            return model;
        }
    }
}
