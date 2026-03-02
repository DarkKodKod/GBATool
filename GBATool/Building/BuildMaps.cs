using GBATool.Enums;

namespace GBATool.Building;

public static class BuildMaps
{
    public static IBuilding Get(OutputFormat outputFormat)
    {
        return outputFormat switch
        {
            OutputFormat.None => EmptyBuilder.Instance,
            OutputFormat.Fasmarm => BuildMapsFasmarm.Instance,
            _ => EmptyBuilder.Warning($"Format ({outputFormat}) not implemented for Maps")
        };
    }
}
