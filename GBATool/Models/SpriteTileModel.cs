namespace GBATool.Models;

public class SpriteTileModel
{
    public string ID { get; set; } = string.Empty;
    public string TileSetID { get; set; } = string.Empty;
    public string Alias { get; set; } = string.Empty;

    public override bool Equals(object? obj)
    {
        if (obj is null or not SpriteTileModel)
        {
            return false;
        }

        return ID.Equals(((SpriteTileModel)obj).ID)
            && Alias.Equals(((SpriteTileModel)obj).Alias)
            && TileSetID.Equals(((SpriteTileModel)obj).TileSetID);
    }

    public override int GetHashCode()
    {
        return System.HashCode.Combine(ID, Alias, TileSetID);
    }

    //null == null     //true
    //null != null     //false
    //null == nonNull  //false
    //null != nonNull  //true

    public static bool operator ==(SpriteTileModel? left, SpriteTileModel? right)
    {
        if (left is null && right is null)
            return true;

        if (left is null || right is null)
            return false;

        return left.Equals(right);
    }

    public static bool operator !=(SpriteTileModel? left, SpriteTileModel? right)
    {
        return !(left == right);
    }
}
