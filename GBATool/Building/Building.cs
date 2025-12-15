using GBATool.Enums;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace GBATool.Building;

public interface IBuilding
{
    Task<bool> Generate();
    string[] GetErrors();
    string[] GetWarnings();
    abstract OutputFormat GetFormat();
}

public sealed class EmptyBuilder : IBuilding
{
    public static EmptyBuilder Instance { get; } = new();
    public Task<bool> Generate() { return Task.FromResult(true); }
    public string[] GetErrors() { return []; }
    public string[] GetWarnings() { return _warnings.Count > 0 ? [.. _warnings] : []; }
    public OutputFormat GetFormat() { return OutputFormat.None; }
    public static EmptyBuilder Warning(string warning)
    {
        EmptyBuilder builder = new();
        builder._warnings.Add(warning);
        return builder;
    }
    private readonly List<string> _warnings = [];
}

public abstract class Building<TBuilder> : IBuilding
    where TBuilder : class, new()
{
    private readonly List<string> _errors = [];
    private readonly List<string> _warnings = [];

    protected abstract string FileName { get; }
    protected abstract OutputFormat OutputFormat { get; }
    protected abstract string OutputPath { get; }

    public static TBuilder Instance { get; } = new();

    public OutputFormat GetFormat() { return OutputFormat; }

    public async Task<bool> Generate()
    {
        if (!CheckValidFolder(OutputPath))
        {
            AddError($"Invalid path: {OutputPath}");
            return false;
        }

        PrepareGenerate();

        if (!string.IsNullOrEmpty(FileName))
        {
            string filePath = Path.Combine(OutputPath, FileName);

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

    private static bool CheckValidFolder(string path)
    {
        try
        {
            string result = Path.GetFullPath(path);

            return Directory.Exists(result);
        }
        catch
        {
            return false;
        }
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
