using GBATool.Enums;
using System;

namespace GBATool.Building;

public static class BuildPalettes
{
    public static IBuilding Get(OutputFormat outputFormat)
    {
        return outputFormat switch
        {
            OutputFormat.Fasmarm => BuildPalettesFasmarm.Instance,
            _ => throw new NotImplementedException("Format not implemented")
        };
    }
}
