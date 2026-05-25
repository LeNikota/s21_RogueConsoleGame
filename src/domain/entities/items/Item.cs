namespace domain
{
class Item(int x, int y, uint _icon, string _name) : Entity(x, y, _icon)
{
    public readonly string name = _name; //to print in the inventory

    public static Item GetRandomItem()
    {
        return Random.Shared.Next(0, 3) switch
        {
            0 => new Weapon(),
            1 => new Food(),
            2 => new Scroll(),
            3 => new Elixir(),
            _ => new Food(),
        };
    }
}
}