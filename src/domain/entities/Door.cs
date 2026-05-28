namespace domain
{
    class Door(int x, int y, bool _exit = false) : Entity(x, y, '#')
    {
        public bool IsExit = _exit;
    }
}