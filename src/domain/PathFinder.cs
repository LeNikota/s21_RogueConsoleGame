
namespace domain
{
    class PathFinder
    {
        public static List<Vector2i> FindPath(Level level, Vector2i start, Vector2i end)
        {
            if (start == end) return [start];
            if (!level.IsPositionValid(start) || !level.IsPositionValid(end)) return [];

            Dictionary<Vector2i, Vector2i> CameFrom = [];
            CameFrom[start] = start;
            
            Dictionary<Vector2i, double> GCost = []; //cost so far
            GCost[start] = 0;

            PriorityQueue<Vector2i, double> Frontier = new(); //opened nodes
            Frontier.Enqueue(start, 0);
            
            HashSet<Vector2i> closed = [];
            Vector2i curr;
            
            while (Frontier.Count > 0)
            {
                curr = Frontier.Dequeue();
                if (curr == end) break; //found

                if (closed.Contains(curr)) continue;
                closed.Add(curr);
                
                List<Vector2i> neighbors = [curr + Vector2i.UP, curr + Vector2i.DOWN, curr + Vector2i.LEFT, curr + Vector2i.RIGHT];
                neighbors = neighbors.FindAll(level.IsPositionValid);
                
                foreach (Vector2i next in neighbors)
                {
                    double newCost = 1 + GCost[curr];
                    if (!GCost.TryGetValue(next, out double value) || newCost < value)
                    {
                        GCost[next] = newCost;
                        double priority = newCost + next.DistanceTo(end);
                        Frontier.Enqueue(next, priority);
                        CameFrom[next] = curr;
                    }
                }
            
            }
            List<Vector2i> result = [];
            if (!CameFrom.ContainsKey(end)) return result; // no path can be found

            curr = end;
            while (curr != start) {
                result.Add(curr);
                curr = CameFrom[curr];
            }
            result.Reverse();
            return result;
        }
    }
}