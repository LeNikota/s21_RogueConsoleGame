
namespace domain
{
    class Backpack
    {
        public const int MAX_SIZE = 9;

        private readonly List<Item> list = [];
        public List<Item> List {
            get { return list; }
        }

        public int Count { get {return list.Count;}}

        public Item this[int id]
        {
            get => list[id];
            set => list[id] = value;
        }

        public bool Add(Item item)
        {
            if (list.Count == MAX_SIZE) return false;
            list.Add(item);
            return true;
        }
        public void Remove(Item i) => list.Remove(i);

        public IEnumerator<Item> GetEnumerator()
        {
            // Just delegate to the list's enumerator
            return list.GetEnumerator();
        }

    }
}