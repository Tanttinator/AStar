
namespace AStar
{
    public interface INode
    {
        Vector2 Position { get; }
        INode[] Neighbors { get; }
        float EntryCost(object agent, INode from);
        bool CanEnter(object agent, INode from);
    }
}
