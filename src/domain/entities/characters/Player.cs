using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using System.Text.Json.Serialization;
namespace domain
{
    [JsonDerivedType(typeof(Player))]
    class Player : Character
    {
        // ---- Warning ----
        // if _gridPosition.X or _gridPosition.Y updates
        // it won't update _worldPosition and vice versa
        // there needs to be a way to update both symoltanisly
        // like make _gridPosition source of truth for _worldPosition
        private Vector2i _gridPosition = new(0, 0);
        private Vector2d _worldPosition = new(0, 0);

        // new() in getters of Position and PositionDouble is used
        // to avoid mutating internal _gridPosition and _worldPosition

        // Grid Position (Used by Game Logic, 2D Rendering, Save/Load)
        public override Vector2i Position
        {
            get => new(_gridPosition.X, _gridPosition.Y);
            set
            {
                _gridPosition = value;
                _worldPosition.X = value.X;
                _worldPosition.Y = value.Y;
            }
        }

        // Smooth World Position (Used by 3D Rendering)
        public Vector2d PositionDouble
        {
            get
            {
                int wordPositionFlooredX = (int)Math.Floor(_worldPosition.X);
                int wordPositionFlooredY = (int)Math.Floor(_worldPosition.Y);
                int gridPositionX = _gridPosition.X;
                int gridPositionY = _gridPosition.Y;

                if (wordPositionFlooredX - gridPositionX != 0)
                    _worldPosition.X = gridPositionX;

                if (wordPositionFlooredY - gridPositionY != 0)
                    _worldPosition.Y = gridPositionY;

                return new(_worldPosition.X, _worldPosition.Y);
            }
            set
            {
                _worldPosition = value;
                _gridPosition.X = (int)Math.Floor(value.X);
                _gridPosition.Y = (int)Math.Floor(value.Y);

            }
        }

        // The direction the player is facing in 3D space
        public double Angle { get; set; } = 0; // In radians

        public bool IsAsleep { get; set; } = false;

        public Backpack Backpack { get; set; } = new();
        public List<Elixir> ActiveElixirs { get; set; } = [];
        public Weapon? CurrentWeapon { get; set; } = null;

        public Player() : base(0, 0, '@', 100, 5, 5)
        { }

        public Player(int x = 0, int y = 0, GameStatistics? stats = null) : base(x, y, '@', 100, 5, 5)
        {
            _gridPosition = new Vector2i(x, y);
            _worldPosition = new Vector2d(x, y);
        }


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
            elixir.TurnsLeft += 1;

            ModifyStat(elixir.Stat, elixir.Power);
            ActiveElixirs.Add(elixir);
            Backpack.Remove(elixir);

            return true;
        }

        public void OnTurnEnded()
        {
            if (IsAsleep)
            {
                IsAsleep = false;
                GameLog.Add("You wake up.");
            }

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

        public void StealMaxHealth(int amount)
        {
            if (amount <= 0) return;
            int oldMax = MaxHealth;
            SetStat(Stat.MaxHealth, Math.Max(1, MaxHealth - amount));

            // If current health was at max, reduce it too so they don't have more HP than max
            if (CurrentHealth >= oldMax)
            {
                CurrentHealth = MaxHealth;
            }
            else if (CurrentHealth > MaxHealth)
            {
                CurrentHealth = MaxHealth;
            }
        }

        public override void Attack(Character other)
        {
            if (IsAsleep)
                return;

            if (other is Vampire vampire)
            {
                if (!vampire.WasAttacked)
                {
                    vampire.WasAttacked = true;
                    GameLog.Add($"The Vampire dodges your first attack! `");
                    return;
                }
            }

            base.Attack(other);
        }
    }
}