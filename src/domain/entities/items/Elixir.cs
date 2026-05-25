namespace domain
{
class Elixir(int x, int y, (string _name, Character.Stat _stat, int _power, int _turns) EData) : Item(x, y, '&', EData._name)
{
    public readonly Character.Stat Stat = EData._stat;
    public readonly int Power = EData._power;
    public int TurnsLeft = EData._turns;

    public Elixir(int x, int y) : this(x, y, GenerateElixir()) {}

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
}