namespace domain
{
    class Door(int x, int y, bool exit = false, Key? key = null) : Entity(x, y, '#', key?.Color ?? Color.None)
    {
        public bool IsExit { get; } = exit;
        public Key? Key { get; } = key;

        public bool Open(Player player, Level level)
        {
            bool can_open = player.GetBackpack().List.Any(item => item is Key k && k == Key);

            if (can_open)
            {
                GameLog.Add($"You open the {Key?.Color} door.");
            }
            else
            {
                GameLog.Add($"The door is locked. You need a {Key}.");
            }

            return can_open;
        }
    }
}
