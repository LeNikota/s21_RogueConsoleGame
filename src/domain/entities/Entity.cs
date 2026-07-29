using System.Text.Json.Serialization;

namespace domain;

enum Color { None, White, Red, Green, Blue, Yellow, Cyan, Magenta, Black }


[JsonDerivedType(typeof(Player), "player")]
[JsonDerivedType(typeof(Enemy), "enemy")]
[JsonDerivedType(typeof(Zombie), "zombie")]
[JsonDerivedType(typeof(Vampire), "vampire")]
[JsonDerivedType(typeof(Ghost), "ghost")]
[JsonDerivedType(typeof(Ogre), "ogre")]
[JsonDerivedType(typeof(SnakeMage), "snakeMage")]
[JsonDerivedType(typeof(Mimic), "mimic")]
[JsonDerivedType(typeof(Weapon), "weapon")]
[JsonDerivedType(typeof(Food), "food")]
[JsonDerivedType(typeof(Scroll), "scroll")]
[JsonDerivedType(typeof(Elixir), "elixir")]
[JsonDerivedType(typeof(Key), "key")]

[JsonDerivedType(typeof(Character), "character")]
[JsonDerivedType(typeof(Item), "item")]
[JsonDerivedType(typeof(Door), "door")]

class Entity
{
    public virtual Vector2i Position { get; set; }
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
