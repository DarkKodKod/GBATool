using GBATool.ViewModels;
using System.Collections.Generic;

namespace GBATool.VOs
{
    public class ProjectOpenVO
    {
        public List<ProjectItem> Items { get; set; } = new();
        public string ProjectName { get; set; } = "";
    }
}
