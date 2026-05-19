

namespace domain {
class GameSession
{
    public enum InputAction { UP, DOWN, LEFT, RIGHT};
    public readonly Dictionary<InputAction, Vector2i> InputDirectionMap = new()
    {
        {InputAction.UP, Vector2i.UP},
        {InputAction.DOWN, Vector2i.DOWN},
        {InputAction.LEFT, Vector2i.LEFT},
        {InputAction.RIGHT, Vector2i.RIGHT}
    };

    private readonly Level level;
    private readonly Player player; //lookup not to look for it all the time

    public bool isOver = false;
    public string GameLog = "Welcome to s21_rogue!";

    public GameSession()
    {
        level = new Level();
        player = new Player(8, 8);
        level.entities.Add(player);
        level.entities.Add(new Zombie(8, 24));
        level.entities.Add(new Weapon(10, 10));
        level.entities.Add(new Food(12, 12));
    }

    public Level GetLevel()
    {
        return level;
    }

    public Player GetPlayer()
    {
        return player;
    }

    public void Input(InputAction action)
    {
        GameLog = "";
        if (action == InputAction.UP || action == InputAction.DOWN || action == InputAction.LEFT || action == InputAction.RIGHT)
        {
            Vector2i new_pos = player.position + InputDirectionMap[action];
            if (!level.IsPositionValid(new_pos)) return; //no movement nor enemey turn
            Entity? entity = level.GetEntityByPos(new_pos);
            
            if (entity == null) 
                player.position = new_pos;
            else if (entity is Enemy enemy) 
                OnEnemyTouched(enemy);
            else if (entity is Item item)
            {
                player.position = new_pos;
                OnItemTouched(item);
            }
            
        }
        //handle non-movement input here:

        //enemy turn:
        foreach(Enemy enemy in level.entities.FindAll(e => e is Enemy).Cast<Enemy>())
        {
            if (isOver) return;
            
            if (enemy.position.DistanceTo(player.position) > enemy.hostility)
            {
                enemy.MovePeacefully(level);
                continue;
            }
            List<Vector2i> path = PathFinder.FindPath(level, enemy.position, player.position);
            if (path.Count == 0)
            {
                //no path to player
                enemy.MovePeacefully(level);
                continue; 
            }
            if (path[0] == player.position) OnEnemyTouchingPlayer(enemy);
            else enemy.position = path[0];
        }

    }

    private void OnEnemyTouched(Enemy enemy)
    {
        string enemy_name = enemy.GetType().Name;
        bool isHit = Attack(player, enemy);
        if (isHit)
        {
            GameLog += $"You hit the {enemy_name}. ";
        }
        else
        {
            GameLog += $"You miss {enemy_name}. ";
        }
        
        if (enemy.IsDead())
        {
            GameLog += $"{enemy_name} dies!";
            level.entities.Remove(enemy);
            //TO DO: GIVE PLAYER A TREASURE here
            return;
        }
    }

    private void OnEnemyTouchingPlayer(Enemy enemy)
    {
        string enemy_name = enemy.GetType().Name;
        bool isHit = Attack(enemy, player);
        if (isHit)
        {
            GameLog += $"{enemy_name} hits you. ";
        } 
        else
        {
            GameLog += $"{enemy_name} misses you. ";
        }
        if (player.IsDead())
        {
            isOver = true;
            GameLog += "You die. Game is over, press any key to exit.";
        } 
    }

    private void OnItemTouched(Item item)
    {
        if (player.PickItem(item))
        {
            GameLog += $"You pick up a {item.name}";
            level.entities.Remove(item);
        }
        else
        {
            GameLog += $"Your backpack is full";
        }
    }

    private static bool Attack(Character c1, Character c2)
    {
        int roll = Random.Shared.Next(21);
        if (roll + c1.Agility >= 10 + c2.Agility)
        {
            c2.TakeDamage(c1.GetDamage());
            return true; //returns if hit
        }
        return false;
    }

}

} //domain
