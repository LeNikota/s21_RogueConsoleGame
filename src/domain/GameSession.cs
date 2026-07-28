using data;
using domain.generation;

namespace domain;

enum InputAction { NONE, UP, DOWN, LEFT, RIGHT, ESCAPE, ENTER, BACKPACK, DROP, WEAPON, FOOD, ELIXIR, SCROLL, RESTART, NEXT };

class GameSession
{
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

    private Level level;
    public Level Level => level;
    public Player Player => level.Player;
    public State state = State.PLAYING;
    public GameStatistics Stats { get; private set; }

    public event EventHandler<GameSaveEventArgs>? OnSaveRequested;

    public GameSession(GameStatistics stats)
    {
        Stats = stats;
        level = new Level(1, new Player());
        state = State.PLAYING;
    }

    public GameSession(GameStatistics stats, Level level)
    {
        Stats = stats;
        this.level = level;
        state = State.PLAYING;
    }

    protected virtual void RaiseSaveEvent()
    {
        OnSaveRequested?.Invoke(this, new GameSaveEventArgs(Stats, level));
    }

    public void RequestSave()
    {
        RaiseSaveEvent();
    }

    public void Input(InputAction action, int? backpack_id = null)
    {
        switch (state)
        {
            case State.PLAYING:
                GameLog.Clear();
                if (!OnGameRunningInput(action, backpack_id)) return;

                OnEnemyTurn();
                Player.OnTurnEnded();
                break;

            case State.OVER:
                OnGameOverInput(action);
                break;
        }
    }

    private bool OnGameRunningInput(InputAction action, int? backpack_id = null)
    {
        // Check if Player is Asleep
        if (Player.IsAsleep)
        {
            GameLog.Log("You are asleep and cannot move!");
            // Still process enemy turns
            OnEnemyTurn();
            Player.OnTurnEnded();
            return false;
        }

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
            // Task 2: The number of treasures depends on the difficulty of the enemy and the floor
            int baseTreasure = Random.Shared.Next(100, 1000);
            int floorBonus = Level.Floor * 50; // +50 gold for each floor
            int treasure = baseTreasure + floorBonus;


            Stats.TreasuresCollected += treasure;
            Stats.EnemiesDefeated++;

            level.Entities.Remove(enemy);

            GameLog.Add($"{enemy.GetType().Name} dies! You get treasure worth {treasure}");
        }
    }

    private void OnItemTouched(Item item)
    {
        if (Player.PickItem(item))
        {
            GameLog.Add($"You pick up a {item.Name}");
            level.Entities.Remove(item);
        }
        else
        {
            GameLog.Add($"Your backpack is full");
        }
    }

    private void OnEnemyTurn()
    {
        int startHealth = Player.CurrentHealth;

        foreach (Enemy enemy in level.Entities.FindAll(e => e is Enemy).Cast<Enemy>())
        {
            enemy.Act(Player, level, Stats);
            if (Player.IsDead())
            {
                state = State.OVER;
                GameLog.Add("You die. Press ESC to exit");
                return;
            }
        }

        int damageTaken = startHealth - Player.CurrentHealth;
        if (damageTaken > 0)
        {
            Stats.DamageTakenLastLevel += damageTaken;
        }
    }

    public void OnLevelExited()
    {
        int nextLevel = level.Floor + 1;

        if (nextLevel > Stats.HighestLevelReached)
            Stats.HighestLevelReached = level.Floor;

        if (nextLevel > 21)
        {
            state = State.OVER;
            GameLog.Add("You won. Press ESC to exit");
            return;
        }

        level = new Level(nextLevel, Player, Stats);

        Stats.ResetLevelStats();
        RaiseSaveEvent();
    }
}

