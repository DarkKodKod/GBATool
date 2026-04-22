using System;
using System.IO;
using System.Windows;
using Tomlyn;

namespace GBATool.Models;

public abstract class AFileModel
{
    public abstract string FileExtension { get; }

    public string GUID { get; set; }
    public string Version { get; set; }
    public bool DoNotExport { get; set; }

    protected string _fileExtension = string.Empty;

    private const string _modelVersioKey = "modelVersion";

    public AFileModel()
    {
        GUID = Guid.NewGuid().ToString();
        Version = (string)Application.Current.FindResource(_modelVersioKey);
    }

    public void Save(string path, string name)
    {
        try
        {
            TomlSerializerOptions options = new();
            string toml = TomlSerializer.Serialize(this, GetType(), options);
            File.WriteAllText(Path.Combine(path, name + FileExtension), toml);
        }
        catch (IOException ex)
        {
            // Sometimes the IO is overlapped with another one, I dont want to save if this happens
            Console.WriteLine(ex.Message);
            throw;
        }
    }
}
