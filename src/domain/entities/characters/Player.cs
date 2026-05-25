
namespace domain
{
class Player(int x, int y) : Character(x, y, '@', 100, 5, 5)
{

    readonly Backpack backpack = new();
    readonly List<Elixir> ActiveElixirs = [];
    Weapon? CurrentWeapon = null;

    public bool PickItem(Item item)
    {
        return backpack.Add(item);
    }
    public Backpack GetBackpack() { return backpack; }
    public void RemoveItem(Item item) => backpack.Remove(item);

    public override int GetDamage() 
    {
        int weapon_damage = CurrentWeapon == null ? 0 : CurrentWeapon.Strength;
        return base.GetDamage() + weapon_damage;
    }

    public Weapon? GetWeapon() {return CurrentWeapon;}

    public bool SetWeapon(int id, Level level)
    {
        if (id < 0)
        {
            if (CurrentWeapon == null) return false;
            if (!backpack.Add(CurrentWeapon)) return false;
            CurrentWeapon = null;
            return true;
        }
        
        if (backpack[id] is not Weapon) return false;
        if (CurrentWeapon != null && !backpack.Add(CurrentWeapon))
        {
            CurrentWeapon.position = position;
            level.entities.Add(CurrentWeapon);
        }
        CurrentWeapon = (Weapon)backpack[id];
        backpack.Remove(CurrentWeapon);
        return true;
    }

    public bool Eat(int id)
    {
        if (backpack[id] is not Food) return false;
        Food food = (Food)backpack[id];
        CurrentHealth = Math.Min(MaxHealth, CurrentHealth + food.Health);
        backpack.Remove(food);
        return true;
    }

    public bool Read(int id)
    {
        if (backpack[id] is not Scroll) return false;
        Scroll scroll = (Scroll)backpack[id];
        
        ModifyStat(scroll.Stat, scroll.Power);
        backpack.Remove(scroll);
        return true;
    }

    public bool Drink(int id)
    {
        if (backpack[id] is not Elixir) return false;
        Elixir elixir = (Elixir)backpack[id];
        elixir.TurnsLeft += 1; //to mitigate immediate decrease
        
        ModifyStat(elixir.Stat, elixir.Power);
        ActiveElixirs.Add(elixir);
        backpack.Remove(elixir);
        
        return true;
    }

    public void OnTurnEnded()
    {
        List<Elixir> ToRemove = [];
        foreach(var elixir in ActiveElixirs)
        {
            elixir.TurnsLeft -= 1;
            if (elixir.TurnsLeft == 0)
            {
                ToRemove.Add(elixir);
                ModifyStat(elixir.Stat, -elixir.Power);
            }
        }
        foreach(var elixir in ToRemove) ActiveElixirs.Remove(elixir);
    }
}
}