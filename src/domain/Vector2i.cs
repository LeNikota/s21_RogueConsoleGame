
namespace domain
{
class Vector2i(int _x, int _y)
{
    public int x = _x;
    public int y = _y;

    public static readonly Vector2i ZERO = new(0, 0);

    public static readonly Vector2i UP = new(0, -1);
    public static readonly Vector2i DOWN = new(0, 1);
    public static readonly Vector2i LEFT = new(-1, 0);
    public static readonly Vector2i RIGHT = new(1, 0);

    public static Vector2i operator +(Vector2i a, Vector2i b)
    {
        return new Vector2i(a.x + b.x, a.y + b.y);
    }

    public static bool operator ==(Vector2i a, Vector2i b)
    {
        return a.x == b.x && a.y == b.y;
    }

    public static bool operator !=(Vector2i a, Vector2i b)
    {
        return a.x != b.x || a.y != b.y;
    }

    public double DistanceTo(Vector2i other)
    {
        //Manhattan distance (square grid based)
        return Math.Abs(x - other.x) + Math.Abs(y - other.y);
        //Euclidean distance:
        //return Math.Sqrt(Math.Pow(other.x - x, 2) + Math.Pow(other.y - y, 2));
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        return this == (Vector2i)obj;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(x, y);
    }

    public override string ToString()
    {
        return $"({x}, {y})";
    }
}

}