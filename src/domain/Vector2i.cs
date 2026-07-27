
namespace domain;

class Vector2i
{
    public int X { get; set; }
    public int Y { get; set; }

    public Vector2i() { }
    public Vector2i(int x, int y) { X = x; Y = y; }

    public static readonly Vector2i ZERO = new(0, 0);
    public static readonly Vector2i UP = new(0, -1);
    public static readonly Vector2i DOWN = new(0, 1);
    public static readonly Vector2i LEFT = new(-1, 0);
    public static readonly Vector2i RIGHT = new(1, 0);

    public static readonly Vector2i UP_LEFT = new(-1, -1);
    public static readonly Vector2i UP_RIGHT = new(1, -1);
    public static readonly Vector2i DOWN_LEFT = new(-1, 1);
    public static readonly Vector2i DOWN_RIGHT = new(1, 1);

    public static readonly Vector2i[] SIMPLE_DIRECTIONS = [UP, DOWN, LEFT, RIGHT];
    public static readonly Vector2i[] DIAGONAL_DIRECTIONS = [UP_LEFT, UP_RIGHT, DOWN_LEFT, DOWN_RIGHT];
    public static readonly Vector2i[] ALL_DIRECTIONS = [
        UP, DOWN, LEFT, RIGHT,
        UP_LEFT, UP_RIGHT, DOWN_LEFT, DOWN_RIGHT
    ];

    public static Vector2i operator *(Vector2i a, int b)
    {
        return new Vector2i(a.X * b, a.Y * b);
    }

    public static Vector2i operator *(int a, Vector2i b)
    {
        return new Vector2i(a * b.X, a * b.Y);
    }

    public static Vector2i operator +(Vector2i a, Vector2i b)
    {
        return new Vector2i(a.X + b.X, a.Y + b.Y);
    }

    public static bool operator ==(Vector2i a, Vector2i b)
    {
        return a.X == b.X && a.Y == b.Y;
    }

    public static bool operator !=(Vector2i a, Vector2i b)
    {
        return a.X != b.X || a.Y != b.Y;
    }

    public double DistanceTo(Vector2i other)
    {
        //Manhattan distance (square grid based)
        return Math.Abs(X - other.X) + Math.Abs(Y - other.Y);
        //Euclidean distance:
        //return Math.Sqrt(Math.Pow(other.X - X, 2) + Math.Pow(other.Y - Y, 2));
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        return this == (Vector2i)obj;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y);
    }

    public override string ToString()
    {
        return $"({X}, {Y})";
    }
}

