namespace domain
{
class Weapon(int x, int y, (string _name, int strength) WData) : Item(x, y, '!', WData._name)
{
    static readonly string[] names = ["Sword", "Spear", "Mace", "Axe", "Hammer", "Dagger", "Club", "Flail", "Whip", "Glaive"];

    public readonly int strength = WData.strength;

    public Weapon(int x, int y) : this(x, y, GenerateWeapon()) {}

    private static (string _name, int strength) GenerateWeapon()
    {
        int name_id = Random.Shared.Next(names.Length);
        return (names[name_id], Random.Shared.Next(1, 5));
    }
}
}