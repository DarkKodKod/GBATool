using ArchitectureLibrary.Model;
using GBATool.FileSystem;
using GBATool.Models;
using GBATool.VOs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GBATool.Building;

public sealed class BuildMetaSprites : Building<BuildMetaSprites>
{
    protected override string FileName { get; } = string.Empty;

    private const float FrameRate = 59.727500569606f;

    protected override async Task<bool> DoGenerate()
    {
        ProjectModel projectModel = ModelManager.Get<ProjectModel>();

        List<FileModelVO> models = ProjectFiles.GetModels<CharacterModel>();

        foreach (FileModelVO item in models)
        {
            if (item.Model is not CharacterModel model)
            {
                continue;
            }

            if (string.IsNullOrEmpty(item.Name))
            {
                continue;
            }

            string fullPath = Path.Combine(Path.GetFullPath(projectModel.Build.GeneratedAssetsPath), item.Name + ".asm");

            using StreamWriter outputFile = new(fullPath);

            await WriteMetaSprites(outputFile, model, item.Name);
        }

        return GetErrors().Length == 0;
    }

    private static async Task WriteMetaSprites(StreamWriter outputFile, CharacterModel model, string name)
    {
        await outputFile.WriteLineAsync("; This file is auto generated!");
        await outputFile.WriteAsync(Environment.NewLine);

        List<string> animationIndices = [];
        List<string> listOfCollectionFrames = [];

        foreach (KeyValuePair<string, CharacterAnimation> animationItem in model.Animations)
        {
            CharacterAnimation animation = animationItem.Value;

            if (string.IsNullOrEmpty(animation.ID))
            {
                continue;
            }

            animationIndices.Add($"{animation.Name}");

            int frameIndex = 0;
            List<string> frameNames = [];

            #region Frame description
            foreach (KeyValuePair<string, FrameModel> frameItem in animation.Frames)
            {
                FrameModel frameModel = frameItem.Value;

                BankModel? bankModel = ProjectFiles.GetModel<BankModel>(frameModel.BankID);

                if (bankModel == null)
                {
                    continue;
                }

                frameNames.Add($"{name}_{animation.Name}_frame_{frameIndex}_0");

                await WriteFrameInformation0(outputFile, frameNames.Last(), frameModel);

                frameNames.Add($"{name}_{animation.Name}_frame_{frameIndex}_1");

                await WriteFrameInformation1(outputFile, frameNames.Last(), frameModel);

                frameNames.Add($"{name}_{animation.Name}_frame_{frameIndex}_2");

                await WriteFrameInformation2(outputFile, frameNames.Last(), frameModel);

                frameNames.Add($"{name}_{animation.Name}_frame_{frameIndex}_3");

                await WriteFrameInformation3(outputFile, frameNames.Last(), frameModel);

                frameIndex++;
            }
            #endregion

            #region Frames configuration
            await outputFile.WriteLineAsync("    align 1");
            await outputFile.WriteLineAsync($"{name}_{animation.Name}_configuration:");

            int frameDuration = (int)(animation.Speed * FrameRate);

            await outputFile.WriteLineAsync($"    ; number of frames");
            await outputFile.WriteLineAsync($"    db ${frameNames.Count:X2} ; decimal {frameNames.Count}");
            await outputFile.WriteLineAsync($"    ; frame duration");
            await outputFile.WriteLineAsync($"    db ${frameDuration:X2} ; decimal {frameDuration}");

            await outputFile.WriteAsync(Environment.NewLine);
            #endregion

            #region List of frames
            if (frameNames.Count > 0)
            {
                listOfCollectionFrames.Add($"{name}_{animation.Name}_frames");

                await outputFile.WriteLineAsync("    align 4");
                await outputFile.WriteLineAsync(listOfCollectionFrames.Last() + ":");

                foreach (string frameName in frameNames)
                {
                    await outputFile.WriteLineAsync($"    dw {frameName}");
                }

                await outputFile.WriteAsync(Environment.NewLine);
            }
            #endregion
        }

        #region Animation info
        if (animationIndices.Count > 0)
        {
            await outputFile.WriteLineAsync("; aninmation indices");

            for (int i = 0; i < animationIndices.Count; ++i)
            {
                string index = animationIndices[i].ToUpper();
                string nameUpper = name.ToUpper();

                await outputFile.WriteLineAsync($"ANIM_{nameUpper}_{index} = ${i:X2}");
            }

            await outputFile.WriteAsync(Environment.NewLine);
            await outputFile.WriteLineAsync("    align 4");
            await outputFile.WriteLineAsync($"{name}_anim_frames_table:");

            foreach (string frameListName in listOfCollectionFrames)
            {
                await outputFile.WriteLineAsync($"    dw {frameListName}");
            }

            await outputFile.WriteAsync(Environment.NewLine);
            await outputFile.WriteLineAsync("    align 4");
            await outputFile.WriteLineAsync($"{name}_anim_config_table:");

            for (int i = 0; i < animationIndices.Count; ++i)
            {
                string index = animationIndices[i];
                await outputFile.WriteLineAsync($"    dw {name}_{index}_configuration");
            }
        }
        #endregion
    }

    private static async Task WriteFrameInformation0(StreamWriter outputFile, string name, FrameModel frameModel)
    {
        await outputFile.WriteLineAsync("    align 4");
        await outputFile.WriteLineAsync($"{name}:");

        foreach (KeyValuePair<string, CharacterSprite> tileItem in frameModel.Tiles)
        {
            CharacterSprite sprite = tileItem.Value;

            await outputFile.WriteLineAsync($"    dw (ATTR_0_SPRITE_SIZE_00 or 0000000001100100b)");
        }

        await outputFile.WriteAsync(Environment.NewLine);
    }

    private static async Task WriteFrameInformation1(StreamWriter outputFile, string name, FrameModel frameModel)
    {
        await outputFile.WriteLineAsync("    align 4");
        await outputFile.WriteLineAsync($"{name}:");

        foreach (KeyValuePair<string, CharacterSprite> tileItem in frameModel.Tiles)
        {
            CharacterSprite sprite = tileItem.Value;

            await outputFile.WriteLineAsync($"    dw (ATTR_0_SPRITE_SIZE_00 or 0000000001100100b)");
        }

        await outputFile.WriteAsync(Environment.NewLine);
    }

    private static async Task WriteFrameInformation2(StreamWriter outputFile, string name, FrameModel frameModel)
    {
        await outputFile.WriteLineAsync("    align 4");
        await outputFile.WriteLineAsync($"{name}:");

        foreach (KeyValuePair<string, CharacterSprite> tileItem in frameModel.Tiles)
        {
            CharacterSprite sprite = tileItem.Value;

            await outputFile.WriteLineAsync($"    dw (ATTR_0_SPRITE_SIZE_00 or 0000000001100100b)");
        }

        await outputFile.WriteAsync(Environment.NewLine);
    }

    private static async Task WriteFrameInformation3(StreamWriter outputFile, string name, FrameModel frameModel)
    {
        await outputFile.WriteLineAsync("    align 4");
        await outputFile.WriteLineAsync($"{name}:");

        foreach (KeyValuePair<string, CharacterSprite> tileItem in frameModel.Tiles)
        {
            CharacterSprite sprite = tileItem.Value;

            await outputFile.WriteLineAsync($"    dw (ATTR_0_SPRITE_SIZE_00 or 0000000001100100b)");
        }

        await outputFile.WriteAsync(Environment.NewLine);
    }
}
