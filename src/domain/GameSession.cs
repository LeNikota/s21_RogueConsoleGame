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

    private Level level;
    public Player Player { get {return level.player;}}

    public State state = State.PLAYING;
    public BackpackAction backpackAction = BackpackAction.NONE;
    public bool backpackOpened = false;

    public GameSession()
    {
        level = new Level(1, new Player());
    }

    public Level GetLevel()
    {
        return level;
    }

    public void Input(InputAction action, int? backpack_id = null)
    {
        if (state == State.OVER) return;
        
        GameLog.Clear();
        if (InputDirectionMap.TryGetValue(action, out Vector2i? dir))
        {
            backpackOpened = false;
            Vector2i new_pos = Player.position + dir;
            
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
            if (backpackOpened) GameLog.Log("Your backpack:");
            return;
        }
        else if (action == InputAction.NONE && !OnBackpackAction(backpack_id)) return;

        OnEnemyTurn(); //enemy turn (getting here only if a valid player's turn is made):
        Player.OnTurnEnded(); //wearing down active elixirs
    }

    private bool MovePlayer(Vector2i new_pos)
    {
        if (!level.IsPositionValid(new_pos)) return false;
        Entity? entity = level.GetEntityByPos(new_pos);
        
        if (entity == null) 
            Player.position = new_pos;
        else if (entity is Enemy enemy) 
            OnEnemyTouched(enemy);
        else if (entity is Item item)
        {
            Player.position = new_pos;
            OnItemTouched(item);
        }
        else if (entity is Door door && door.IsExit)
        {
            OnLevelExited();
            return false;
        }
        //GameLog = player.position.ToString();
        return true;
    }

    private void OnEnemyTouched(Enemy enemy)
    {
        Player.Attack(enemy);    
        if (enemy.IsDead())
        {
            GameLog.Add($"{enemy.GetType().Name} dies!");
            level.entities.Remove(enemy);
            //TO DO: GIVE PLAYER A TREASURE here
            return;
        }
    }

    private void OnItemTouched(Item item)
    {
        if (Player.PickItem(item))
        {
            GameLog.Add($"You pick up a {item.name}");
            level.entities.Remove(item);
        }
        else
        {
            GameLog.Add($"Your backpack is full");
        }
    }

    private void OnEnemyTurn()
    {
        foreach(Enemy enemy in level.entities.FindAll(e => e is Enemy).Cast<Enemy>())
        {
            if (state == State.OVER) return;
            
            if (enemy.position.DistanceTo(Player.position) > enemy.hostility)
            {
                enemy.MovePeacefully(level);
                continue;
            }
            List<Vector2i> path = PathFinder.FindPath(level, enemy.position, Player.position);
            if (path.Count == 0)
            {
                //no path to player
                enemy.MovePeacefully(level);
                continue; 
            }
            if (path[0] == Player.position) OnEnemyTouchingPlayer(enemy);
            else enemy.position = path[0];
        }
    }

    private void OnEnemyTouchingPlayer(Enemy enemy)
    {
        enemy.Attack(Player);
        if (Player.IsDead())
        {
            state = State.OVER;
            GameLog.Add("You die. Game is over, press any key to exit."); 
        } 
    }

    private void OnBackpackActionChanged()
    {
        switch(backpackAction)
        {
            case BackpackAction.DROP:
                GameLog.Log("Which item to drop?");
                break;
            case BackpackAction.WEAPON:
                GameLog.Log("Which weapon to equip?");
                break;
            case BackpackAction.FOOD:
                GameLog.Log("Which food to eat?");
                break;
            case BackpackAction.ELIXIR:
                GameLog.Log("Which elixir to drink?");
                break;
            case BackpackAction.SCROLL:
                GameLog.Log("Which scroll to read?");
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

        if (backpackAction == BackpackAction.NONE)
        {
            //modern-friendly context-based use
            switch (Player.GetBackpack()[id])
            {
                case Weapon w:
                    Player.SetWeapon(id, level);
                    GameLog.Add($"{w.name} is equipped.");
                    break;
                case Food f:
                    Player.Eat(id);
                    GameLog.Add($"You eat the {f.name}.");
                    break;
                case Elixir e:
                    Player.Drink(id);
                    GameLog.Add($"You drink the {e.name}.");
                    break;
                case Scroll s:
                    Player.Read(id);
                    GameLog.Add($"You read the {s.name}.");
                    break;
                default:
                    GameLog.Add($"What can you do with this?");
                    return false;
            }
            return true;
        }

        string n = Player.GetBackpack()[id].name;
        //classic action-based use
        switch(backpackAction) 
        {
            case BackpackAction.DROP:
                Item item = Player.GetBackpack()[id];
                item.position = Player.position;
                level.entities.Add(item);
                Player.RemoveItem(item);
                break;
            case BackpackAction.WEAPON:
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
            case BackpackAction.FOOD:
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
            case BackpackAction.ELIXIR:
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
            case BackpackAction.SCROLL:
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

        //backpackAction = BackpackAction.NONE;
        return true;
    }

    public void OnLevelExited()
    {
        level = new Level(level.floor + 1, Player);
    }

}

} //domain
