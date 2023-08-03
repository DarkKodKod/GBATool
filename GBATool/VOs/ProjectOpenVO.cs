using GBATool.ViewModels;
using System.Collections.Generic;

namespace GBATool.VOs
{
    public record ProjectOpenVO
    {
        public List<ProjectItem> Items { get; init; } = new();
        public string ProjectName { get; init; } = string.Empty;
    }
}
