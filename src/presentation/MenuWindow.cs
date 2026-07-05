namespace domain;

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

    public void AddItem(string Label, Action Callback)
    {
        _items.Add((Label, Callback));
    }


    public string GetLabel(int index) => _items[index].Label;

    public void Reset()
    {
        SelectionIndex = 0;
    }
}