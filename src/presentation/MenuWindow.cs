using Mindmagma.Curses;
using domain;

namespace presentation;

class MenuWindow : Window
{
    private readonly List<(string Label, Action Callback, Func<bool>? IsEnabled)> _items;
    public int SelectionIndex { get; private set; } = 0;
    public int Count => _items.Count;

    public MenuWindow(string title = "Menu", List<(string Label, Action Callback, Func<bool>?)>? items = null) : base(title)
    {
        _items = items ?? [];

        AddInput(InputAction.UP, () =>
        {
            if (_items.Count == 0) return;

            int startIndex = SelectionIndex;
            bool isEnabled;
            do
            {
                SelectionIndex = (SelectionIndex + _items.Count - 1) % _items.Count;
                if (SelectionIndex == startIndex) break;

                var item = _items[SelectionIndex];
                isEnabled = item.IsEnabled == null || item.IsEnabled();
            } while (!isEnabled);
        });

        AddInput(InputAction.DOWN, () =>
        {
            if (_items.Count == 0) return;

            int startIndex = SelectionIndex;
            bool isEnabled;
            do
            {
                SelectionIndex = (SelectionIndex + 1) % _items.Count;
                if (SelectionIndex == startIndex) break;

                var item = _items[SelectionIndex];
                isEnabled = item.IsEnabled == null || item.IsEnabled();
            } while (!isEnabled);
        });

        AddInput(InputAction.ENTER, () =>
        {
            if (SelectionIndex < 0 || SelectionIndex >= _items.Count)
                return;

            var item = _items[SelectionIndex];
            bool isEnabled = item.IsEnabled == null || item.IsEnabled();

            if (isEnabled)
            {
                item.Callback?.Invoke();
            }
        });
    }

    // Adds a menu item and its press event handler
    public void AddItem(string Label, Action Callback, Func<bool>? isEnabled = null)
    {
        _items.Add((Label, Callback, isEnabled));
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

        if (!string.IsNullOrEmpty(Title))
        {
            NCurses.MoveAddString(centerY - 4, centerX - Title.Length / 2, Title);
        }

        int y = centerY;
        for (int i = 0; i < _items.Count; i++)
        {
            var item = _items[i];

            string label = item.Label;
            int x = centerX - label.Length / 2;
            bool isEnabled = item.IsEnabled == null || item.IsEnabled();

            if (!isEnabled)
                continue;

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

            y+=2;
        }

        NCurses.MoveAddString(y + 2, centerX - 19, "Use UP/DOWN to select, ENTER to confirm");
    }
}