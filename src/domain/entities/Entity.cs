namespace domain
{
    public enum Color { None, White, Red, Green, Blue, Yellow, Cyan, Magenta, Black }

    class Entity(int x, int y, uint _icon, Color color = Color.None)
    {
        public Vector2i Position {get; set;} = new(x, y);
        public uint Icon {get; set;} = _icon; //the one to be printed in console
        public Color Color {get; set;} = color;

    }
}