using Mindmagma.Curses;
using domain;

namespace presentation;

class MenuWindow : Window
{
    private readonly List<(string Label, Action Callback)> _items;

    public int SelectionIndex { get; private set; } = 0;
    public int Count => _items.Count;

    public MenuWindow(string title = "Menu", List<(string Label, Action Callback)>? items = null) : base(title)
    {
        _items = items ?? [];

        AddInput(InputAction.UP, () =>
        {
            SelectionIndex = (SelectionIndex - 1 + _items.Count) % _items.Count;
        });

        AddInput(InputAction.DOWN, () =>
        {
            SelectionIndex = (SelectionIndex + 1) % _items.Count;
        });

        AddInput(InputAction.ENTER, () =>
        {
            if (SelectionIndex >= 0 && SelectionIndex < _items.Count)
            {
                _items[SelectionIndex].Callback?.Invoke();
            }
        });
    }

    // Adds a menu item and its press event handler
    public void AddItem(string Label, Action Callback)
    {
        _items.Add((Label, Callback));
    }


    public string GetLabel(int index) => _items[index].Label;

    public void Reset()
    {
        SelectionIndex = 0;
    }

    public override void Render()
    {

        NCurses.Clear();
        int centerX = Level.WIDTH / 2;
        int centerY = Level.HEIGHT / 2;

        // Draw Title
        if (!string.IsNullOrEmpty(Title))
        {
            NCurses.MoveAddString(centerY - 4, centerX - Title.Length / 2, Title);
        }

        // Draw Options
        for (int i = 0; i < _items.Count; i++)
        {
            int y = centerY + i * 2;
            string label = _items[i].Label;
            int x = centerX - label.Length / 2;

            if (i == SelectionIndex)
            {
                NCurses.AttributeSet(NCurses.ColorPair(5)); // Highlight color (Blue)
                NCurses.MoveAddString(y, x - 2, "> " + label + " <");
                NCurses.AttributeSet(NCurses.ColorPair(0)); // Reset
            }
            else
            {
                NCurses.MoveAddString(y, x, label);
            }
        }

        NCurses.MoveAddString(centerY + 10, centerX - 19, "Use UP/DOWN to select, ENTER to confirm");
    }
}