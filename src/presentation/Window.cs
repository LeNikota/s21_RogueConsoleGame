using Mindmagma.Curses;
using domain;

namespace presentation;

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

    // Adds a key press handler
    public void AddInput(InputAction input, Action action)
    {
        _actions[input] = action; // Use indexer to allow overwriting
    }

    public virtual void Render()
    {
        int centerX = Level.WIDTH / 2;
        int centerY = Level.HEIGHT / 2;
        string label = "Nothing to render";
        int x = centerX - label.Length / 2;
        
        NCurses.Clear();
        NCurses.MoveAddString(centerY, x, label);
    }
}