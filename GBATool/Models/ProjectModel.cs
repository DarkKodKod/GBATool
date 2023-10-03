using ArchitectureLibrary.Model;
using ArchitectureLibrary.Signals;
using GBATool.Enums;
using GBATool.Signals;
using Nett;

namespace GBATool.Models
{
    public class ProjectModel : ISingletonModel
    {
        public class BuildConfig
        {
            public string OutputFilePath { get; set; } = "";

            public BuildConfig()
            {
                Reset();
            }

            public void Reset()
            {
                OutputFilePath = "";
            }
        }

        public int Version { get; set; } = 0;
        public string Name { get; set; } = "";
        [TomlIgnore] public string ProjectFilePath { get; set; } = "";
        [TomlIgnore] public string ProjectPath { get; set; } = "";
        public BuildConfig Build { get; set; } = new();
        public SpritePattern SpritePatternFormat { get; set; } = SpritePattern.Format1D;

        public ProjectModel()
        {
        }

        public void Reset()
        {
            ProjectFilePath = "";
            ProjectPath = "";
            Name = "";
            SpritePatternFormat = SpritePattern.Format1D;
            Build.Reset();
        }

        public bool IsOpen()
        {
            return !string.IsNullOrEmpty(ProjectFilePath) && !string.IsNullOrEmpty(ProjectPath);
        }

        public void Copy(ProjectModel copy)
        {
            Name = copy.Name;
            Version = copy.Version;
            SpritePatternFormat = copy.SpritePatternFormat;

            Build.OutputFilePath = copy.Build.OutputFilePath;
        }

        public void Load(string path, string filePath)
        {
            ProjectPath = path;
            ProjectFilePath = filePath;

            Copy(Toml.ReadFile<ProjectModel>(ProjectFilePath));
        }

        public void Save(string path)
        {
            ProjectFilePath = path;

            Save();
        }

        public void Save()
        {
            if (string.IsNullOrEmpty(ProjectFilePath))
            {
                return;
            }

            Toml.WriteFile(this, ProjectFilePath);

            SignalManager.Get<ProjectConfigurationSavedSignal>().Dispatch();
        }
    }
}
