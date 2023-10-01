using GBATool.Enums;

namespace GBATool.Utils
{
    public static class SpriteUtils
    {
        public static void ConvertToShapeSize(int width, int height, ref SpriteShape shape, ref SpriteSize size)
        {
            //shape\size  00	01	    10	    11
            //   00	      8x8	16x16	32x32	64x64
            //   01	      16x8	32x8	32x16	64x32
            //   10	      8x16	8x32	16x32	32x64

            if (width == 8)
            {
                switch (height)
                {
                    case 8: shape = SpriteShape.Shape00; size = SpriteSize.Size00; break;
                    case 16: shape = SpriteShape.Shape10; size = SpriteSize.Size00; break;
                    case 32: shape = SpriteShape.Shape10; size = SpriteSize.Size01; break;
                }
            }
            else if (width == 16)
            {
                switch (height)
                {
                    case 8: shape = SpriteShape.Shape01; size = SpriteSize.Size00; break;
                    case 16: shape = SpriteShape.Shape00; size = SpriteSize.Size01; break;
                    case 32: shape = SpriteShape.Shape10; size = SpriteSize.Size10; break;
                }
            }
            else if (width == 32)
            {
                switch (height)
                {
                    case 8: shape = SpriteShape.Shape01; size = SpriteSize.Size01; break;
                    case 16: shape = SpriteShape.Shape01; size = SpriteSize.Size10; break;
                    case 32: shape = SpriteShape.Shape00; size = SpriteSize.Size10; break;
                    case 64: shape = SpriteShape.Shape10; size = SpriteSize.Size11; break;
                }
            }
            else if (width == 64)
            {
                switch (height)
                {
                    case 32: shape = SpriteShape.Shape01; size = SpriteSize.Size11; break;
                    case 64: shape = SpriteShape.Shape00; size = SpriteSize.Size11; break;
                }
            }
        }

        public static void ConvertToWidthHeight(SpriteShape shape, SpriteSize size, ref int width, ref int height)
        {
            //shape\size  00	01	    10	    11
            //   00	      8x8	16x16	32x32	64x64
            //   01	      16x8	32x8	32x16	64x32
            //   10	      8x16	8x32	16x32	32x64

            if (size == SpriteSize.Size00)
            {
                switch (shape)
                {
                    case SpriteShape.Shape00: width = 8; height = 8; break;
                    case SpriteShape.Shape01: width = 16; height = 8; break;
                    case SpriteShape.Shape10: width = 8; height = 16; break;
                }
            }
            else if (size == SpriteSize.Size01)
            {
                switch (shape)
                {
                    case SpriteShape.Shape00: width = 16; height = 16; break;
                    case SpriteShape.Shape01: width = 32; height = 8; break;
                    case SpriteShape.Shape10: width = 8; height = 32; break;
                }
            }
            else if (size == SpriteSize.Size10)
            {
                switch (shape)
                {
                    case SpriteShape.Shape00: width = 32; height = 32; break;
                    case SpriteShape.Shape01: width = 32; height = 16; break;
                    case SpriteShape.Shape10: width = 16; height = 32; break;
                }
            }
            else if (size == SpriteSize.Size11)
            {
                switch (shape)
                {
                    case SpriteShape.Shape00: width = 64; height = 64; break;
                    case SpriteShape.Shape01: width = 64; height = 32; break;
                    case SpriteShape.Shape10: width = 32; height = 64; break;
                }
            }
        }
    }
}
