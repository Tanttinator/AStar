using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AStar {
    public interface IAgent
    {
        float MovementCost(INode from, INode to);
    }
}
