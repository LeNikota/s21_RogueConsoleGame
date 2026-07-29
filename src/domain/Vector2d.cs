namespace domain;

class Vector2d
{
    public double X { get; set; }
    public double Y { get; set; }

    public Vector2d() { }

    public Vector2d(double x, double y)
    {
        X = x;
        Y = y;
    }

    public static implicit operator Vector2d(Vector2i v)
    {
        return new Vector2d(v.X, v.Y);
    }
    
    public Vector2i ToVector2i()
    {
        return new Vector2i((int)Math.Round(X), (int)Math.Round(Y));
    }

    // Operators
    public static Vector2d operator *(Vector2d a, double b)
    {
        return new Vector2d(a.X * b, a.Y * b);
    }

    public static Vector2d operator *(double a, Vector2d b)
    {
        return new Vector2d(a * b.X, a * b.Y);
    }

    public static Vector2d operator +(Vector2d a, Vector2d b)
    {
        return new Vector2d(a.X + b.X, a.Y + b.Y);
    }

    public static Vector2d operator -(Vector2d a, Vector2d b)
    {
        return new Vector2d(a.X - b.X, a.Y - b.Y);
    }

    public static bool operator ==(Vector2d a, Vector2d b)
    {
        return a.X == b.X && a.Y == b.Y;
    }

    public static bool operator !=(Vector2d a, Vector2d b)
    {
        return a.X != b.X || a.Y != b.Y;
    }

    // Euclidean distance. It is better for 3D/Ray Casting calculations.
    public double DistanceTo(Vector2d other)
    {
        return Math.Sqrt(Math.Pow(other.X - X, 2) + Math.Pow(other.Y - Y, 2));
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj is not Vector2d v) return false;
        return this == v;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y);
    }

    public override string ToString()
    {
        return $"({X:F2}, {Y:F2})";
    }
}