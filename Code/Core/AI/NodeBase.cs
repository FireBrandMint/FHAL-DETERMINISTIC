using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FHAL.AI;

namespace FHAL.AI;
public class PathNode
{
    
    public AIVector2i Coords;
    public int GetDistance(PathNode other) => Coords.ManhattanDistance(other.Coords); // Helper to reduce noise in pathfinding
    public bool Walkable { get; private set; }
    private bool _selected;

    public virtual void Init(bool walkable, AIVector2i coords, PathNode[] neighbors)
    {
        Walkable = walkable;

        Coords = coords;

        Neighbors = neighbors;
    }

    public static event Action<PathNode> OnHoverTile;

    #region Pathfinding

    public PathNode[] Neighbors { get; protected set; }
    public PathNode Connection { get; private set; }
    public int G { get; private set; }
    public int H { get; private set; }
    public int F => G + H;

    public void SetConnection(PathNode nodeBase) {
        Connection = nodeBase;
    }

    public void SetG(int g) {
        G = g;
    }

    public void SetH(int h) {
        H = h;
    }

    #endregion
}