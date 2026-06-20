namespace domain
{
class Item(int x, int y, uint _icon, string _name, Color color = Color.None) : Entity(x, y, _icon, color)
{
    public readonly string name = _name; //to print in the inventory

    public static Item GetRandomItem(Vector2i? pos = null)
    {
        return Random.Shared.Next(0, 3) switch
        {
            0 => new Weapon(pos is null ? 0 : pos.X, pos is null ? 0 : pos.Y),
            1 => new Food(pos is null ? 0 : pos.X, pos is null ? 0 : pos.Y),
            2 => new Scroll(pos is null ? 0 : pos.X, pos is null ? 0 : pos.Y),
            3 => new Elixir(pos is null ? 0 : pos.X, pos is null ? 0 : pos.Y),
            _ => new Food(pos is null ? 0 : pos.X, pos is null ? 0 : pos.Y),
        };
    }
}
}