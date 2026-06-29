
namespace domain
{
    class Vector2i(int _x, int _y)
    {
        public int X {get; set;} = _x;
        public int Y {get; set;} = _y;

        public static readonly Vector2i ZERO = new(0, 0);
        public static readonly Vector2i UP = new(0, -1);
        public static readonly Vector2i DOWN = new(0, 1);
        public static readonly Vector2i LEFT = new(-1, 0);
        public static readonly Vector2i RIGHT = new(1, 0);
        public static readonly Vector2i[] ALL_DIRECTIONS =
        [
            new(0, -1), new(1, -1), new(1, 0), new(1, 1),
            new(0, 1), new(-1, 1), new(-1, 0), new(-1, -1)
        ];

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

}