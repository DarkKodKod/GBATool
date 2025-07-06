using ArchitectureLibrary.Model;
using GBATool.Enums;
using GBATool.FileSystem;
using GBATool.Models;
using GBATool.Utils;
using GBATool.Utils.Image;
using GBATool.VOs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace GBATool.Building;

using TileBlocks = (int width, int height);

public sealed class BuildMemoryBanks : Building<BuildMemoryBanks>
{
    protected override string FileName { get; } = string.Empty;

    protected override async Task<bool> DoGenerate()
    {
        ProjectModel projectModel = ModelManager.Get<ProjectModel>();

        string outputPath = Path.GetFullPath(projectModel.Build.GeneratedAssetsPath);

        List<FileModelVO> bankModelVOs = ProjectFiles.GetModels<BankModel>();

        int processedCount = 0;

        foreach (FileModelVO vo in bankModelVOs)
        {
            if (vo.Model is not BankModel bank)
                continue;

            BitsPerPixel bpp = bank.Use256Colors ? BitsPerPixel.f8bpp : BitsPerPixel.f4bpp;

            TileBlocks cellsCount = bank.GetBoundingBoxSize();

            int imageWidth = cellsCount.width * 8;
            int imageHeight = cellsCount.height * 8;

            BankImageMetaData metaData = BankUtils.CreateImage(bank, false, imageWidth, imageHeight);

            if (metaData.image == null)
                continue;

            byte[]? imageData = null;

            List<System.Windows.Media.Color> palette = ImageProcessing.GetNewPalette(bpp, bank.TransparentColor);

            using (metaData.image.GetBitmapContext())
            {
                try
                {
                    List<string> warnings = [];

                    imageData = ImageProcessing.ConvertToXbpp(bpp, metaData.image, cellsCount.width, cellsCount.height, ref palette, warnings);

                    foreach (string item in warnings)
                    {
                        AddWarning($"{vo.Name}, " + item);
                    }
                }
                catch (Exception e)
                {
                    AddError(e.ToString());
                    continue;
                }

                if (imageData == null)
                {
                    AddError("Could not convert the image from the bank to the bit per pixel requested");
                    continue;
                }
            }

            if (!string.IsNullOrEmpty(vo.Name))
            {
                string fileName = Path.Combine(outputPath, vo.Name.ToLower());

                await File.WriteAllBytesAsync(fileName + ".bin", imageData).ConfigureAwait(false);
            }

            processedCount++;
        }

        if (processedCount == 0)
        {
            AddError("No banks processed");
        }

        return GetErrors().Length == 0;
    }
}
