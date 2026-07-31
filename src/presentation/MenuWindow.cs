using Mindmagma.Curses;
using domain;

namespace presentation;

class MenuWindow : Window
{
    private readonly List<(string Label, Action Callback, Func<bool>? IsEnabled)> _items;
    private List<(string Label, Action Callback, Func<bool>? IsEnabled)> _activeItems;
    
    public int SelectionIndex { get; private set; } = 0;
    public int Count => _items.Count;

    public MenuWindow(string title = "Menu", List<(string Label, Action Callback, Func<bool>?)>? items = null) : base(title)
    {
        _items = items ?? [];
        _activeItems = _items;
        UpdateActiveItems();

        AddInput(InputAction.UP, () =>
        {
            if (_activeItems.Count == 0) return;
            SelectionIndex = (SelectionIndex - 1 + _activeItems.Count) % _activeItems.Count;
        });

        AddInput(InputAction.DOWN, () =>
        {
            if (_activeItems.Count == 0) return;
            SelectionIndex = (SelectionIndex + 1) % _activeItems.Count;
        });

        AddInput(InputAction.ENTER, () =>
        {
            if (SelectionIndex < 0 || SelectionIndex >= _activeItems.Count)
                return;

            _activeItems[SelectionIndex].Callback?.Invoke();
        });
    }

    private void UpdateActiveItems()
    {
        _activeItems = _items.Where(item => item.IsEnabled == null || item.IsEnabled()).ToList();
        
        // Safety check: if selection is out of bounds after filtering, reset it
        if (_activeItems.Count == 0 || SelectionIndex >= _activeItems.Count)
        {
            SelectionIndex = 0;
        }
    }

    // Adds a menu item and its press event handler
    public void AddItem(string Label, Action Callback, Func<bool>? isEnabled = null)
    {
        _items.Add((Label, Callback, isEnabled));
        UpdateActiveItems();
    }

    public void Reset()
    {
        SelectionIndex = 0;
        UpdateActiveItems();
    }

    public override void Render()
    {
        NCurses.Clear();
        int centerX = Level.WIDTH / 2;
        int centerY = Level.HEIGHT / 2;

        if (!string.IsNullOrEmpty(Title))
        {
            NCurses.MoveAddString(centerY - 4, centerX - Title.Length / 2, Title);
        }

        UpdateActiveItems();

        int y = centerY;
        for (int i = 0; i < _activeItems.Count; i++)
        {
            var item = _activeItems[i];
            string label = item.Label;
            int x = centerX - label.Length / 2;

            if (i == SelectionIndex)
            {
                NCurses.AttributeSet(NCurses.ColorPair(5)); // Blue for active selection
                NCurses.MoveAddString(y, x - 2, "> " + label + " <");
                NCurses.AttributeSet(NCurses.ColorPair(0));
            }
            else
            {
                NCurses.MoveAddString(y, x, label);
            }

            y += 2;
        }

        if (_activeItems.Count == 0)
        {
             NCurses.MoveAddString(centerY, centerX - 10, "No options available");
        }

        NCurses.MoveAddString(y + 2, centerX - 19, "Use UP/DOWN to select, ENTER to confirm");
    }
}