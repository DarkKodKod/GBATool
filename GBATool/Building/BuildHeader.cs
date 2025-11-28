using GBATool.Enums;
using System;

namespace GBATool.Building;

public static class BuildHeader
{
    public static IBuilding Get(OutputFormat outputFormat)
    {
        return outputFormat switch
        {
            OutputFormat.Fasmarm => BuildHeaderFasmarm.Instance,
            _ => throw new NotImplementedException("Format not implemented")
        };
    }
}
