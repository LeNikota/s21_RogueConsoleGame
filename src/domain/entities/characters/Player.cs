using System.Text.Json.Serialization;

namespace domain
{
    [JsonDerivedType(typeof(Player))]
    class Player : Character
    {
        public Backpack Backpack { get; set; } = new();
        public List<Elixir> ActiveElixirs { get; set; } = [];
        public Weapon? CurrentWeapon { get; set; } = null;

        public Player() : base(0, 0, '@', 100, 5, 5) { }

        public Player(int x = 0, int y = 0, GameStatistics? stats = null)
            : base(x, y, '@', 100, 5, 5) {}

        public bool PickItem(Item item)
        {
            return Backpack.Add(item);
        }

        public Backpack GetBackpack() { return Backpack; }

        public void RemoveItem(Item item) => Backpack.Remove(item);

        public override int GetDamage()
        {
            int weapon_damage = CurrentWeapon == null ? 0 : CurrentWeapon.Strength;
            return base.GetDamage() + weapon_damage;
        }

        public Weapon? GetWeapon() { return CurrentWeapon; }

        public bool SetWeapon(int id, Level level)
        {
            if (id < 0)
            {
                if (CurrentWeapon == null) return false;
                if (!Backpack.Add(CurrentWeapon)) return false;
                CurrentWeapon = null;
                return true;
            }

            if (Backpack[id] is not Weapon) return false;
            if (CurrentWeapon != null && !Backpack.Add(CurrentWeapon))
            {
                CurrentWeapon.Position = Position;
                level.Entities.Add(CurrentWeapon);
            }
            CurrentWeapon = (Weapon)Backpack[id];
            Backpack.Remove(CurrentWeapon);
            return true;
        }

        public bool Eat(int id)
        {
            if (Backpack[id] is not Food) return false;

            Food food = (Food)Backpack[id];
            CurrentHealth = Math.Min(MaxHealth, CurrentHealth + food.Health);
            Backpack.Remove(food);
            
            return true;
        }

        public bool Read(int id)
        {
            if (Backpack[id] is not Scroll) return false;

            Scroll scroll = (Scroll)Backpack[id];
            ModifyStat(scroll.Stat, scroll.Power);
            Backpack.Remove(scroll);

            return true;
        }

        public bool Drink(int id)
        {
            if (Backpack[id] is not Elixir) return false;

            Elixir elixir = (Elixir)Backpack[id];
            elixir.TurnsLeft += 1; //to mitigate immediate decrease

            ModifyStat(elixir.Stat, elixir.Power);
            ActiveElixirs.Add(elixir);
            Backpack.Remove(elixir);

            return true;
        }

        public void OnTurnEnded()
        {
            List<Elixir> ToRemove = [];
            foreach (var elixir in ActiveElixirs)
            {
                elixir.TurnsLeft -= 1;
                if (elixir.TurnsLeft == 0)
                {
                    ToRemove.Add(elixir);
                    ModifyStat(elixir.Stat, -elixir.Power);
                }
            }
            foreach (var elixir in ToRemove) ActiveElixirs.Remove(elixir);
        }
    }
}