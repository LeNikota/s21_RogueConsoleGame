namespace domain
{
    class Door(int x, int y, bool exit = false, Key? key = null) : Entity(x, y, '#', key?.Color ?? Color.None)
    {
        public bool IsExit { get; } = exit;
        public Key? Key { get; } = key;
    }
}
