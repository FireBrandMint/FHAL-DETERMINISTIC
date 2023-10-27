using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace GJP2;
public static class ShapeColBench
{
    static Random rand = new Random(4530);
    static Shape[] TestPool;

    #pragma warning disable 0414
    static bool Dorment = true;

    #pragma warning restore 0414

    static void Initialize ()
    {
        TestPool = new Shape[8000];

        for(int i = 0; i < 8000; i += 2)
        {
            TestPool[i] = Shape.NewRectangle(new Vector2Fi(0,0), new Vector2Fi(0,0), new FInt(5), new Vector2Fi(5,5), new Vector2Fi(1,1));
            TestPool[i + 1] = Shape.NewRectangle(new Vector2Fi(2,0), new Vector2Fi(0,0), new FInt(-5), new Vector2Fi(5,5), new Vector2Fi(1,1));
        }

        Dorment = false;
    }

    static void InitInternalCache()
    {
        int numOverhead = 500;
        for(int i = 0; i < numOverhead; ++i)
        {
            Shape s1 = Shape.NewTriangle(new Vector2Fi(0,0), new Vector2Fi(0,0), new FInt(5), new Vector2Fi(1,1), new FInt(1), new Vector2Fi(0, 1));
            s1.Dispose();
        }
        Dorment = false;
    }

    static Shape s1 = Shape.NewRectangle(new Vector2Fi(0,0), new Vector2Fi(0,0), new FInt(5), new Vector2Fi(5,5), new Vector2Fi(1,1));
    static Shape s2 = Shape.NewRectangle(new Vector2Fi(2,0), new Vector2Fi(0,0), new FInt(-5), new Vector2Fi(5,5), new Vector2Fi(1,1));

    public static void TestCol1()
    {
        if(Dorment) InitInternalCache();
        //if(Dorment) Initialize();

        //Shape s1 = Shape.NewRectangle(new Vector2Fi(0,0), new Vector2Fi(0,0), new FInt(5), new Vector2Fi(5,5), new Vector2Fi(1,1));
        //Shape s2 = Shape.NewRectangle(new Vector2Fi(2,0), new Vector2Fi(0,0), new FInt(-5), new Vector2Fi(5,5), new Vector2Fi(1,1));
        
        CollisionResult res = new CollisionResult();
        s1.IntersectsInfo(s2, ref res);
        res.Separation += new Vector2Fi();

        s1.Dispose();
        s2.Dispose();
    }
}