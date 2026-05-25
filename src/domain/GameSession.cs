using domain.generation;

namespace domain
{
    class GameSession 
    {
    public enum InputAction { NONE, UP, DOWN, LEFT, RIGHT, ESCAPE, BACKPACK, DROP, WEAPON, FOOD, ELIXIR, SCROLL};

    public enum State {PLAYING, OVER};
    public enum BackpackAction {NONE, DROP, WEAPON, FOOD, ELIXIR, SCROLL}
    public readonly Dictionary<InputAction, Vector2i> InputDirectionMap = new()
    {
        {InputAction.UP, Vector2i.UP},
        {InputAction.DOWN, Vector2i.DOWN},
        {InputAction.LEFT, Vector2i.LEFT},
        {InputAction.RIGHT, Vector2i.RIGHT}
    };

    public readonly Dictionary<InputAction, BackpackAction> InputBackpackMap = new()
    {
        {InputAction.DROP, BackpackAction.DROP},
        {InputAction.WEAPON, BackpackAction.WEAPON},
        {InputAction.FOOD, BackpackAction.FOOD},
        {InputAction.ELIXIR, BackpackAction.ELIXIR},
        {InputAction.SCROLL, BackpackAction.SCROLL}
    };

    private readonly Level level;
    private readonly Player player; //lookup not to look for it all the time

    public State state = State.PLAYING;
    public BackpackAction backpackAction = BackpackAction.NONE;
    public bool backpackOpened = false;
    public string GameLog = "Welcome to s21_rogue!";

    public GameSession()
    {
        level = new Level();
        player = new Player(8, 8);

        var rooms = LevelGenerator.GenerateRooms();
        var passages = LevelGenerator.GeneratePassages(rooms);
        LevelGenerator.PlacePlayer(rooms, player);
        level.is_wall = LevelGenerator.ConvertToArray(rooms, passages, Level.WIDTH, Level.HEIGHT);
        level.entities.Add(player);
    }

    public Level GetLevel()
    {
        return level;
    }

    public Player GetPlayer()
    {
        return player;
    }

    public void Input(InputAction action, int? backpack_id = null)
    {
        if (state == State.OVER) return;
        
        GameLog = "";
        if (InputDirectionMap.TryGetValue(action, out Vector2i? dir))
        {
            backpackOpened = false;
            Vector2i new_pos = player.position + dir;
            
            if (!MovePlayer(new_pos)) return;
        }   
        else if (InputBackpackMap.TryGetValue(action, out BackpackAction a))
        {
            if (backpackOpened && a == backpackAction)
            {
                backpackOpened = false;
                backpackAction = BackpackAction.NONE;
                return;
            }
            backpackAction = a;
            backpackOpened = true;
            OnBackpackActionChanged();
            return;
        }
        else if (action == InputAction.ESCAPE)
        {
            if (backpackOpened) backpackOpened = false;
            else state = State.OVER;
            return;     
        }
        else if (action == InputAction.BACKPACK)
        {
            backpackOpened = !backpackOpened;
            backpackAction = BackpackAction.NONE;
            if (backpackOpened) GameLog =  "Your backpack:";
            return;
        }
        else if (action == InputAction.NONE && !OnBackpackAction(backpack_id)) return;

        OnEnemyTurn(); //enemy turn (getting here only if a valid player's turn is made):
        player.OnTurnEnded(); //wearing down active elixirs
    }

    private bool MovePlayer(Vector2i new_pos)
    {
        if (!level.IsPositionValid(new_pos)) return false;
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
        //GameLog = player.position.ToString();
        return true;
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

    private void OnEnemyTurn()
    {
        foreach(Enemy enemy in level.entities.FindAll(e => e is Enemy).Cast<Enemy>())
        {
            if (state == State.OVER) return;
            
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
            state = State.OVER;
            GameLog += "You die. Game is over, press any key to exit.";
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

    private void OnBackpackActionChanged()
    {
        switch(backpackAction)
        {
            case BackpackAction.DROP:
                GameLog = "Which item to drop?";
                break;
            case BackpackAction.WEAPON:
                GameLog = "Which weapon to equip?";
                break;
            case BackpackAction.FOOD:
                GameLog = "Which food to eat?";
                break;
            case BackpackAction.ELIXIR:
                GameLog = "Which elixir to drink?";
                break;
            case BackpackAction.SCROLL:
                GameLog = "Which scroll to read?";
                break;
            default: break;
        }
    }

    private bool OnBackpackAction(int? backpack_id)
    {
        if (backpack_id == null) return false;
        
        int id = (int)backpack_id - 1; //-1 for random access
        if (id >= Backpack.MAX_SIZE) return false;

        if (id < 0)
        {
            //special case of weapon unequipping
            if (backpackAction != BackpackAction.WEAPON) return false;
            if (player.SetWeapon(id, level))
            {
                GameLog = $"Weapon is unequipped.";
                return true;
            } 
            else
            {
                GameLog = $"No weapon to unequip or no space in the backpack.";
                return false;
            }
        }

        if (backpackAction == BackpackAction.NONE)
        {
            //modern-friendly context-based use
            switch (player.GetBackpack()[id])
            {
                case Weapon w:
                    player.SetWeapon(id, level);
                    GameLog = $"{w.name} is equipped.";
                    break;
                case Food f:
                    player.Eat(id);
                    GameLog = $"You eat the {f.name}.";
                    break;
                case Elixir e:
                    player.Drink(id);
                    GameLog = $"You drink the {e.name}.";
                    break;
                case Scroll s:
                    player.Read(id);
                    GameLog = $"You read the {s.name}.";
                    break;
                default:
                    GameLog = $"What can you do with this?";
                    return false;
            }
            return true;
        }

        string n = player.GetBackpack()[id].name;
        //classic action-based use
        switch(backpackAction) 
        {
            case BackpackAction.DROP:
                Item item = player.GetBackpack()[id];
                item.position = player.position;
                level.entities.Add(item);
                player.RemoveItem(item);
                break;
            case BackpackAction.WEAPON:
                if (player.SetWeapon(id, level))
                {
                    GameLog = $"{n} is equipped.";
                }
                else
                {
                    GameLog = $"{n} is not a weapon.";
                    return false;
                }
                break;
            case BackpackAction.FOOD:
                if (player.Eat(id))
                {
                    GameLog = $"You eat the {n}.";
                }
                else
                {
                    GameLog = $"{n} is not a food.";
                    return false;
                }
                break;
            case BackpackAction.ELIXIR:
                if (player.Drink(id))
                {
                    GameLog = $"You drink the {n}.";
                }
                else
                {
                    GameLog = $"{n} is not an elixir.";
                    return false;
                }
                break;
            case BackpackAction.SCROLL:
                if (player.Read(id))
                {
                    GameLog = $"You read the {n}.";
                }
                else
                {
                    GameLog = $"{n} is not a scroll.";
                    return false;
                }
                break;
            default: break;
        }

        backpackAction = BackpackAction.NONE;
        return true;
    }

}

} //domain
