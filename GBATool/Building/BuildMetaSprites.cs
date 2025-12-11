using GBATool.Enums;

namespace GBATool.Building;

public static class BuildMetaSprites
{
    public static IBuilding Get(OutputFormat outputFormat)
    {
        return outputFormat switch
        {
            OutputFormat.None => EmptyBuilder.Instance,
            OutputFormat.Fasmarm => BuildMetaSpritesFasmarm.Instance,
            OutputFormat.Butano => BuildMetaSpritesButano.Instance,
            _ => EmptyBuilder.Warning($"Format ({outputFormat}) not implemented for Meta Sprites")
        };
    }
}
