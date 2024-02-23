using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace FHAL.AI;
public struct AIVector2i
{
    public int x, y;
    public AIVector2i(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public int ManhattanDistance (AIVector2i targetNode)
    {
        return Math.Abs(x - targetNode.x) + Math.Abs(y - targetNode.y);
    }

    public override int GetHashCode() => x * 31 + y;

    public override bool Equals([NotNullWhen(true)] object obj)
    {
        if(!(obj is AIVector2i)) return false;
        var fodder = (AIVector2i) obj;
        return this.x == fodder.x & this.y == fodder.y;
    }
}