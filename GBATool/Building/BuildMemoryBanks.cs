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
            {
                continue;
            }

            bool ret = GetPaletteIfExistInCharacters(bank, out List<Color> palette);

            if (!ret)
            {
                AddError($"No palette configure for bank {vo.Name}");
                continue;
            }

            byte[]? imageData = null;

            using (metaData.image.GetBitmapContext())
            {
                try
                {
                    List<string> warnings = [];

                    imageData = ImageProcessing.ConvertToXbpp(bpp, in metaData.image, in cellsCount, in palette, ref warnings);

                    foreach (string item in warnings)
                    {
                        AddWarning($"{vo.Name}, " + item);
                    }
                }
                catch (Exception e)
                {
                    AddError(e.Message);
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

    private static bool GetPaletteIfExistInCharacters(BankModel bank, out List<Color> palette)
    {
        palette = [];

        List<FileModelVO> models = ProjectFiles.GetModels<CharacterModel>();

        foreach (FileModelVO fileModel in models)
        {
            if (fileModel.Model is not CharacterModel character)
            {
                continue;
            }

            if (string.IsNullOrEmpty(character.PaletteID))
            {
                continue;
            }

            foreach (KeyValuePair<string, CharacterAnimation> animation in character.Animations)
            {
                foreach (KeyValuePair<string, FrameModel> frame in animation.Value.Frames)
                {
                    if (frame.Value.BankID == bank.GUID)
                    {
                        PaletteModel? paletteModel = ProjectFiles.GetModel<PaletteModel>(character.PaletteID);

                        if (paletteModel != null)
                        {
                            for (int i = 0; i < paletteModel.Colors.Length; i++)
                            {
                                palette.Add(PaletteUtils.GetColorFromInt(paletteModel.Colors[i]));
                            }

                            return true;
                        }
                    }
                }
            }
        }

        return false;
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

        await outputFile.WriteLineAsync("    align 4");

        foreach (string bankName in _bankNames)
        {
            await outputFile.WriteLineAsync($"block_{bankName.ToLower()}:");

            await outputFile.WriteLineAsync($"    file \"{bankName.ToLower() + ".bin"}\"");

            await outputFile.WriteLineAsync($"__block_{bankName.ToLower()}:");
        }
    }
}
