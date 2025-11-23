using ArchitectureLibrary.Model;
using GBATool.Enums;
using GBATool.FileSystem;
using GBATool.Models;
using GBATool.Utils;
using GBATool.VOs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GBATool.Building;

public sealed class BuildPalettes : Building<BuildPalettes>
{
    protected override string FileName { get; } = "palettes.asm";
    protected override OutputFormat OutputFormat { get; } = OutputFormat.Fasmarm;

    protected override async Task<bool> DoGenerate(string filePath)
    {
        ProjectModel projectModel = ModelManager.Get<ProjectModel>();

        using StreamWriter outputFile = new(filePath);

        List<FileModelVO> paletteModelVOs = ProjectFiles.GetModels<PaletteModel>();

        outputFile.WriteLine("; This file is auto generated!");
        outputFile.Write(Environment.NewLine);
        outputFile.WriteLine("; Color format: RGB555 0BBBBBGGGGGRRRRR");

        SortedDictionary<string, int[]> pals = [];

        int processedCount = 0;

        string folderPalettesKey = "folderPalettes";
        string folderPalettes = (string)Application.Current.FindResource(folderPalettesKey);

        foreach (FileModelVO vo in paletteModelVOs)
        {
            if (vo.Model is not PaletteModel model)
            {
                continue;
            }

            if (model == null)
            {
                continue;
            }

            if (string.IsNullOrEmpty(vo.Name))
            {
                continue;
            }

            if (string.IsNullOrEmpty(vo.Path))
            {
                continue;
            }

            string[] array = vo.Path.Split(Path.DirectorySeparatorChar);
            int index = Array.IndexOf(array, folderPalettes);

            StringBuilder sb = new();
            for (int i = index + 1; i < array.Length; i++)
            {
                sb.Append(array[i]);
                sb.Append('_');
            }

            string name = $"palette_{sb}{vo.Name.Replace(' ', '_').ToLower()}";

            pals.Add(name, model.Colors);

            processedCount++;
        }

        if (processedCount == 0)
        {
            AddError("No palettes processed");
        }
        else
        {
            await Task.Run(() => WritePalettesToOutputFile(ref pals, outputFile)).ConfigureAwait(false);
        }

        return true;
    }

    private static void WritePalettesToOutputFile(ref SortedDictionary<string, int[]> pals, StreamWriter outputFile)
    {
        outputFile.Write(Environment.NewLine);
        _ = outputFile.WriteLineAsync("    align 32");

        foreach (var palette in pals)
        {
            int cont = 0;

            outputFile.WriteLine($"{palette.Key}:");
            outputFile.Write($"    db ");

            foreach (int item in palette.Value)
            {
                System.Windows.Media.Color color = PaletteUtils.GetColorFromInt(item);

                ushort rgb555 = (ushort)(((color.B & 0xF8) << 7) | ((color.G & 0xF8) << 2) | (color.R >> 3));

                byte[] bytes = BitConverter.GetBytes(rgb555);

                outputFile.Write($"0x{bytes[0]:X2},0x{bytes[1]:X2}");

                cont++;

                if (cont % 16 > 0)
                    outputFile.Write(",");
                else
                    outputFile.Write(Environment.NewLine);
            }
        }

        outputFile.Write(Environment.NewLine);
    }
}
