using GBATool.Enums;

namespace GBATool.Building;

public sealed class BuildTilesDefinitions : Building<BuildTilesDefinitions>
{
    protected override string FileName { get; } = "tilesdefinitions.asm";
    protected override OutputFormat OutputFormat { get; } = OutputFormat.Fasmarm;
}
