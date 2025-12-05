using GBATool.Enums;

namespace GBATool.Building;

public static class BuildMemoryBanks
{
    public static IBuilding Get(OutputFormat outputFormat)
    {
        return outputFormat switch
        {
            OutputFormat.None => EmptyBuilder.Instance,
            OutputFormat.Binary => BuildMemoryBanksBinary.Instance,
            _ => EmptyBuilder.Warning($"Format ({outputFormat}) not implemented for Memory Banks")
        };
    }
}
