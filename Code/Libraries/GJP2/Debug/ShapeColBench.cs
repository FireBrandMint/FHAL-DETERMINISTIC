using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using GJP2.Optimization;

namespace GJP2;
public static class ShapeColBench
{
    static Random rand = new Random(4530);
    static Shape[] TestPool;

    #pragma warning disable 0414
    static bool Dorment = true;

    #pragma warning restore 0414

    static ShapeColBench ()
    {
        Initialize();
        s1 = Shape.NewRectangle(new Vector2Fi(0,0), new Vector2Fi(0,0), new FInt(5), new Vector2Fi(5,5), new Vector2Fi(1,1));
        s2 = Shape.NewRectangle(new Vector2Fi(2,0), new Vector2Fi(0,0), new FInt(-5), new Vector2Fi(5,5), new Vector2Fi(1,1));
    }

    static void Initialize ()
    {
        int size = 8000;
        TestPool = new Shape[size];

        for(int i = 0; i < size; i += 2)
        {
            TestPool[i] = Shape.NewRectangle(new Vector2Fi(0,0), new Vector2Fi(0,0), new FInt(5), new Vector2Fi(5,5), new Vector2Fi(1,1));
            TestPool[i + 1] = Shape.NewRectangle(new Vector2Fi(2,0), new Vector2Fi(0,0), new FInt(-5), new Vector2Fi(5,5), new Vector2Fi(1,1));
            TestPool[i].BakeShape();
            TestPool[i + 1].BakeShape();
        }
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

    static Shape s1;
    static Shape s2;
    public static void TestCol1()
    {
        //if(Dorment) InitInternalCache();
        Shape s1 = TestPool[rand.Next(TestPool.Length - 6000) + 6000];
        Shape s2 = TestPool[rand.Next(TestPool.Length - 2000)];
        //Shape s1 = Shape.NewRectangle(new Vector2Fi(0,0), new Vector2Fi(0,0), new FInt(5), new Vector2Fi(5,5), new Vector2Fi(1,1));
        //Shape s2 = Shape.NewRectangle(new Vector2Fi(2,0), new Vector2Fi(0,0), new FInt(-5), new Vector2Fi(5,5), new Vector2Fi(1,1));
        s1.Position += new Vector2Fi(1, 0);
        s2.Position += new Vector2Fi(1, 0);
        s1.Rotation += 359;
        s2.Rotation += 359;
        s1.BakeShape();
        s2.BakeShape();

        
        CollisionResult res = new CollisionResult();
        s1.IntersectsInfo(s2, ref res);
        res.Separation += new Vector2Fi();


        s1.Dispose();
        s2.Dispose();
    }
}