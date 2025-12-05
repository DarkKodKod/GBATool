using GBATool.Enums;
using System;

namespace GBATool.Building;

public static class BuildPalettes
{
    public static IBuilding Get(OutputFormat outputFormat)
    {
        return outputFormat switch
        {
            OutputFormat.None => EmptyBuilder.Instance,
            OutputFormat.Fasmarm => BuildPalettesFasmarm.Instance,
            _ => throw new NotImplementedException("Format not implemented")
        };
    }
}
