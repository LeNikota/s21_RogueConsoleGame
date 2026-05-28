
namespace domain
{
    //in-game logger
    class GameLog
    {
        private static string log = "Welcome to s21_rogue!";
        public static string GetLog() { return log; }

        public static void Log(string new_log) { log = new_log; }
        public static void Clear() { log = ""; }
        public static void Add(string _str) { log += _str; }

    }
}