using ArchitectureLibrary.Model;
using GBATool.Enums;
using GBATool.FileSystem;
using GBATool.Models;
using GBATool.Utils;
using GBATool.VOs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

// Information taken from pages:
// - https://gbadev.net/gbadoc/sprites.html
// - https://www.coranac.com/tonc/text/regobj.htm

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

            #region Frame information
            foreach (KeyValuePair<string, FrameModel> frameItem in animation.Frames)
            {
                FrameModel frameModel = frameItem.Value;

                BankModel? bankModel = ProjectFiles.GetModel<BankModel>(frameModel.BankID);

                if (bankModel == null)
                {
                    continue;
                }

                string bankNameLow = "db 0x00, 0x00";
                string bankNameHigh = "db 0x00, 0x00";

                FileModelVO? fileModelVO = ProjectFiles.GetFileModel(bankModel.GUID);
                if (fileModelVO != null)
                {
                    bankNameLow = $"dh (block_{fileModelVO.Name} and 0xFFFF)";
                    bankNameHigh = $"dh ((block_{fileModelVO.Name} shr 16) and 0xFFFF)";
                }

                frameNames.Add($"{name}_{animation.Name}_frame_{frameIndex}");

                int nextFrameIndex = frameIndex + 1 < animation.Frames.Count ? frameIndex + 1 : 0;

                await outputFile.WriteLineAsync("    align 2");
                await outputFile.WriteLineAsync($"{frameNames.Last()}:");

                await outputFile.WriteLineAsync($"    ; number of sprites");
                await outputFile.WriteLineAsync($"    db 0x{frameModel.Tiles.Count:X2}");
                await outputFile.WriteLineAsync($"    ; collision list");
                await outputFile.WriteLineAsync($"    db 0x{frameModel.CollisionInfo.Count:X2}");
                await outputFile.WriteLineAsync($"    ; pointer to the Block source (low 16 bits)");
                await outputFile.WriteLineAsync($"    {bankNameLow}");
                await outputFile.WriteLineAsync($"    ; pointer to the Block source (high 16 bits)");
                await outputFile.WriteLineAsync($"    {bankNameHigh}");
                await outputFile.WriteLineAsync($"    ; pointer to the next frame (low 16 bits)");
                await outputFile.WriteLineAsync($"    dh ({name}_{animation.Name}_frame_{nextFrameIndex} and 0xFFFF)");
                await outputFile.WriteLineAsync($"    ; pointer to the next frame (high 16 bits)");
                await outputFile.WriteLineAsync($"    dh (({name}_{animation.Name}_frame_{nextFrameIndex} shr 16) and 0xFFFF)");

                foreach (KeyValuePair<string, CharacterSprite> tileItem in frameModel.Tiles)
                {
                    CharacterSprite sprite = tileItem.Value;

                    await WriteFrameAttribute0(sprite, outputFile, bankModel, animation);
                    await WriteFrameAttribute1(sprite, outputFile, animation);
                    await WriteFrameAttribute2(sprite, outputFile, bankModel, model);
                    await WriteFrameAttribute3(sprite, outputFile);
                }

                await outputFile.WriteAsync(Environment.NewLine);

                frameIndex++;
            }
            #endregion

            #region Frames configuration
            await outputFile.WriteLineAsync("    align 4");
            await outputFile.WriteLineAsync($"{name}_{animation.Name}_configuration:");

            int frameDuration = (int)(animation.Speed * FrameRate);

            await outputFile.WriteLineAsync($"    ; number of frames");
            await outputFile.WriteLineAsync($"    db 0x{frameNames.Count:X2}; decimal {frameNames.Count}");
            await outputFile.WriteLineAsync($"    ; frame duration");
            await outputFile.WriteLineAsync($"    db 0x{frameDuration:X2} ; decimal {frameDuration}");
            await outputFile.WriteLineAsync($"    ; vertical axis");
            await outputFile.WriteLineAsync($"    db {animation.VerticalAxis:X2}; decimal {animation.VerticalAxis}");
            await outputFile.WriteLineAsync($"    ; Empty");
            await outputFile.WriteLineAsync($"    db 0x00");
            await outputFile.WriteLineAsync($"    ; pointer to the first frame");
            await outputFile.WriteLineAsync($"    dw {frameNames.First()}");

            await outputFile.WriteAsync(Environment.NewLine);
            #endregion
        }

        #region Animation info
        if (animationIndices.Count > 0)
        {
            await outputFile.WriteLineAsync("    align 4");
            await outputFile.WriteLineAsync($"{name}_anim_config_table:");

            for (int i = 0; i < animationIndices.Count; ++i)
            {
                string indexName = animationIndices[i];
                await outputFile.WriteLineAsync($"    dw {name}_{indexName}_configuration");
            }

            await outputFile.WriteAsync(Environment.NewLine);
            await outputFile.WriteLineAsync("; animation indices");

            for (int i = 0; i < animationIndices.Count; ++i)
            {
                string indexName = animationIndices[i].ToUpper();
                string nameUpper = name.ToUpper();

                await outputFile.WriteLineAsync($"ANIM_{nameUpper}_{indexName} = {i}");
            }
        }
        #endregion
    }

    private static async Task WriteFrameAttribute0(CharacterSprite sprite, StreamWriter outputFile, BankModel bankModel, CharacterAnimation animation)
    {
        string yPosition = Util.ConvertShortToBits((short)(sprite.Position.Y - animation.RelativeOrigin.Y));
        yPosition += "b";

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

    private static async Task WriteFrameAttribute1(CharacterSprite sprite, StreamWriter outputFile, CharacterAnimation animation)
    {
        string xPosition = Util.ConvertIntToBits((int)(sprite.Position.X - animation.RelativeOrigin.X), 0x1FF);
        xPosition += "b";

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

    private static async Task WriteFrameAttribute2(CharacterSprite sprite, StreamWriter outputFile, BankModel bankModel, CharacterModel characterModel)
    {
        string tileIndex = Util.ConvertShortToBits((short)bankModel.GetTileIndex(sprite.SpriteID));
        tileIndex += "b";

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
            paletteIndex = Util.ConvertShortToBits((short)characterModel.PaletteIndex);
            paletteIndex += "b";
        }

        string attribute2 = $"{paletteIndex} or {priority} or {tileIndex}";

        await outputFile.WriteAsync($"    dh ({attribute2})" + Environment.NewLine);
    }

    private static async Task WriteFrameAttribute3(CharacterSprite sprite, StreamWriter outputFile)
    {
        string attribute3 = "0000000000000000b";

        if (sprite.AffineIsEnabled)
        {
            string fraction = "0000000000000000b";
            string integer = "0000000000000000b";
            string signBit = "0000000000000000b";

            attribute3 = $"{signBit} or {integer} or {fraction}";
        }

        await outputFile.WriteAsync($"    dh ({attribute3})" + Environment.NewLine);
    }
}
