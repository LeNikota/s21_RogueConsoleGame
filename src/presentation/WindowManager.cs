// presentation/WindowManager.cs
using Mindmagma.Curses;
using domain;

namespace presentation;

class WindowManager
{
    private Window? _currentWindow;
    private readonly Stack<Window> _windowStack = new();
    // Should close the game
    public bool ShouldClose { get; set; } = false;

    public void SetStartWindow(Window window)
    {
        _currentWindow = window;
        _windowStack.Clear();
    }

    public void PushWindow(Window window)
    {
        if (_currentWindow != null)
            _windowStack.Push(_currentWindow);
        _currentWindow = window;
    }

    public void PopWindow()
    {
        if (_windowStack.Count > 0)
            _currentWindow = _windowStack.Pop();
        else
            ShouldClose = true;
    }

    public void SwitchTo(Window window)
    {
        _currentWindow = window;
        _windowStack.Clear();
    }

    public void Clear()
    {
        _windowStack.Clear();
    }


    public void Render()
    {
        _currentWindow?.Render();
        NCurses.Refresh();
    }

    public void HandleInput(int key)
    {
        if (_currentWindow == null) return;

        InputAction action = (key >= '0' && key <= '9') ?
            (InputAction)((int)InputAction.ZERO + (key - '0')) :
            MapKeyToAction(key);

        // Global Escape handling for popping menus
        if (action == InputAction.ESCAPE && _windowStack.Count > 0)
        {
            PopWindow();
            return;
        }

        _currentWindow.Input(action);
    }

    static private InputAction MapKeyToAction(int key)
    {
        return key switch
        {
            'w' or 'W' or CursesKey.UP => InputAction.UP,
            's' or 'S' or CursesKey.DOWN => InputAction.DOWN,
            'a' or 'A' or CursesKey.LEFT => InputAction.LEFT,
            'd' or 'D' or CursesKey.RIGHT => InputAction.RIGHT,
            CursesKey.ESC => InputAction.ESCAPE,
            CursesKey.ENTER or '\r' or '\n' => InputAction.ENTER,
            'i' or 'I' or '\t' => InputAction.BACKPACK,
            'h' or 'H' => InputAction.WEAPON,
            'j' or 'J' => InputAction.FOOD,
            'k' or 'K' => InputAction.ELIXIR,
            'e' or 'E' => InputAction.SCROLL,
            't' or 'T' => InputAction.DROP,
            'r' or 'R' => InputAction.RESTART,
            'n' or 'N' => InputAction.NEXT,
            _ => InputAction.NONE,
        };
    }
}