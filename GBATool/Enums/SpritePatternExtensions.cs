using System;
using System.ComponentModel;
using System.Linq;

namespace GBATool.Enums;

public static class SpritePatternExtensions
{
    public static string Description(this SpritePattern spritePattern)
    {
        Type type = typeof(SpritePattern);

        string? enumValueString = spritePattern.ToString();

        if (enumValueString == null)
            return string.Empty;

        return type?.GetField(enumValueString)?.
            GetCustomAttributes(
                typeof(DescriptionAttribute), false).
                FirstOrDefault() is DescriptionAttribute descriptionAttribute ? descriptionAttribute.Description : enumValueString;
    }
}
