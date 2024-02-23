using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Numerics;
using System.Diagnostics.CodeAnalysis;

namespace FHAL.AI;

public class Pathfinding
{
    static List<AIVector2i> NeighborCache = new List<AIVector2i>();

    public static List<PathNode> FindPath(PathNode startNode, PathNode targetNode) {
            var toSearch = new List<PathNode>() { startNode };
            var processed = new List<PathNode>();

            while (toSearch.Any()) {
                var current = toSearch[0];
                foreach (var t in toSearch) 
                    if (t.F < current.F || t.F == current.F && t.H < current.H) current = t;

                processed.Add(current);
                toSearch.Remove(current);

                if (current == targetNode) {
                    var currentPathTile = targetNode;
                    var path = new List<PathNode>();
                    var count = 100;
                    while (currentPathTile != startNode) {
                        path.Add(currentPathTile);
                        currentPathTile = currentPathTile.Connection;
                        count--;
                        if (count < 0) throw new Exception();
                        //Debug.Log("sdfsdf");
                    }

                    //Debug.Log(path.Count);
                    return path;
                }

                foreach (var neighbor in current.Neighbors.Where(t => t.Walkable && !processed.Contains(t))) {
                    var inSearch = toSearch.Contains(neighbor);

                    var costToNeighbor = current.G + current.GetDistance(neighbor);

                    if (!inSearch || costToNeighbor < neighbor.G) {
                        neighbor.SetG(costToNeighbor);
                        neighbor.SetConnection(current);

                        if (!inSearch) {
                            neighbor.SetH(neighbor.GetDistance(targetNode));
                            toSearch.Add(neighbor);
                        }
                    }
                }
            }
            return null;
        }
}