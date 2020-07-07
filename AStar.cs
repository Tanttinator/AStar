using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace AStar
{
    /// <summary>
    /// Handles path generation.
    /// </summary>
    public static class AStar
    {

        static Queue<IPathData> pathQueue = new Queue<IPathData>();

        /// <summary>
        /// Generates a path from start to end.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns>Path or null if the nodes aren't connected.</returns>
        public static Queue<T> GeneratePath<T>(INode start, INode end, object agent) where T : INode
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            Stopwatch.StartNew();
            Queue<T> path = new Queue<T>();

            SortedList<float, INode> openSet = new SortedList<float, INode>(new DuplicateKeyComparer<float>());
            Dictionary<INode, INode> cameFrom = new Dictionary<INode, INode>();
            Dictionary<INode, float> gScore = new Dictionary<INode, float>();
            Dictionary<INode, float> fScore = new Dictionary<INode, float>();

            gScore[start] = 0f;
            fScore[start] = CostEstimate(start, end);
            openSet.Add(GetScore(fScore, start), start);

            while(openSet.Count > 0)
            {
                INode current = openSet.First().Value;
                if(current == end)
                {
                    INode step = end;
                    path.Enqueue((T)step);
                    while(cameFrom.ContainsKey(step))
                    {
                        step = cameFrom[step];
                        path.Enqueue((T)step);
                    }
                    path = new Queue<T>(path.Reverse());
                    stopWatch.Stop();
                    long ms = stopWatch.ElapsedMilliseconds;
                    //UnityEngine.Debug.Log("Path found in " + ms / 1000f +" seconds!");
                    return path;
                }

                openSet.RemoveAt(0);
                foreach(INode neighbor in current.Neighbors)
                {
                    if (!neighbor.CanEnter(agent, current))
                        continue;
                    float tentativeGScore = GetScore(gScore, current) + neighbor.EntryCost(agent, current);
                    if(tentativeGScore < GetScore(gScore, neighbor))
                    {
                        cameFrom[neighbor] = current;
                        gScore[neighbor] = tentativeGScore;
                        fScore[neighbor] = GetScore(gScore, neighbor) + CostEstimate(neighbor, end);
                        if (!openSet.ContainsValue(neighbor))
                            openSet.Add(GetScore(fScore, neighbor), neighbor);
                    }
                }
            }

            stopWatch.Stop();
            UnityEngine.Debug.Log("Path not found!");
            return null;
        }

        /// <summary>
        /// Estimation algorithm for A*.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        static float CostEstimate(INode node, INode end)
        {
            return Vector2.Distance(node.Position, end.Position);
        }

        /// <summary>
        /// Try to get a score from a dictionary.
        /// </summary>
        /// <param name="dict"></param>
        /// <param name="node"></param>
        /// <returns>Value in the dictionary or Mathf.infinite if the node doesn't have a score yet.</returns>
        static float GetScore(Dictionary<INode, float> dict, INode node)
        {
            if (dict.ContainsKey(node))
                return dict[node];
            else
                return float.MaxValue;
        }

        /// <summary>
        /// Create a pathfinding request.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="callback"></param>
        public static void GeneratePathThreaded<T>(INode start, INode end, object agent, Action<Queue<T>> callback) where T : INode
        {
            pathQueue.Enqueue(new PathData<T>(start, end, agent, callback));
        }

        /// <summary>
        /// Starts a new thread to handle path requests.
        /// </summary>
        public static void StartPathThread()
        {
            ThreadStart start = new ThreadStart(delegate
            {
                while(true)
                {
                    if(pathQueue.Count > 0)
                    {
                        IPathData data = pathQueue.Dequeue();
                        Type type = data.Type;
                        var method = typeof(AStar).GetMethod("GeneratePath", new Type[] { typeof(INode), typeof(INode), typeof(object) }).MakeGenericMethod(type);
                        var path = method.Invoke(null, new object[] { data.Start, data.End, data.Agent });
                        data.Invoke(new Queue<INode>((path as IEnumerable).Cast<INode>()));
                    }
                }
            });
            new Thread(start).Start();
        }
    }

    /// <summary>
    /// Used to handle duplicate keys in a sortedlist.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    class DuplicateKeyComparer<TKey> : IComparer<TKey> where TKey : IComparable
    {
        public int Compare(TKey x, TKey y)
        {
            int result = x.CompareTo(y);
            if (result == 0)
                return 1;
            else
                return result;
        }
    }

    /// <summary>
    /// Generic wrapper for PathData
    /// </summary>
    public interface IPathData
    {
        INode Start { get; }
        INode End { get; }
        object Agent { get; }
        Type Type { get; }
        void Invoke(Queue<INode> path);
    }

    /// <summary>
    /// Data struct for path generation thread.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct PathData<T> : IPathData where T : INode
    {
        public INode start;
        public INode end;
        public object agent;
        public Type type;
        public Action<Queue<T>> callback;

        public PathData(INode start, INode end, object agent, Action<Queue<T>> callback)
        {
            this.start = start;
            this.end = end;
            this.agent = agent;
            this.callback = callback;
            type = typeof(T);
        }

        public INode Start => start;

        public INode End => end;

        public object Agent => agent;

        public Type Type => type;

        public void Invoke(Queue<INode> path)
        {
            Queue<T> newPath = new Queue<T>(path.Cast<T>());
            callback?.Invoke(newPath);
        }
    }
}
