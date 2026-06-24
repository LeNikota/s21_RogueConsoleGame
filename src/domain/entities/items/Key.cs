namespace domain
{
    class Key(int x, int y, Color color = Color.None) : Item(x, y, '*', $"Key ({color})")
    {
        public override string ToString()
        {
            return $"{color} key";
        }
    }
}