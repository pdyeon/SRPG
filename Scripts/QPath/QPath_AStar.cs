using System.Collections.Generic;
using System.Linq;

namespace QPath
{
    public class QPath_AStar<T> where T : IQPathTile
    {
        public QPath_AStar(
            IQPathWorld world,
            IQPathUnit unit,
            T startTile,
            T endTile,
            CostEstimateDelegate costEstimateFunc
            ) 
        {
            this.world = world;
            this.unit = unit;
            this.startTile = startTile;
            this.endTile = endTile;
            this.costEstimateFunc = costEstimateFunc;
        }

        IQPathWorld world;
        IQPathUnit unit;
        T startTile;
        T endTile;
        CostEstimateDelegate costEstimateFunc;

        Queue<T> path;

        public void DoWork()
        {
            path = new Queue<T>();

            HashSet<T> closedSet = new HashSet<T>();

            PathfindingPriorityQueue<T> openSet = new PathfindingPriorityQueue<T>();
            openSet.Enqueue(startTile, 0);

            Dictionary<T, T> came_From = new Dictionary<T, T>();

            Dictionary<T, float> g_score = new Dictionary<T, float>();
            g_score[startTile] = 0;

            Dictionary<T, float> f_score = new Dictionary<T, float>();
            f_score[startTile] = costEstimateFunc(startTile, endTile);

            while(openSet.Count > 0)
            {
                T current = openSet.Dequeue();

                if(System.Object.ReferenceEquals(current, endTile))
                {
                    Reconstruct_path(came_From, current);
                    return;
                }

                closedSet.Add(current);

                foreach(T edge_neighbour in current.GetNeighbours())
                {
                    T neighbour = edge_neighbour;

                    if(closedSet.Contains(neighbour))
                    {
                        continue;
                    }

                    float total_pathfinding_cost_to_neighbor = 
                        neighbour.AggregateCostToEnter(g_score[current], current, unit);

                    if(total_pathfinding_cost_to_neighbor < 0)
                    {
                        // Values less than zero represent an invalid/impassable tile
                        continue;
                    }

                    //Debug.Log(total_pathfinding_cost_to_neighbor);

                    float tentative_g_score = total_pathfinding_cost_to_neighbor;

                    if(openSet.Contains(neighbour) && tentative_g_score >= g_score[neighbour])
                    {
                        continue;
                    }

                    came_From[neighbour] = current;
                    g_score[neighbour] = tentative_g_score;
                    f_score[neighbour] = g_score[neighbour] + costEstimateFunc(neighbour, endTile);

                    openSet.EnqueueOrUpdate(neighbour, f_score[neighbour]);
                }
            }
        }

        private void Reconstruct_path(
            Dictionary<T, T> came_From,
            T current
            )
        {
            Queue<T> total_path = new Queue<T>();
            total_path.Enqueue(current);

            while(came_From.ContainsKey(current))
            {
                current = came_From[current];
                total_path.Enqueue(current);
            }

            path = new Queue<T>(total_path.Reverse());
        }

        public T[] GetList()
        {
            return path.ToArray();
        }
    }
}
