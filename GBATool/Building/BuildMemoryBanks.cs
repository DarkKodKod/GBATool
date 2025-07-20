﻿using ArchitectureLibrary.Model;
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
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GBATool.Building;

using TileBlocks = (int width, int height, int numberOfTiles);

public sealed class BuildMemoryBanks : Building<BuildMemoryBanks>
{
    protected override string FileName { get; } = string.Empty;

    private readonly List<string> _bankNames = [];

    protected override async Task<bool> DoGenerate()
    {
        ProjectModel projectModel = ModelManager.Get<ProjectModel>();

        string outputPath = Path.GetFullPath(projectModel.Build.GeneratedAssetsPath);

        List<FileModelVO> bankModelVOs = ProjectFiles.GetModels<BankModel>();

        _bankNames.Clear();

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

            List<Color> palette = ImageProcessing.GetNewPalette(bpp, bank.TransparentColor);

            using (metaData.image.GetBitmapContext())
            {
                try
                {
                    List<string> warnings = [];

                    imageData = ImageProcessing.ConvertToXbpp(bpp, metaData.image, cellsCount, ref palette, warnings);

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
                _bankNames.Add(vo.Name);

                string fileName = Path.Combine(outputPath, vo.Name.ToLower());

                await File.WriteAllBytesAsync(fileName + ".bin", imageData).ConfigureAwait(false);
            }

            processedCount++;
        }

        await WriteBlocksMetaData();

        if (processedCount == 0)
        {
            AddError("No banks processed");
        }

        return GetErrors().Length == 0;
    }

    private async Task WriteBlocksMetaData()
    {
        if (_bankNames.Count == 0)
        {
            return;
        }

        ProjectModel projectModel = ModelManager.Get<ProjectModel>();

        string fullPath = Path.Combine(Path.GetFullPath(projectModel.Build.GeneratedAssetsPath), "blocks_metadata.asm");

        using StreamWriter outputFile = new(fullPath);

        await outputFile.WriteLineAsync("    align 4");

        foreach (string bankName in _bankNames)
        {
            await outputFile.WriteLineAsync($"Size_block_{bankName.ToLower()}:");
            await outputFile.WriteLineAsync($"    dw (__block_{bankName.ToLower()} - block_{bankName.ToLower()})");
        }

        await outputFile.WriteAsync(Environment.NewLine);

        await outputFile.WriteLineAsync("    align 4");

        foreach (string bankName in _bankNames)
        {
            await outputFile.WriteLineAsync($"Block_{bankName.ToLower()}:");
            await outputFile.WriteLineAsync($"    dw block_{bankName.ToLower()}");
        }

        await outputFile.WriteAsync(Environment.NewLine);

        await outputFile.WriteLineAsync("    align 32");

        foreach (string bankName in _bankNames)
        {
            await outputFile.WriteLineAsync($"block_{bankName.ToLower()}:");

            await outputFile.WriteLineAsync($"    file \"{bankName.ToLower() + ".bin"}\"");

            await outputFile.WriteLineAsync($"__block_{bankName.ToLower()}:");
        }
    }
}
