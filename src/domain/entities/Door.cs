namespace domain
{
    class Door(int x, int y, bool _exit = false, Key? key = null) : Entity(x, y, '#')
    {
        public bool IsExit = _exit;
        public Key? Key = key;
    }
}