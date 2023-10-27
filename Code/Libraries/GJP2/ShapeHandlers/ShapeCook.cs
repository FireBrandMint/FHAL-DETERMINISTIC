using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using GJP2;

namespace GJP2;
public class ShapeCook
{
    static int Ticket = 0;
    int IdActual;
    List<Shape> Queue;

    public int Id { get => IdActual; }

    /// <summary>
    /// Prevents the shapes from being able to queue on this cook.
    /// </summary>
    public bool PreventQueue = false;

    public ShapeCook()
    {
        IdActual = Ticket;
        ++Ticket;

        Queue = new List<Shape>(50);
    }
    public void QueueShapeToCook(Shape shape)
    {
        if(PreventQueue) return;
        Queue.Add(shape);
    }

    public void ProcessQueue()
    {
        Span<Shape> queue = CollectionsMarshal.AsSpan(Queue);

        for(int i = 0; i < queue.Length; ++i)
        {
            queue[i].BakeShape();
        }
        
        Queue.Clear();
    }

    public void Dispose()
    {
        Queue = null;
    }
}