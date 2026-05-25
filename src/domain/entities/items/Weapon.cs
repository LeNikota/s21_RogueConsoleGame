namespace domain
{
class Weapon(int x, int y, (string _name, int _strength) WData) : Item(x, y, '!', WData._name)
{
    static readonly string[] names = ["Sword", "Spear", "Mace", "Axe", "Hammer", "Dagger", "Club", "Flail", "Whip", "Glaive"];

    public readonly int Strength = WData._strength;

    public Weapon(int x, int y) : this(x, y, GenerateWeapon()) {}

    private static (string _name, int _strength) GenerateWeapon()
    {
        int strength = Random.Shared.Next(5, 25);
        return ($"{names[Random.Shared.Next(names.Length)]} (+{strength})", strength);
    }
}
}