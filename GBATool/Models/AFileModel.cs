using Nett;
using System;
using System.IO;

namespace GBATool.Models;

public abstract class AFileModel
{
    public abstract string FileExtension { get; }

    public string GUID { get; set; }

    protected string _fileExtension = string.Empty;

    public AFileModel()
    {
        GUID = Guid.NewGuid().ToString();
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
