namespace domain
{
    public enum Color { None, White, Red, Green, Blue, Yellow, Cyan, Magenta, Black }

    class Entity(int x, int y, uint _icon, Color color = Color.None)
    {
        public Vector2i position = new(x, y);
        public uint icon = _icon; //the one to be printed in console
        public Color Color = color;

    }
}