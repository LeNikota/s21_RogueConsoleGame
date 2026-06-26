namespace domain;

class Menu(string title, List<(string Label, Action Callback)> items)
{
    public string Title { get; } = title;
    private readonly List<(string Label, Action Callback)> _items = items;

    public int SelectionIndex { get; private set; } = 0;
    public int Count => _items.Count;

    public string GetLabel(int index) => _items[index].Label;

    public void HandleInput(GameSession.InputAction action)
    {
        switch (action)
        {
            case GameSession.InputAction.UP:
                SelectionIndex = (SelectionIndex - 1 + _items.Count) % _items.Count;
                break;
            case GameSession.InputAction.DOWN:
                SelectionIndex = (SelectionIndex + 1) % _items.Count;
                break;
            case GameSession.InputAction.ENTER:
                if (SelectionIndex >= 0 && SelectionIndex < _items.Count)
                {
                    _items[SelectionIndex].Callback?.Invoke();
                }
                break;
        }
    }

    public void Reset()
    {
        SelectionIndex = 0;
    }
}