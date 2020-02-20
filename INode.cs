using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AStar
{
    public interface INode
    {
        Vector2 Position { get; }
        INode[] Neighbors { get; }
        float MovementCost { get; }
    }
}
