namespace domain
{
class Item(int x, int y, uint _icon, string _name) : Entity(x, y, _icon)
{
    public readonly string name = _name; //to print in the inventory
}
}