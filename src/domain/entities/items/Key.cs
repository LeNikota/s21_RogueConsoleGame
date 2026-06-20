namespace domain
{
    class Key(int x, int y, Color color) : Item(x, y, '*', $"Key ({color})")
    {
        public readonly Color Color = color; 
    }
}