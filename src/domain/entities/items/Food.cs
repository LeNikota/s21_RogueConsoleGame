namespace domain
{
class Food(int x, int y, (string _name, int hp) FData) : Item(x, y, 'F', FData._name)
{
    static readonly string[] names = ["Apple", "Banana", "Salmon", "Potato", "Cabbage", "Beef", "Pork", "Chicken"];

    public readonly int Health = FData.hp;

    public Food(int x, int y) : this(x, y, GenerateFood()) {}

    private static (string _name, int hp) GenerateFood()
    {
        int name_id = Random.Shared.Next(names.Length);
        int health = Random.Shared.Next(10, 40);
        return ($"{names[name_id]} (+{health})", health);
    }
}
}