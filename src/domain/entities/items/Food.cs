namespace domain;

class Food : Item
{
    static readonly string[] names = ["Apple", "Banana", "Salmon", "Potato", "Cabbage", "Beef", "Pork", "Chicken"];

    public int Health { get; set; }

    public Food() : base() { }
    public Food(int x, int y, (string _name, int hp) FData)
        : base(x, y, 'F', FData._name)
    {
        Health = FData.hp;
    }

    public Food(int x = 0, int y = 0) : this(x, y, GenerateFood()) { }

    private static (string _name, int hp) GenerateFood()
    {
        int name_id = Random.Shared.Next(names.Length);
        int health = Random.Shared.Next(10, 40);
        return ($"{names[name_id]} (+{health})", health);
    }
}