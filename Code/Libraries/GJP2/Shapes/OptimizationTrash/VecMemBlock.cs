using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GJP2.Optimization;

namespace GJP2;
public struct VecMemBlock
{
    readonly int Index;
    public readonly int Length;

    bool Disposed;

    public Vector2Fi this[int i]
    {
        get => ShapeVecPool.Memory[i];
        set => ShapeVecPool.Memory[i] = value;
    }

    public VecMemBlock(int index, int length)
    {
        Index = index;
        Length = length;
        Disposed = false;
    }

    public void CopyFrom(Vector2Fi[] sourceArr, int count)
    {
        var source = sourceArr.AsSpan();
        var destination = this.AsSpan();
        for(int i = 0; i < count; ++i)
        {
            destination[i] = source[i];
        }
    }

    public void CopyFrom(Span<Vector2Fi> source, int count)
    {
        var destination = this.AsSpan();
        for(int i = 0; i < count; ++i)
        {
            destination[i] = source[i];
        }
    }

    public Span<Vector2Fi> AsSpan()
    {
        return ShapeVecPool.Memory.AsSpan(Index, Length);
    }

    public void Dispose()
    {
        if(Disposed) return;
        Disposed = true;
        ShapeVecPool.Free(this);
    }
    public static VecMemBlock NullBlock() => new VecMemBlock(-1, -1);
    public int GetBlockIndex() => Index;

    public int GetBlockLength() => Length;
}