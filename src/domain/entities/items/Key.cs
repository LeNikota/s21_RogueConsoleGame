namespace domain;

class Key : Item
{
    public Key() : base() { }

    public Key(int x, int y, Color color = Color.None)
        : base(x, y, '*', $"Key ({color})", color)
    {
    }

    public override string ToString()
    {
        return $"{Color} key";
    }
}