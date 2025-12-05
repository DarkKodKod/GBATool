using GBATool.Enums;

namespace GBATool.Building;

public static class BuildBackgrounds
{
    public static IBuilding Get(OutputFormat outputFormat)
    {
        return outputFormat switch
        {
            OutputFormat.None => EmptyBuilder.Instance,
            _ => EmptyBuilder.Warning($"Format ({outputFormat}) not implemented for Backgrounds")
        };
    }
}
