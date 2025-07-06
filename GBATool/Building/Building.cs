using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace GBATool.Building;

public abstract class Building<T> where T : class, new()
{
    private readonly List<string> _errors = [];
    private readonly List<string> _warnings = [];

    private static readonly Lazy<T> instance = new(() => new T());

    protected abstract string FileName { get; }

    public static T Instance => instance.Value;

    public async Task<bool> Generate(string outputPath)
    {
        PrepareGenerate();

        if (!string.IsNullOrEmpty(FileName))
        {
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

            return await DoGenerate(filePath).ConfigureAwait(false);
        }

        return await DoGenerate().ConfigureAwait(false);
    }

    protected virtual async Task<bool> DoGenerate(string filePath)
    {
        return await Task.FromResult(true);
    }

    protected virtual async Task<bool> DoGenerate()
    {
        return await Task.FromResult(true);
    }

    protected void AddError(string error)
    {
        _errors.Add(error);
    }

    protected void AddWarning(string warning)
    {
        _warnings.Add(warning);
    }

    public string[] GetErrors()
    {
        return [.. _errors];
    }

    public string[] GetWarnings()
    {
        return [.. _warnings];
    }

    private void PrepareGenerate()
    {
        _errors.Clear();
        _warnings.Clear();
    }
}
