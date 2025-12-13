using GBATool.Enums;

namespace GBATool.Building;

public static class BuildPalettes
{
    public static IBuilding Get(OutputFormat outputFormat)
    {
        return outputFormat switch
        {
            OutputFormat.None => EmptyBuilder.Instance,
            OutputFormat.Fasmarm => BuildPalettesFasmarm.Instance,
            OutputFormat.Butano => BuildPalettesButano.Instance,
            _ => EmptyBuilder.Warning($"Format ({outputFormat}) not implemented for Palettes")
        };
    }
}
