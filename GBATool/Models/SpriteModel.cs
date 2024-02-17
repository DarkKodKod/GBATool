using GBATool.Enums;

namespace GBATool.Models;

public class SpriteModel
{
    public string ID { get; set; } = string.Empty;
    public SpriteShape Shape { get; set; }
    public SpriteSize Size { get; set; }
    public int PosX { get; set; }
    public int PosY { get; set; }
    public string TileSetID { get; set; } = string.Empty;
    public string Alias { get; set; } = string.Empty;

    public override bool Equals(object? obj)
    {
        if (obj is null or not SpriteModel)
        {
            return false;
        }

        return PosX.Equals(((SpriteModel)obj).PosX)
            && PosY.Equals(((SpriteModel)obj).PosY)
            && Shape.Equals(((SpriteModel)obj).Shape)
            && Size.Equals(((SpriteModel)obj).Size)
            && TileSetID.Equals(((SpriteModel)obj).TileSetID);
    }

    public override int GetHashCode()
    {
        return System.HashCode.Combine(PosX, PosY, Shape, Size, TileSetID);
    }

    //null == null     //true
    //null != null     //false
    //null == nonNull  //false
    //null != nonNull  //true

    public static bool operator ==(SpriteModel? left, SpriteModel? right)
    {
        if (left is null && right is null)
            return true;

        if (left is null || right is null)
            return false;

        return left.Equals(right);
    }

    public static bool operator !=(SpriteModel? left, SpriteModel? right)
    {
        return !(left == right);
    }
}
