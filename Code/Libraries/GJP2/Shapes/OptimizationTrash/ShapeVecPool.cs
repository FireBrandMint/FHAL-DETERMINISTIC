using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace GJP2.Optimization;
public static class ShapeVecPool
{
    static List<FreeBlock> FreeIndex;
    public static Vector2Fi[] Memory;

    static ShapeVecPool()
    {
        Memory = new Vector2Fi[1024];
        for(int i = 0; i < Memory.Length; ++i)
        {
            Memory[i] = new Vector2Fi();
        }
        FreeIndex = new List<FreeBlock>(256);
        FreeIndex.Add(new FreeBlock(0, Memory.Length));
    }

    /// <summary>
    /// Allocated a block of memory with the given size.
    /// </summary>
    /// <param name="size"></param>
    /// <returns></returns>
    public static VecMemBlock Allocate(int size)
    {
        //index of the block it found to be more suitable
        int selectedBlock = -1;

        bool isCommon = size < 5;
        int size2x = size << 1;

        Span<FreeBlock> FreeIndexSpan = CollectionsMarshal.AsSpan<FreeBlock>(FreeIndex);
        if(isCommon) goto commonLoop;

        //not common must allocate a lot more recklessly
        for(int i = FreeIndexSpan.Length - 1; i > -1; --i)
        {
            if(FreeIndexSpan[i].Length >= size)
            {
                selectedBlock = i;
                break;
            }
        }
        goto endloop;
        commonLoop:;
        //common, allocate carefully.
        for(int i = FreeIndexSpan.Length - 1; i > -1; --i)
        {
            int len = FreeIndexSpan[i].Length;
            if(size == len | len >= size2x)
            {
                selectedBlock = i;
                break;
            }
        }
        endloop:;

        if(selectedBlock == 0)
        {
            //if it's the primary allocator
            //move its memory index further
            //and return what's left behind
            var value = FreeIndex[0];
            int ind = value.Index;
            FreeIndexSpan[0] = new FreeBlock(value.Index + size, value.Length - size);

            return new VecMemBlock(ind, size);
        }
        else if(selectedBlock > 0)
        {
            //if it's not the primary allocator
            //remove and return if it has the same asked size
            //slice and return if it has more than the asked size
            var value = FreeIndexSpan[selectedBlock];
            if(value.Length == size)
            {
                FreeIndex.RemoveAt(selectedBlock);
                return new VecMemBlock(value.Index, value.Length);
            }
            int ind = value.Index;
            Console.WriteLine("wtf2");
            FreeIndexSpan[selectedBlock] = new FreeBlock(value.Index + size, value.Length - size);

            return new VecMemBlock(ind, size);
        }
        
        //if the memory is completely full or fragmented expand

        //TODO: defragment? Maybe, because:
        //Might not be necessary since
        //circles don't allocate and they were my main concern.
            
        FreeIndexSpan[0].Length += ExpandMemory();
        
        var value1 = FreeIndex[0];
        int ind1 = value1.Index;
        FreeIndexSpan[0] = new FreeBlock(value1.Index + size, value1.Length - size);

        return new VecMemBlock(ind1, size);
    }
    /// <summary>
    /// Frees memory. Please use this.
    /// </summary>
    /// <param name="block"></param>
    public static void Free(VecMemBlock block)
    {
        FreeIndex.Add(new FreeBlock(block.GetBlockIndex(), block.GetBlockLength()));
    }

    /// <summary>
    /// It defragments this circular memory pool randomly by [amount] values
    /// or less.
    /// <para> </para>
    /// Please do not use more than 32 values as [amount]. Keep it as low as possible.
    /// <para> </para>
    /// Should be used. (sparingly)
    /// </summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void Defragment(int amount)
    {
        var mainAllocator = FreeIndex[0];

        int foundCount = 0;
        Span<int> found = stackalloc int[amount];

        Span<FreeBlock> FreeIndexSpan = CollectionsMarshal.AsSpan(FreeIndex);
        int lookingFor = mainAllocator.Index;

        int collected = 0;

        for(int i = 1; i < FreeIndexSpan.Length; ++i)
        {
            FreeBlock curr = FreeIndexSpan[i];
            int next = curr.Index + curr.Length;
            if(next != lookingFor) continue;

            lookingFor = curr.Index;
            found[foundCount] = i;
            ++foundCount;
            collected += curr.Length;
            if(foundCount == found.Length) break;
        }

        FreeIndexSpan[0] = new FreeBlock(mainAllocator.Index - collected, mainAllocator.Length + collected);

        for(int i = foundCount - 1; i > -1; --i)
        {
            FreeIndex.RemoveAt(found[i]);
        }
    }

    public static int FreeIndexSize()
    {
        return FreeIndex.Count;
    }

    private static int ExpandMemory()
    {
        int pastSize = Memory.Length;
        Vector2Fi[] newMem = new Vector2Fi[pastSize + pastSize];
        Array.Copy(Memory, newMem, pastSize);
        Memory = newMem;
        return pastSize;
    }
    
    private struct FreeBlock
    {
        public int Index;
        public int Length;

        public FreeBlock(int index, int length)
        {
            Index = index;
            Length = length;
        }
    }
}