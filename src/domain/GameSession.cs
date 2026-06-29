using data;

namespace domain;

class GameSession
{
    public enum InputAction { NONE, UP, DOWN, LEFT, RIGHT, ESCAPE, ENTER, BACKPACK, DROP, WEAPON, FOOD, ELIXIR, SCROLL, RESTART };
    public enum State { MENU, PLAYING, OVER, EXITING, PAUSED, STATS };
    public readonly Dictionary<InputAction, Vector2i> InputDirectionMap = new()
        {
            {InputAction.UP, Vector2i.UP},
            {InputAction.DOWN, Vector2i.DOWN},
            {InputAction.LEFT, Vector2i.LEFT},
            {InputAction.RIGHT, Vector2i.RIGHT}
        };

    public readonly Dictionary<InputAction, Backpack.Action> InputBackpackMap = new()
        {
            {InputAction.DROP, Backpack.Action.DROP},
            {InputAction.WEAPON, Backpack.Action.WEAPON},
            {InputAction.FOOD, Backpack.Action.FOOD},
            {InputAction.ELIXIR, Backpack.Action.ELIXIR},
            {InputAction.SCROLL, Backpack.Action.SCROLL}
        };

    private Level? level;
    public Level? Level => level;
    public Player Player => level.Player;
    public State state = State.PLAYING;
    public GameStatistics Stats { get; private set; } = new GameStatistics();

    private Menu _mainMenu;
    private Menu _pauseMenu;
    public Menu? CurrentMenu
    {
        get
        {
            return state switch
            {
                State.MENU => _mainMenu,
                State.PAUSED => _pauseMenu,
                _ => null
            };
        }
    }

    private readonly IDataService dataService;
    private SaveData? lastSaveData;
    public List<GameStatistics> GetHighScores() => lastSaveData?.HighScores ?? [];

    public GameSession(IDataService dataService)
    {
        this.dataService = dataService;
        state = State.MENU;
        level = null;

        _mainMenu = new Menu(
            "==== S21 ROGUE GAME ====",
            [
                ("Start New Game", () => {
                        level = new Level(1, new Player());
                        state = State.PLAYING;
                        GameLog.Log("Welcome to s21_rogue! Use WASD to move.");
                    }),
                    ("View Statistics", () => {
                        state = State.STATS;
                    }),
                    ("Exit", () => {
                        state = State.EXITING;
                    })
            ]
        );

        _pauseMenu = new Menu(
            "PAUSED",
            [
                ("Resume", () => {
                        state = State.PLAYING;
                    }),
                    ("Restart", () => {
                        state = State.PLAYING;
                        level = new Level(1, new Player());
                    }),
                    ("View Statistics", () => {
                        state = State.STATS;
                    }),
                    ("Main Menu", () => {
                        state = State.MENU;
                        level = null;
                        _mainMenu.Reset();
                    })
            ]
        );
    }

    public void Input(InputAction action, int? backpack_id = null)
    {
        switch (state)
        {
            case State.MENU:
            case State.PAUSED:
                CurrentMenu?.HandleInput(action);
                break;
            case State.STATS:
                // Any key exits stats view and returns to previous state
                // if level is null (Menu) or not (Paused)
                if (level == null) state = State.MENU;
                else state = State.PAUSED;
                break;
            case State.PLAYING:
                GameLog.Clear();
                if (!OnGameRunningInput(action, backpack_id)) return;
                // getting here only if a valid player's turn is made:
                OnEnemyTurn();
                // wearing down active elixirs
                Player.OnTurnEnded();
                break;
            case State.OVER:
                OnGameOverInput(action);
                break;
        }
    }

    private bool OnGameRunningInput(InputAction action, int? backpack_id = null)
    {
        Backpack backpack = Player.GetBackpack();
        if (InputDirectionMap.TryGetValue(action, out Vector2i? dir))
        {
            backpack.Opened = false;
            Vector2i new_pos = Player.Position + dir;

            return MovePlayer(new_pos);
        }
        else if (InputBackpackMap.TryGetValue(action, out Backpack.Action a))
        {
            if (backpack.Opened && a == backpack.action)
            {
                backpack.Opened = false;
                backpack.action = Backpack.Action.NONE;
                return false;
            }
            backpack.SetAction(a);
            backpack.Opened = true;
            return false;
        }
        else if (action == InputAction.ESCAPE)
        {
            if (backpack.Opened)
            {
                backpack.Opened = false;
                return false;
            }

            state = State.PAUSED;

            return false;
        }
        else if (action == InputAction.BACKPACK)
        {
            backpack.Opened = !backpack.Opened;
            backpack.action = Backpack.Action.NONE;
            if (backpack.Opened) GameLog.Log("Your backpack:");
            return true;
        }
        else if (action == InputAction.NONE)
            return backpack.OnAction(backpack_id, Player, level);

        return true;
    }

    private void OnGameOverInput(InputAction action)
    {
        if (action == InputAction.ESCAPE)
        {
            state = State.EXITING;
        }
        else if (action == InputAction.RESTART)
        {
            state = State.MENU;
            level = null;
        }
    }

    private bool MovePlayer(Vector2i new_pos)
    {
        dataService.SaveAsync(this);

        if (!level.IsPositionValid(new_pos)) return false;
        Entity? entity = level.GetEntityByPos(new_pos);

        if (Player.Position != new_pos)
            Stats.TilesTraversed++;

        if (entity == null)
            Player.Position = new_pos;
        else if (entity is Enemy enemy)
            OnEnemyTouched(enemy);
        else if (entity is Item item)
        {
            Player.Position = new_pos;
            OnItemTouched(item);
        }
        else if (entity is Door door)
        {
            if (door.IsExit)
            {
                OnLevelExited();
                return false;
            }
            if (door.Open(Player, level))
                level.Entities.Remove(door);
        }

        level.UpdateVisibility();

        return true;
    }

    private void OnEnemyTouched(Enemy enemy)
    {
        Stats.HitsDealt++;

        Player.Attack(enemy);
        if (enemy.IsDead())
        {
            int treasure = Random.Shared.Next(100, 1000);
            GameLog.Add($"{enemy.GetType().Name} dies! You get treasure worth {treasure}");
            level.Entities.Remove(enemy);
            Player.GetBackpack().AddTreasure(treasure);
        }
    }

    private void OnItemTouched(Item item)
    {
        if (Player.PickItem(item))
        {
            GameLog.Add($"You pick up a {item.name}");
            level.Entities.Remove(item);
        }
        else
        {
            GameLog.Add($"Your backpack is full");
        }
    }

    private void OnEnemyTurn()
    {
        foreach (Enemy enemy in level.Entities.FindAll(e => e is Enemy).Cast<Enemy>())
        {
            if (enemy.Position.DistanceTo(Player.Position) > enemy.hostility)
            {
                enemy.MovePeacefully(level);
                continue;
            }
            List<Vector2i> path = PathFinder.FindPath(level, enemy.Position, Player.Position);
            if (path.Count == 0)
            {
                //no path to player
                enemy.MovePeacefully(level);
                continue;
            }
            if (path[0] == Player.Position)
            {
                enemy.Attack(Player);
                Stats.HitsTaken++;

                if (Player.IsDead())
                {
                    state = State.OVER;
                    GameLog.Add("You die. Press R to restart or ESC to exit");
                    return;
                }
            }
            else enemy.Position = path[0];
        }
    }

    public void OnLevelExited()
    {
        level = new Level(level.Floor + 1, Player);
        if (level.Floor > Stats.HighestLevelReached)
            Stats.HighestLevelReached = level.Floor;
    }

}

