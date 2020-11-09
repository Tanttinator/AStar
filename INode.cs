
namespace Tanttinator.AStar
{
    public interface INode
    {
        float X { get; }
        float Y { get; }
        INode[] Neighbors { get; }
        float EntryCost(object agent, INode from);
        bool CanEnter(object agent, INode from);
    }
}
