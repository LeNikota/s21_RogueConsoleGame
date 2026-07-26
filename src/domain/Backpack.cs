namespace domain;

class Backpack
{
    public enum Action { NONE, DROP, WEAPON, FOOD, ELIXIR, SCROLL }
    public const int MAX_SIZE = 9;

    public Action action = Action.NONE;
    public bool Opened = false;

    // Changed from private readonly field to public property for serialization
    // We use a private setter to maintain control via Add/Remove methods if needed, 
    // but allow the serializer to set it during deserialization.
    public List<Item> Items { get; set; } = [];

    public int Treasure { get; set; } = 0;

    public int Count => Items.Count;

    public Item this[int id]
    {
        get => Items[id];
        set => Items[id] = value;
    }

    public bool Add(Item item)
    {
        if (Items.Count == MAX_SIZE) return false;
        Items.Add(item);
        return true;
    }

    public void AddTreasure(int _value) { Treasure += _value; }

    public void Remove(Item i) => Items.Remove(i);

    public IEnumerator<Item> GetEnumerator()
    {
        return Items.GetEnumerator();
    }

    public void SetAction(Action a)
    {
        action = a;
        switch (action)
        {
            case Action.DROP:
                GameLog.Log("Which item to drop?");
                break;
            case Action.WEAPON:
                GameLog.Log("Which weapon to equip?");
                break;
            case Action.FOOD:
                GameLog.Log("Which food to eat?");
                break;
            case Action.ELIXIR:
                GameLog.Log("Which elixir to drink?");
                break;
            case Action.SCROLL:
                GameLog.Log("Which scroll to read?");
                break;
            default: break;
        }
    }

    public bool OnAction(int? backpack_id, Player Player, Level level)
    {
        if (backpack_id == null || !Opened) return false;

        int id = (int)backpack_id - 1;
        if (id >= MAX_SIZE) return false;

        if (id < 0)
        {
            if (action != Action.WEAPON) return false;
            if (Player.SetWeapon(id, level))
            {
                GameLog.Add($"Weapon is unequipped.");
                return true;
            }
            else
            {
                GameLog.Add($"No weapon to unequip or no space in the backpack.");
                return false;
            }
        }

        if (action == Action.NONE)
        {
            switch (Player.GetBackpack()[id])
            {
                case Weapon w:
                    Player.SetWeapon(id, level);
                    GameLog.Add($"{w.Name} is equipped.");
                    break;
                case Food f:
                    Player.Eat(id);
                    GameLog.Add($"You eat the {f.Name}.");
                    break;
                case Elixir e:
                    Player.Drink(id);
                    GameLog.Add($"You drink the {e.Name}.");
                    break;
                case Scroll s:
                    Player.Read(id);
                    GameLog.Add($"You read the {s.Name}.");
                    break;
                default:
                    GameLog.Add($"What can you do with this?");
                    return false;
            }
            return true;
        }

        string n = Player.GetBackpack()[id].Name;
        switch (action)
        {
            case Action.DROP:
                Item item = Player.GetBackpack()[id];
                item.Position = Player.Position;
                level.Entities.Add(item);
                Player.RemoveItem(item);
                break;
            case Action.WEAPON:
                if (Player.SetWeapon(id, level))
                {
                    GameLog.Add($"{n} is equipped.");
                }
                else
                {
                    GameLog.Add($"{n} is not a weapon.");
                    return false;
                }
                break;
            case Action.FOOD:
                if (Player.Eat(id))
                {
                    GameLog.Add($"You eat the {n}.");
                }
                else
                {
                    GameLog.Add($"{n} is not a food.");
                    return false;
                }
                break;
            case Action.ELIXIR:
                if (Player.Drink(id))
                {
                    GameLog.Add($"You drink the {n}.");
                }
                else
                {
                    GameLog.Add($"{n} is not an elixir.");
                    return false;
                }
                break;
            case Action.SCROLL:
                if (Player.Read(id))
                {
                    GameLog.Add($"You read the {n}.");
                }
                else
                {
                    GameLog.Add($"{n} is not a scroll.");
                    return false;
                }
                break;
            default: break;
        }

        return true;
    }
}
