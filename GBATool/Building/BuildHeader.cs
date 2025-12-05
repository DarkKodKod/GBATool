using GBATool.Enums;

namespace GBATool.Building;

public static class BuildHeader
{
    public static IBuilding Get(OutputFormat outputFormat)
    {
        return outputFormat switch
        {
            OutputFormat.None => EmptyBuilder.Instance,
            OutputFormat.Fasmarm => BuildHeaderFasmarm.Instance,
            _ => EmptyBuilder.Warning($"Format ({outputFormat}) not implemented for Headers")
        };
    }
}
