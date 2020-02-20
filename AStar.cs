using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

namespace AStar
{
    /// <summary>
    /// Handles path generation.
    /// </summary>
    public static class AStar
    {
        /// <summary>
        /// Generates a path from start to end.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns>Path or null if the nodes aren't connected.</returns>
        public static Queue<T> GeneratePath<T>(INode start, INode end) where T : INode
        {
            if (end.EntryCost < 0)
                return null;
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
                    UnityEngine.Debug.Log("Path found in " + ms / 1000f +" seconds!");
                    return path;
                }

                openSet.RemoveAt(0);
                foreach(INode neighbor in current.Neighbors)
                {
                    if (current.MovementCost(neighbor) < 0)
                        continue;
                    float tentativeGScore = GetScore(gScore, current) + current.MovementCost(neighbor);
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
                return Mathf.Infinity;
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
}
