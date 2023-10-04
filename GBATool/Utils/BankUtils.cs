using GBATool.Models;
using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace GBATool.Utils
{
    public static class BankUtils
    {
        internal static WriteableBitmap CreateImage(BankModel model, ref Dictionary<string, WriteableBitmap> bitmapCache)
        {
            WriteableBitmap bankBitmap = BitmapFactory.New(128, 128);

            foreach (SpriteModel sprite in model.Sprites)
            {

            }

            return bankBitmap;
        }
    }
}
