using ArchitectureLibrary.Model;
using GBATool.Enums;
using GBATool.FileSystem;
using GBATool.Models;
using GBATool.Utils;
using GBATool.VOs;
using System;
using System.Collections;
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

                await WriteFrameAttribute0(outputFile, frameNames.Last(), frameModel, bankModel);

                frameNames.Add($"{name}_{animation.Name}_frame_{frameIndex}_1");

                await WriteFrameAttribute1(outputFile, frameNames.Last(), frameModel);

                frameNames.Add($"{name}_{animation.Name}_frame_{frameIndex}_2");

                await WriteFrameAttribute2(outputFile, frameNames.Last(), frameModel, bankModel, model);

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

    private static async Task WriteFrameAttribute0(StreamWriter outputFile, string name, FrameModel frameModel, BankModel bankModel)
    {
        await outputFile.WriteLineAsync("    align 2");
        await outputFile.WriteLineAsync($"{name}:");

        foreach (KeyValuePair<string, CharacterSprite> tileItem in frameModel.Tiles)
        {
            CharacterSprite sprite = tileItem.Value;

            string yPosition = "0000000000000000b"; // TODO

            string objectMode = "0000000000000000b";

            if (sprite.AffineIsEnabled)
            {
                objectMode = "ATTR_0_ROTATION_SCALING";

                if (sprite.IsHidden)
                {
                    objectMode += "or ATTR_0_HIDE";
                }
                else if (sprite.IsDoubleSized)
                {
                    objectMode += "or ATTR_0_DOUBLE_SIZED";
                }
            }

            string gfxMode = "0000000000000000b";

            if (sprite.EnableAlphaBlending && sprite.IsMask)
            {
                gfxMode = "ATTR_0_SEMI_TRANSPARENT or ATTR_0_TRANSPARENT_WIND";
            }
            else if (sprite.IsMask)
            {
                gfxMode = "ATTR_0_TRANSPARENT_WIND";
            }
            else if (sprite.EnableAlphaBlending)
            {
                gfxMode = "ATTR_0_SEMI_TRANSPARENT";
            }

            string mosaic = sprite.EnableMosaic ? "ATTR_0_MOSAIC" : "0000000000000000b";
            string colorMode = bankModel.Use256Colors ? "ATTR_0_COLOR_DEPTH" : "0000000000000000b";

            SpriteShape shape = SpriteShape.Shape00;
            SpriteSize size = SpriteSize.Size00;

            SpriteUtils.ConvertToShapeSize(sprite.Width, sprite.Height, ref shape, ref size);

            string spriteShape = "ATTR_0_SPRITE_SIZE_00";

            switch (shape)
            {
                case SpriteShape.Shape01:
                    spriteShape = "ATTR_0_SPRITE_SIZE_01";
                    break;
                case SpriteShape.Shape10:
                    spriteShape = "ATTR_0_SPRITE_SIZE_10";
                    break;
                case SpriteShape.Shape00:
                default:
                    break;
            }

            string attribute0 = $"{spriteShape} or {colorMode} or {mosaic} or {gfxMode} or {objectMode} or {yPosition}";

            await outputFile.WriteAsync($"    dh ({attribute0})" + Environment.NewLine);
        }

        await outputFile.WriteAsync(Environment.NewLine);
    }

    private static async Task WriteFrameAttribute1(StreamWriter outputFile, string name, FrameModel frameModel)
    {
        await outputFile.WriteLineAsync("    align 2");
        await outputFile.WriteLineAsync($"{name}:");

        foreach (KeyValuePair<string, CharacterSprite> tileItem in frameModel.Tiles)
        {
            CharacterSprite sprite = tileItem.Value;

            string xPosition = "00000000000000000b"; // TODO

            SpriteShape shape = SpriteShape.Shape00;
            SpriteSize size = SpriteSize.Size00;

            SpriteUtils.ConvertToShapeSize(sprite.Width, sprite.Height, ref shape, ref size);

            string spriteSize = "ATTR_1_SPRITE_SIZE_00";

            switch (size)
            {
                case SpriteSize.Size01:
                    spriteSize = "ATTR_1_SPRITE_SIZE_01";
                    break;
                case SpriteSize.Size10:
                    spriteSize = "ATTR_1_SPRITE_SIZE_10";
                    break;
                case SpriteSize.Size11:
                    spriteSize = "ATTR_1_SPRITE_SIZE_11";
                    break;
                case SpriteSize.Size00:
                default:
                    break;
            }

            string attribute1;

            if (!sprite.AffineIsEnabled)
            {
                string horizontalFlip = sprite.FlipHorizontal ? "ATTR_1_FLIP_HORIZONTAL" : "0000000000000000b";
                string verticalFlip = sprite.FlipVertical ? "ATTR_1_FLIP_VERTICAL" : "0000000000000000b";

                attribute1 = $"{spriteSize} or {verticalFlip} or {horizontalFlip} or {xPosition}";
            }
            else
            {
                string affineIndex = "0000000000000000b";

                attribute1 = $"{spriteSize} or {affineIndex} or {xPosition}";
            }

            await outputFile.WriteAsync($"    dh ({attribute1})" + Environment.NewLine);
        }

        await outputFile.WriteAsync(Environment.NewLine);
    }

    private static async Task WriteFrameAttribute2(StreamWriter outputFile, string name, FrameModel frameModel, BankModel bankModel, CharacterModel characterModel)
    {
        await outputFile.WriteLineAsync("    align 2");
        await outputFile.WriteLineAsync($"{name}:");

        foreach (KeyValuePair<string, CharacterSprite> tileItem in frameModel.Tiles)
        {
            CharacterSprite sprite = tileItem.Value;

            string tileIndex = "0000000000000000b"; // TODO

            string priority = sprite.Priority switch
            {
                Priority.Highest => "0000110000000000b",
                Priority.High => "0000100000000000b",
                Priority.Low => "0000010000000000b",
                _ => "0000000000000000b",
            };

            string paletteIndex = "0000000000000000b";

            if (!bankModel.Use256Colors)
            {
                BitArray b = new(new byte[] { (byte)characterModel.PaletteIndex });
                char[] bits = [.. b.Cast<bool>().Select(bit => bit ? '1' : '0')];

                paletteIndex = new(bits);
                paletteIndex += "00000000b";
            }

            string attribute2 = $"{paletteIndex} or {priority} or {tileIndex}";

            await outputFile.WriteAsync($"    dh ({attribute2})" + Environment.NewLine);
        }

        await outputFile.WriteAsync(Environment.NewLine);
    }
}
