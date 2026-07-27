namespace domain;

class Zombie : Enemy
{
    public Zombie() : base() { }
    public Zombie(int x = 0, int y = 0) : base(x, y, 'z', StatFactor.HIGH, StatFactor.MEDIUM, StatFactor.LOW, StatFactor.MEDIUM, Color.Green) { }
}

