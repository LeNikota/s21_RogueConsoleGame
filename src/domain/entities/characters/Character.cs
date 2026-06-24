namespace domain
{

    class Character(int x, int y, uint _icon, int maxHealth, int strength, int agility, Color color = Color.None)  : Entity(x, y, _icon, color)
    {
        public enum Stat { Strength, Agility, MaxHealth}
        
        private Dictionary<Stat, int> _stats = new()
        {
            [Stat.Strength] = strength,
            [Stat.Agility] = agility,
            [Stat.MaxHealth] = maxHealth
        };
        private int _currentHealth = maxHealth;
        public int CurrentHealth
        {
            get => _currentHealth;
            protected set => _currentHealth = Math.Clamp(value, 0, MaxHealth);
        }

        public int MaxHealth => GetStat(Stat.MaxHealth);

        public int Strength => GetStat(Stat.Strength);
        public int Agility => GetStat(Stat.Agility);

        public int GetStat(Stat stat)
        {
            return _stats.TryGetValue(stat, out var value) ? value : 0;
        }

        public void SetStat(Stat stat, int newValue)
        {
            int old = _stats[stat];
            _stats[stat] = newValue;
            if (stat == Stat.MaxHealth)
            {
                _stats[stat] = Math.Max(1, _stats[stat]); //no death by changing max_health
                if (CurrentHealth == old) CurrentHealth = _stats[stat];
                else CurrentHealth = _currentHealth; 
            }
        }

        public void ModifyStat(Stat stat, int delta)
        {
            SetStat(stat, GetStat(stat) + delta);
        }

        public void Attack(Character other)
        {
            string my_name = this is Player ? "You" : GetType().Name;
            string other_name = other is Player ? "you" : other.GetType().Name;
            
            int roll = Random.Shared.Next(20);
            if (roll + Agility >= 10 + other.Agility)
            {
                other.TakeDamage(GetDamage());
                GameLog.Add($"{my_name} hit{(this is Player ? "" : "s")} {other_name}. ");
            } else
            {
                GameLog.Add($"{my_name} miss{(this is Player ? "" : "es")} {other_name}. ");
            }


        }

        public void TakeDamage(int damage)
        {
            CurrentHealth -= damage;
        }

        public bool IsDead()
        {
            return CurrentHealth <= 0;
        }

        public virtual int GetDamage()
        {
            return 5 * Strength; 
        }
    }
}