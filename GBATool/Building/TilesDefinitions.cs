﻿using System.Threading.Tasks;

namespace GBATool.Building;

static class TilesDefinitions
{
    public static async Task<bool> Generate(string outputSourcePath)
    {
        return await Task.FromResult(true);
    }

    public static string GetErrors()
    {
        return string.Empty;
    }
}