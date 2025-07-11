﻿namespace GBATool.Enums;

//shape\size  00	01	    10	    11
//   00	      8x8	16x16	32x32	64x64
//   01	      16x8	32x8	32x16	64x32
//   10	      8x16	8x32	16x32	32x64
public enum SpriteShape
{
    Shape00 = 0,
    Shape01 = 1,
    Shape10 = 2
}

public enum SpriteSize
{
    Size00 = 0,
    Size01 = 1,
    Size10 = 2,
    Size11 = 3
}

public enum ColorMode
{
    Mode4bpp = 0,
    Mode8bpp = 1
}

public enum BitsPerPixel
{
    f4bpp = 4,
    f8bpp = 8
}

public enum Priority
{
    Lowest = 0,
    Low = 1,
    High = 2,
    Highest = 3,
}