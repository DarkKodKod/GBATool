using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace GBATool.Building;

public abstract class Building<T> where T : class, new()
{
    private readonly List<string> _errors = [];

    private static readonly Lazy<T> instance = new(() => new T());

    protected abstract string FileName { get; }

    protected Building()
    {
    }

    public static T Instance => instance.Value;

    public async Task<bool> Generate(string outputPath)
    {
        PrepareGenerate();

        string filePath = Path.Combine(outputPath, FileName);

        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
        catch
        {
            return false;
        }

        return await DoGenerate(filePath);
    }

    protected virtual async Task<bool> DoGenerate(string filePath)
    {
        return await Task.FromResult(true);
    }

    protected void AddError(string error)
    {
        _errors.Add(error);
    }

    public string[] GetErrors()
    {
        return [.. _errors];
    }

    private void PrepareGenerate()
    {
        _errors.Clear();
    }
}
