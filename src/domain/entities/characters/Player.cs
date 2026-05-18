namespace domain
{
class Player(int x, int y) : Character(x, y, '@', 100, 5, 5)
{

    List<Item> Backpack = [];
    //int weapon_idx;

    public bool PickItem(Item item)
    {
        if (Backpack.Count == 9) return false;
        Backpack.Add(item);
        return true;
    }

    public override int GetDamage() 
    {
        return base.GetDamage() + 50; //50 represent weapon modifier for now
    }
}
}