using System.Text.Json.Serialization;

namespace domain;

[JsonDerivedType(typeof(Weapon), "weapon")]
[JsonDerivedType(typeof(Food), "food")]
[JsonDerivedType(typeof(Scroll), "scroll")]
[JsonDerivedType(typeof(Elixir), "elixir")]
[JsonDerivedType(typeof(Key), "key")]

class Item : Entity
{
    public string Name { get; set; } = "";

    public Item() : base() { }

    public Item(int x, int y, uint _icon, string _name, Color color = Color.None)
        : base(x, y, _icon, color)
    {
        Name = _name;
    }

    public static Item GetRandomItem(Vector2i? pos = null)
    {
        int x = pos?.X ?? 0;
        int y = pos?.Y ?? 0;

        return Random.Shared.Next(0, 4) switch
        {
            0 => new Weapon(x, y),
            1 => new Food(x, y),
            2 => new Scroll(x, y),
            3 => new Elixir(x, y),
            _ => new Food(x, y),
        };
    }
}