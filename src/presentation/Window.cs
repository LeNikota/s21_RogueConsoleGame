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
        _actions[input] = action;
    }

    public virtual void Render()
    {
        NCurses.Clear();
        
        int centerX = Level.WIDTH / 2;
        int centerY = Level.HEIGHT / 2;
        
        string label = string.IsNullOrEmpty(Title) ? "Nothing to render" : Title;
        int x = centerX - label.Length / 2;

        NCurses.MoveAddString(centerY, x, label);
    }
}