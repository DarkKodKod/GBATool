using GBATool.Enums;
using System;

namespace GBATool.Building;

public static class BuildBackgrounds
{
    public static IBuilding Get(OutputFormat outputFormat)
    {
        return outputFormat switch
        {
            _ => throw new NotImplementedException("Format not implemented")
        };
    }
}
