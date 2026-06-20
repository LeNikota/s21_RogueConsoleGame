namespace domain
{
    public enum Color { None, Red, Blue, Yellow }

    class Entity(int x, int y, uint _icon)
    {
        public Vector2i position = new(x, y);
        public uint icon = _icon; //the one to be printed in console

    }
}