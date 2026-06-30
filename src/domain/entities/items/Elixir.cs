namespace domain;

class Elixir : Item
{
    public Character.Stat Stat { get; set; }
    public int Power { get; set; }
    public int TurnsLeft { get; set; }

    public Elixir() : base() { }
    public Elixir(int x, int y, (string _name, Character.Stat _stat, int _power, int _turns) EData)
        : base(x, y, '&', EData._name)
    {
        Stat = EData._stat;
        Power = EData._power;
        TurnsLeft = EData._turns;
    }

    public Elixir(int x = 0, int y = 0) : this(x, y, GenerateElixir()) { }

    private static (string _name, Character.Stat _stat, int _power, int _turns) GenerateElixir()
    {
        Character.Stat[] stats = Enum.GetValues<Character.Stat>();
        Character.Stat stat = stats[Random.Shared.Next(stats.Length)];
        int power = Random.Shared.Next(1, 9);
        int turns = Random.Shared.Next(5, 10);
        if (stat == Character.Stat.MaxHealth) power *= 5;
        return ($"Elixir of {stat} (+{power} per {turns} turns)", stat, power, turns);
    }
}