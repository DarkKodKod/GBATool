using GBATool.Enums;
using System;

namespace GBATool.Building;

public static class BuildMemoryBanks
{
    public static IBuilding Get(OutputFormat outputFormat)
    {
        return outputFormat switch
        {
            OutputFormat.None => EmptyBuilder.Instance,
            OutputFormat.Binary => BuildMemoryBanksBinary.Instance,
            _ => throw new NotImplementedException("Format not implemented")
        };
    }
}
