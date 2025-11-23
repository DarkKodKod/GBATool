using GBATool.Enums;

namespace GBATool.Building;

public sealed class BuildBackgrounds : Building<BuildBackgrounds>
{
    protected override string FileName { get; } = "backgrounds.asm";
    protected override OutputFormat OutputFormat { get; } = OutputFormat.Fasmarm;
}
