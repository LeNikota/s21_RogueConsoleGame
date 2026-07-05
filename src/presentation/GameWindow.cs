namespace domain;

class GameWindow(string title = "Game", Dictionary<InputAction, Action>? actions = null) : Window(title, actions)
{
    public GameSession? GameSession { get; set; } = null;

    public override void Input(InputAction input)
    {
        if (_actions.TryGetValue(input, out Action? action))
        {
            action?.Invoke();
        }

        GameSession?.Input(input);
    }
}