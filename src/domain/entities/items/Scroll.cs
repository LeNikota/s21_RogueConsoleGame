namespace domain
{
class Scroll(int x, int y, (string _name, Character.Stat _stat, int _power) SData) : Item(x, y, 'I', SData._name)
{
    public readonly Character.Stat Stat = SData._stat;
    public readonly int Power = SData._power;

    public Scroll(int x, int y) : this(x, y, GenerateScroll()) {}

    private static (string _name, Character.Stat _stat, int _power) GenerateScroll()
    {
        Character.Stat[] stats = Enum.GetValues<Character.Stat>();
        Character.Stat stat = stats[Random.Shared.Next(stats.Length)];
  
        int power = Random.Shared.Next(1, 9);
        if (stat == Character.Stat.MaxHealth) power *= 5;

        return ($"Scroll of {stat} (+{power})", stat, power);
    }
}
}