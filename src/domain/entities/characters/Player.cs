namespace domain
{
class Player(int x, int y) : Character(x, y, '@', 100, 5, 5)
{

    public override int GetDamage() 
    {
        return base.GetDamage() + 50; //50 represent weapon modifier for now
    }
}
}