namespace domain;

class Scroll : Item
{
    public Character.Stat Stat { get; set; }
    public int Power { get; set; }

    public Scroll() : base() { }
    public Scroll(int x, int y, (string _name, Character.Stat _stat, int _power) SData)
        : base(x, y, 'I', SData._name)
    {
        Stat = SData._stat;
        Power = SData._power;
    }

    public Scroll(int x = 0, int y = 0) : this(x, y, GenerateScroll()) { }

    private static (string _name, Character.Stat _stat, int _power) GenerateScroll()
    {
        Character.Stat[] stats = Enum.GetValues<Character.Stat>();
        Character.Stat stat = stats[Random.Shared.Next(stats.Length)];
        int power = Random.Shared.Next(1, 9);
        if (stat == Character.Stat.MaxHealth) power *= 5;
        return ($"Scroll of {stat} (+{power})", stat, power);
    }
}