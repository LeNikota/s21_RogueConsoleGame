using data;
using domain.generation;

namespace domain;

enum InputAction { NONE, UP, DOWN, LEFT, RIGHT, ESCAPE, ENTER, BACKPACK, DROP, WEAPON, FOOD, ELIXIR, SCROLL, RESTART, NEXT };

class GameSession
{
    public enum State { PLAYING, OVER };
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
    // Make this out of 2D part of the code
    public bool Is3DMode = false;

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

            //! TODO: MODIFY
            // Make this out of 2D part of the code
            if (Is3DMode)
                return MovePlayer3D(action);

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

    private bool MovePlayer3D(InputAction action)
    {
        Player player = Level.Player;

        // Constants for smooth movement
        const double MOVE_SPEED = 0.2;
        const double ROT_SPEED = 0.05;

        double dx = 0;
        double dy = 0;
        double dAngle = 0;

        // Calculate intended change based on input
        switch (action)
        {
            case InputAction.UP:
                dx = Math.Cos(player.Angle) * MOVE_SPEED;
                dy = Math.Sin(player.Angle) * MOVE_SPEED;
                break;
            case InputAction.DOWN:
                dx = -Math.Cos(player.Angle) * MOVE_SPEED;
                dy = -Math.Sin(player.Angle) * MOVE_SPEED;
                break;
            case InputAction.LEFT:
                dAngle = -ROT_SPEED;
                break;
            case InputAction.RIGHT:
                dAngle = ROT_SPEED;
                break;
            default:
                return false;
        }

        // Handle Rotation
        if (dAngle != 0)
        {
            player.Angle += dAngle;
            // Normalize angle to keep it between 0 and 2*PI
            if (player.Angle > 2 * Math.PI) player.Angle -= 2 * Math.PI;
            if (player.Angle < 0) player.Angle += 2 * Math.PI;

            //! TODO: MODIFY ENEMY'S TURN AND ITEM INTERACTIONS 
            return false;
            return true;
        }

        Vector2d currentPos = player.PositionDouble;
        Vector2d newPos = new(currentPos.X + dx, currentPos.Y + dy);

        // If there is no collision check here, the player can noclip
        if (!Level.IsWallCollision(newPos.X, newPos.Y))
        {
            player.PositionDouble = newPos;
        }
        // Separate check for wall sliding
        // if (!Level.IsWallCollision(newPos.X, currentPos.Y))
        // {
        //     player.PositionDouble.X = newPos.X;
        // }

        // if (!Level.IsWallCollision(currentPos.X, newPos.Y))
        // {
        //     player.PositionDouble.Y = newPos.Y;
        // }

        //! TODO: DELETE OR KEEP
        level.UpdateVisibility();

        //! TODO: MODIFY ENEMY'S TURN AND ITEM INTERACTIONS
        return false;
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

