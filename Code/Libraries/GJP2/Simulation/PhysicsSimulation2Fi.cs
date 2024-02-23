using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FHAL.Math;

namespace GJP2;

public class PhysicsSimulation2Fi
{
    static Stack<WTFDictionary<int, PhysicsBody2Fi>> DictPool = new Stack<WTFDictionary<int, PhysicsBody2Fi>>(50);

    int IDTicket = 0;
    ShapeCook Cook = new ShapeCook();

    public bool CreatingRigid = false;

    WTFDictionary<int, PhysicsBody2Fi> Registry;

    WTFSaltyDictionary<Vector2L, WTFDictionary<int, PhysicsBody2Fi>> BinaryTree;

    int BTreeFraction;

    List<PhysicsBody2Fi> InitializeList = new List<PhysicsBody2Fi>(25);

    List<PhysicsBody2Fi> RecalculateList = new List<PhysicsBody2Fi>(25);

    public PhysicsSimulation2Fi()
    {
        Registry = new WTFDictionary<int, PhysicsBody2Fi>();
        BinaryTree = new WTFSaltyDictionary<Vector2L, WTFDictionary<int, PhysicsBody2Fi>>(100, 40);
        BTreeFraction = 6;
    }

    public PhysicsSimulation2Fi(int dividingBits)
    {
        Registry = new WTFDictionary<int, PhysicsBody2Fi>();
        BinaryTree = new WTFSaltyDictionary<Vector2L, WTFDictionary<int, PhysicsBody2Fi>>(100, 40);
        if(dividingBits < 2) throw new Exception("PhysicsSimulation2Fi cannot have less than 2 dividing bits");
        BTreeFraction = dividingBits;
    }

    public void PhysicsProcess()
    {
        for(int i = InitializeList.Count; i > 0; --i)
        {
            var curr = InitializeList[i-1];
            if(!curr.Disposed) PutInBinaryTree(curr);
        }
        InitializeList.Clear();

        for(int i = RecalculateList.Count; i > 0; --i)
        {
            var curr = RecalculateList[i-1];
            if(curr.Disposed) continue;
            
            curr.Recalculate(SolveCol);
        }
        RecalculateList.Clear();
    }

    private void SolveCol(PhysicsBody2Fi body)
    {

    }

    /// <summary>
    /// <para>Creates a shape.</para>
    /// IMPORTANT: the shape is bound to this physics simulation
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="center"></param>
    /// <param name="rotation"></param>
    /// <param name="scale"></param>
    /// <param name="length"></param>
    /// <param name="middleVertice"></param>
    /// <returns></returns>
    public Shape NewTriangle(Vector2Fi pos, Vector2Fi center, FInt rotation, Vector2Fi scale, FInt length, Vector2Fi middleVertice)
    {
        Shape subject = Shape.NewTriangle(pos, center, rotation, scale, length, middleVertice);

        return subject;
    }
    /// <summary>
    /// <para>Creates a shape.</para>
    /// IMPORTANT: the shape is bound to this physics simulation
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="center"></param>
    /// <param name="radius"></param>
    /// <returns></returns>
    public Shape NewCircle(Vector2Fi pos, Vector2Fi center, FInt radius)
    {
        Shape subject = Shape.NewCircle(pos, center, radius);

        return subject;
    }

    public PhysicsBody2Fi NewBodyRect(Vector2Fi pos, Vector2Fi center, FInt rotation, Vector2Fi sizeRect, Vector2Fi scale)
    {
        Span<Shape> shapes = stackalloc Shape[1]
        {
            Shape.NewRectangle(pos, center, rotation, sizeRect, scale)
        };
        shapes[0].BakeShape();
        var result = CreateNew(pos, shapes);

        return result;
    }

    private PhysicsBody2Fi CreateNew(Vector2Fi bodyPos, Span<Shape> shapes)
    {
        var subject = new PhysicsBody2Fi(IDTicket, bodyPos, shapes, this);
        ++IDTicket;

        for(int i = 0; i < subject.ColliderCount; ++i)
        {
            ref Shape col = ref subject.GetShape(i);
            col.BakeShape();
        }

        Registry.Add(subject.ID, subject);

        InitializeList.Add(subject);

        return subject;
    }

    //relocates physics bodies
    private void MoveInBinaryTree(PhysicsBody2Fi subject)
    {
        var subjectID = subject.ID;

        var aabb = subject.GetAABB();
        var tlf = aabb.TopLeft >> BTreeFraction;
        var brf = aabb.BottomRight >> BTreeFraction;
        Vector2L tl = new Vector2L((long)tlf.x, (long)tlf.y);
        Vector2L br = new Vector2L((long)brf.x, (long)brf.y);

        Vector2L bltl = subject.BinaryLocationTL;
        Vector2L blbr = subject.BinaryLocationBR;

        for(long x = tl.x; x <= br.x; ++x)
        {
            for(long y = tl.y; y <= br.y; ++y)
            {
                var curr = new Vector2L(x,y);
                if(curr >= tl && curr <= br) continue;
                var bt = BinaryTree[curr];
                bt.Remove(subjectID);
                if(bt.Count == 0) RecycleDict(bt);
            }
        }

        for(long x = bltl.x; x <= blbr.x; ++x)
        {
            for(long y = bltl.y; y <= blbr.y; ++y)
            {
                var curr = new Vector2L(x,y);
                if(curr >= tl && curr <= br) continue;
                WTFDictionary<int, PhysicsBody2Fi> block;
                if(!BinaryTree.TryGetValue(curr, out block))
                {
                    block = GetDict();
                    BinaryTree.Add(curr, block);
                }

                block.Add(subjectID, subject);
            }
        }

        subject.BinaryLocationTL = tl;
        subject.BinaryLocationBR = br;
    }

    //it only puts the physics body in the simulation
    private void PutInBinaryTree(PhysicsBody2Fi subject)
    {

        var aabb = subject.GetAABB();
        var tlf = aabb.TopLeft >> BTreeFraction;
        var brf = aabb.BottomRight >> BTreeFraction;
        Vector2L tl = new Vector2L((long)tlf.x, (long)tlf.y);
        Vector2L br = new Vector2L((long)brf.x, (long)brf.y);

        for(long x = tl.x; x <= br.x; ++x)
        {
            for(long y = tl.y; y <= br.y; ++y)
            {
                var curr = new Vector2L(x,y);
                WTFDictionary<int, PhysicsBody2Fi> block;
                if(!BinaryTree.TryGetValue(curr, out block))
                {
                    block = GetDict();
                    BinaryTree.Add(curr, block);
                }

                block.Add(subject.ID, subject);
            }
        }

        subject.BinaryLocationTL = tl;
        subject.BinaryLocationBR = br;
    }

    private void RecycleDict(WTFDictionary<int, PhysicsBody2Fi> dict)
    {
        DictPool.Push(dict);
    }

    private WTFDictionary<int, PhysicsBody2Fi> GetDict()
    {
        if(DictPool.Count == 0)
            return new WTFDictionary<int, PhysicsBody2Fi>(1 << (BTreeFraction - 1));
        else
            return DictPool.Pop();
    }

    public class PhysicsBody2Fi : IDisposable
    {
        static ArrayPool<Shape> ShapeArrPool = ArrayPool<Shape>.Create();
        #pragma warning disable 0169
        public readonly int ID;

        /// <summary>
        /// Motion steps must be more than 0.
        /// </summary>
        public uint MotionSteps
        {
            get => _MotionSteps;
            set
            {
                if(value == 0) throw new Exception("MotionSteps MUST be more than 0");
                _MotionSteps = value;
            }
        }
        public uint _MotionSteps = 1;

        Vector2Fi _Position, _PastPosition = new Vector2Fi(FInt.MinValue, FInt.MinValue);

        public Vector2Fi Motion
        {
            get => _Motion;
            set
            {
                _Motion = value;
            }
        }
        public Vector2Fi _Motion = Vector2Fi.ZERO;

        public int ColliderCount;
        Shape[] Colliders;

        bool Dirty = false;

        PhysicsSimulation2Fi Simulation;

        public Vector2L BinaryLocationTL;
        public Vector2L BinaryLocationBR;
        bool Changed = false;
        bool _Disposed = false;

        public Vector2Fi Position
        {
            get => _Position;
            set
            {
                if(_PastPosition.x == FInt.MinValue) _PastPosition = _Position;

                RegisterChange();
                _Position = value;
            }
        }

        public bool Disposed
        {
            get => _Disposed;
        }

        private PhysicsBody2Fi()
        {

        }

        public PhysicsBody2Fi(int id, Vector2Fi bodyPos, Span<Shape> colliders, PhysicsSimulation2Fi simulation)
        {
            ID = id;
            _Position = bodyPos;
            ColliderCount = colliders.Length;
            var cols = ShapeArrPool.Rent(colliders.Length);
            for(int i = 0; i < colliders.Length; ++i) cols[i] = colliders[i];
            Colliders = cols;
            Simulation = simulation;

            BinaryLocationTL = new Vector2L(long.MinValue, long.MinValue);
        }

        public ref Shape GetShape(int index)
        {
            return ref Colliders[index];
        }

        public Shape.AABB GetAABB()
        {
            Shape.AABB result = new Shape.AABB();
            result.TopLeft = new Vector2Fi(FInt.MaxValue, FInt.MaxValue);
            result.BottomRight = new Vector2Fi(FInt.MinValue, FInt.MinValue);
            Span<Shape> cols = Colliders.AsSpan<Shape>(0, ColliderCount);
            for(int i = cols.Length; i > 0 ; --i)
            {
                ref Shape col = ref cols[i-1];
                ref var curr = ref col.Area;

                result.TopLeft = curr.TopLeft.Min(curr.TopLeft);
                result.BottomRight = curr.BottomRight.Max(curr.BottomRight);
            }

            return result;
        }

        private void RegisterChange()
        {
            if(Changed) return;
            Simulation.RecalculateList.Add(this);

            Changed = true;
        }

        public void Recalculate(Action<PhysicsBody2Fi> physicsSolveCallback)
        {
            _Position = _PastPosition;

            if(_PastPosition.x == FInt.MinValue | _Position == _PastPosition) goto skipPos;

            var diff = (_PastPosition - _Position);
            for(int i = ColliderCount; i > 0; --i)
            { Colliders[i-1].Position += diff; }
            _PastPosition = new Vector2Fi(FInt.MinValue, FInt.MinValue);
            skipPos:;

            physicsSolveCallback.Invoke(this);

            var motion = _Motion;
            var steps = _MotionSteps;
            if(motion == Vector2Fi.ZERO) goto skipMotion;
            Vector2Fi motionSegment = motion/(int)steps;
            for(uint i = steps - 1; i > 0; --i)
            {
                for(int i2 = ColliderCount; i2 > 0; --i2)
                { Colliders[i2-1].Position += motionSegment; }
                BakeShapes();
                physicsSolveCallback.Invoke(this);
            }
            _Motion = new Vector2Fi(0,0);

            skipMotion:;
            /*
            if(_PastPosition.x == FInt.MinValue) goto skipPos;
            var diff = (_PastPosition - _Position) * stepMultiplier;
            if(diff == Vector2Fi.ZERO) goto skipPos;

            for(int i = ColliderCount; i > 0; --i)
            { Colliders[i-1].Position += diff; }
            skipPos:;
            */
        }

        private void BakeShapes()
        {
            for(int i = ColliderCount; i > 0; --i)
            {
                Colliders[i-1].BakeShape();
            }
        }

        public void ResetLastTick()
        {
            _PastPosition = new Vector2Fi(FInt.MinValue, FInt.MinValue);
        }

        public void Dispose()
        {
            if(_Disposed) return;
            _Disposed = true;
            for(int i = 0; i < ColliderCount; ++i) Colliders[i].Dispose();
            ShapeArrPool.Return(Colliders);
            Colliders = null;
        }
    }
}