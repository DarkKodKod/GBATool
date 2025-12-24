using Nett;
using System;
using System.IO;
using System.Windows;

namespace GBATool.Models;

public abstract class AFileModel
{
    public abstract string FileExtension { get; }

    public string GUID { get; set; }
    public string Version { get; set; }

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
            Toml.WriteFile(this, Path.Combine(path, name + FileExtension));
        }
        catch (IOException ex)
        {
            // Sometimes the IO is overlapped with another one, I dont want to save if this happens
            Console.WriteLine(ex.Message);
            throw;
        }
    }
}
