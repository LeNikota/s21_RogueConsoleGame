using System.Text.Json.Serialization;

namespace domain;

enum Color { None, White, Red, Green, Blue, Yellow, Cyan, Magenta, Black }


[JsonDerivedType(typeof(Player))]
[JsonDerivedType(typeof(Zombie))]
[JsonDerivedType(typeof(Vampire))]
[JsonDerivedType(typeof(Ghost))]
[JsonDerivedType(typeof(Ogre))]
[JsonDerivedType(typeof(SnakeMage))]
[JsonDerivedType(typeof(Weapon))]
[JsonDerivedType(typeof(Food))]
[JsonDerivedType(typeof(Scroll))]
[JsonDerivedType(typeof(Elixir))]
[JsonDerivedType(typeof(Key))]
[JsonDerivedType(typeof(Door))]
class Entity
{
    public Vector2i Position { get; set; }
    public uint Icon { get; set; }
    public Color Color { get; set; }

    public Entity() { }

    public Entity(int x, int y, uint icon, Color color = Color.None)
    {
        Position = new Vector2i(x, y);
        Icon = icon;
        Color = color;
    }
}
