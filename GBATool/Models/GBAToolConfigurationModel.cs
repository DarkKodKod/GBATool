using ArchitectureLibrary.Model;
using ArchitectureLibrary.Signals;
using GBATool.Signals;
using GBATool.VOs;
using Nett;
using System.IO;
using System.Windows;

namespace GBATool.Models;

public class GBAToolConfigurationModel : ISingletonModel
{
    public int MaxRencetProjectsCount { get; set; } = 9;
    public string DefaultProjectPath { get; set; } = string.Empty;
    public int WindowSizeX { get; set; } = 940;
    public int WindowSizeY { get; set; } = 618;
    public bool FullScreen { get; set; }
    public string[] RecentProjects { get; set; }
    public bool EnableOnionSkin { get; set; }
    public bool ShowCollisions { get; set; }
    public double OnionSkinOpacity { get; set; } = 0.25;
    public bool KeepBuildDialogOpen { get; set; }
    public string Version { get; private set; } = string.Empty;

    private const string _modelVersioKey = "modelVersion";
    private const string _configfileNameKey = "configurationFileName";
    private readonly string _configFileName = string.Empty;

    private bool _loaded;

    public GBAToolConfigurationModel()
    {
        RecentProjects = new string[MaxRencetProjectsCount];

        for (int i = 0; i < RecentProjects.Length; ++i)
        {
            RecentProjects[i] = "";
        }

        _configFileName = @".\" + (string)Application.Current.FindResource(_configfileNameKey) + Toml.FileExtension;

        Version = (string)Application.Current.FindResource(_modelVersioKey);
    }

    public void Copy(GBAToolConfigurationModel copy)
    {
        DefaultProjectPath = copy.DefaultProjectPath;
        RecentProjects = copy.RecentProjects;
        MaxRencetProjectsCount = copy.MaxRencetProjectsCount;
        WindowSizeX = copy.WindowSizeX;
        WindowSizeY = copy.WindowSizeY;
        FullScreen = copy.FullScreen;
        EnableOnionSkin = copy.EnableOnionSkin;
        ShowCollisions = copy.ShowCollisions;
        OnionSkinOpacity = copy.OnionSkinOpacity;
        KeepBuildDialogOpen = copy.KeepBuildDialogOpen;
        Version = copy.Version;
    }

    public void Load()
    {
        bool exists = File.Exists(_configFileName);

        if (exists)
        {
            Copy(Toml.ReadFile<GBAToolConfigurationModel>(_configFileName));
        }
        else
        {
            Save();
        }

        WindowVO vo = new() { SizeX = WindowSizeX, SizeY = WindowSizeY, IsFullScreen = FullScreen };

        SignalManager.Get<LoadConfigSuccessSignal>().Dispatch();
        SignalManager.Get<SetUpWindowPropertiesSignal>().Dispatch(vo);

        _loaded = true;
    }

    public void Save()
    {
        if (!_loaded)
        {
            return;
        }

        Toml.WriteFile(this, _configFileName);
    }

    /// <summary>
    /// This method insert the project path to the RecentProjects array which has a limit,
    /// so it will shift all entries one slot to the right and left the last one out
    /// </summary>
    /// <param name="projectFullPath"></param>
    public void InsertToRecentProjects(string projectFullPath)
    {
        string[] tmpArray = RecentProjects;

        string[] newArray = new string[MaxRencetProjectsCount];
        newArray[0] = projectFullPath;

        int count = 1;

        for (int i = 0; i < tmpArray.Length; ++i)
        {
            if (tmpArray[i] != projectFullPath)
            {
                newArray[count] = tmpArray[i];
                count++;

                if (count >= MaxRencetProjectsCount)
                {
                    break;
                }
            }
        }

        RecentProjects = newArray;
    }
}
