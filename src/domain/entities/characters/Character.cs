namespace domain
{
class Character(int x, int y, uint _icon, int h, int s, int a) : Entity(x, y, _icon)
{
    public int MaxHealth = h;
    public int CurrentHealth = h;
    public int Strength = s;
    public int Agility = a;

    public bool Hit(int damage)
    {
        CurrentHealth -= damage;
        return CurrentHealth <= 0;
    }

    public virtual int GetDamage() 
    {
        return 5 * Strength;
    }
}
}