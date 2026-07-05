namespace domain;

class Window(string title = "Window", Dictionary<InputAction, Action>? actions = null)
{
    public string Title { get; } = title;
    protected readonly Dictionary<InputAction, Action> _actions = actions ?? [];

    public virtual void Input(InputAction input)
    {
        if (_actions.TryGetValue(input, out Action? action))
        {
            action?.Invoke();
        }
    }

    public void AddInput(InputAction input, Action action)
    {
        _actions[input] = action; // Use indexer to allow overwriting
    }
}