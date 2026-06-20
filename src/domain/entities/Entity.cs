namespace domain
{
    public enum Color { None, Red, Blue, Yellow, Green, White }

    class Entity(int x, int y, uint _icon, Color color = Color.None)
    {
        public Vector2i position = new(x, y);
        public uint icon = _icon; //the one to be printed in console
        public Color Color = color;

    }
}