namespace domain;

class Weapon : Item
{
    static readonly string[] names = ["Sword", "Spear", "Mace", "Axe", "Hammer", "Dagger", "Club", "Flail", "Whip", "Glaive"];

    public int Strength { get; set; }

    public Weapon() : base() { }

    public Weapon(int x, int y, (string _name, int _strength) WData)
        : base(x, y, '!', WData._name)
    {
        Strength = WData._strength;
    }

    public Weapon(int x = 0, int y = 0) : this(x, y, GenerateWeapon()) { }

    private static (string _name, int _strength) GenerateWeapon()
    {
        int strength = Random.Shared.Next(5, 25);
        return ($"{names[Random.Shared.Next(names.Length)]} (+{strength})", strength);
    }
}