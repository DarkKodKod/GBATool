using GBATool.Enums;

namespace GBATool.Models
{
    public struct SpriteModel
    {
        public string ID { get; set; }
        public SpriteShape Shape { get; set; }
        public SpriteSize Size { get; set; }
        public int PosX { get; set; }
        public int PosY { get; set; }

        public override readonly bool Equals(object? obj)
        {
            if (obj == null || obj is not SpriteModel)
            {
                return false;
            }

            return PosX.Equals(((SpriteModel)obj).PosX)
                && PosY.Equals(((SpriteModel)obj).PosY)
                && Shape.Equals(((SpriteModel)obj).Shape)
                && Size.Equals(((SpriteModel)obj).Size);
        }

        public override readonly int GetHashCode()
        {
            return System.HashCode.Combine(PosX, PosY, Shape, Size);
        }

        public static bool operator ==(SpriteModel left, SpriteModel right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(SpriteModel left, SpriteModel right)
        {
            return !(left == right);
        }
    }
}
