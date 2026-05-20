namespace domain.generation
{
    public class UnionFind
    {
        private readonly int[] parent;
        private readonly int[] rank;

        public UnionFind(int size)
        {
            parent = new int[size];
            rank = new int[size];

            for (int i = 0; i < size; i++) parent[i] = i;
            for (int i = 0; i < size; i++) rank[i] = 0;
        }

        public int Find(int i)
        {
            if (parent[i] != i)
                return parent[i] = Find(parent[i]);

            return parent[i];
        }

        public void Union(int i, int j)
        {
            int root_i = Find(i);
            int root_j = Find(j);

            if (root_i == root_j) return;

            if (rank[root_i] < rank[root_j])
            {
                parent[root_i] = root_j;
            }
            else if (rank[root_i] > rank[root_j])
            {
                parent[root_j] = root_i;
            }
            else
            {
                parent[root_j] = root_i;
                rank[root_i]++;
            }
        }
    }


    public static class Tools
    {
        static readonly Random rand = new();

        public static void Shuffle<T>(T[] array)
        {
            if (array == null || array.Length < 2)
                return;

            int n = array.Length;
            for (int i = 0; i < n - 1; i++)
            {
                int j = rand.Next(i, n);
                (array[j], array[i]) = (array[i], array[j]);
            }
        }

        public static void Shuffle<T>(IList<T> list)
        {
            int n = list.Count;
            for (int i = 0; i < n - 1; i++)
            {
                int j = rand.Next(i, n);
                (list[j], list[i]) = (list[i], list[j]);
            }
        }
    }
}