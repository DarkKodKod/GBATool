using GBATool.Enums;
using System;

namespace GBATool.Building;

public static class BuildMetaSprites
{
    public static IBuilding Get(OutputFormat outputFormat)
    {
        return outputFormat switch
        {
            OutputFormat.None => EmptyBuilder.Instance,
            OutputFormat.Fasmarm => BuildMetaSpritesFasmarm.Instance,
            _ => throw new NotImplementedException("Format not implemented")
        };
    }
}
