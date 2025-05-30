﻿using Nett;
using System.Linq;
using System.Windows;

namespace GBATool.Models;

public class PaletteModel : AFileModel
{
    private const string _extensionKey = "extensionPalettes";

    [TomlIgnore]
    public override string FileExtension
    {
        get
        {
            if (string.IsNullOrEmpty(_fileExtension))
            {
                _fileExtension = (string)Application.Current.FindResource(_extensionKey);
            }

            return _fileExtension;
        }
    }

    public int[] Colors { get; set; } = [.. Enumerable.Repeat(0, 16)];
}
