using GBATool.Enums;
using GBATool.Models;
using Nett;
using System;
using System.IO;
using System.Threading.Tasks;

namespace GBATool.Utils
{
    public static class FileUtils
    {
        public static async ValueTask<byte[]> ReadTextAsync(string filePath)
        {
            byte[] result;

            using (FileStream sourceStream = File.Open(filePath, FileMode.Open))
            {
                result = new byte[sourceStream.Length];

                _ = await sourceStream.ReadAsync(result.AsMemory(0, (int)sourceStream.Length)).ConfigureAwait(false);
            }

            return result;
        }

        public static async ValueTask<AFileModel?> ReadFileAndLoadModelAsync(string filePath, ProjectItemType type)
        {
            byte[] content = await ReadTextAsync(filePath).ConfigureAwait(false);

            MemoryStream stream = new(content);

            AFileModel? model = null;

            try
            {
                switch (type)
                {
                    case ProjectItemType.Bank:
                        model = Toml.ReadStream<BankModel>(stream);
                        break;
                    case ProjectItemType.Character:
                        model = Toml.ReadStream<CharacterModel>(stream);
                        break;
                    case ProjectItemType.Map:
                        model = Toml.ReadStream<MapModel>(stream);
                        break;
                    case ProjectItemType.TileSet:
                        model = Toml.ReadStream<TileSetModel>(stream);
                        break;
                    case ProjectItemType.Palette:
                        model = Toml.ReadStream<PaletteModel>(stream);
                        break;
                    case ProjectItemType.World:
                        model = Toml.ReadStream<WorldModel>(stream);
                        break;
                    case ProjectItemType.Entity:
                        model = Toml.ReadStream<EntityModel>(stream);
                        break;
                }
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }

            return model;
        }
    }
}
