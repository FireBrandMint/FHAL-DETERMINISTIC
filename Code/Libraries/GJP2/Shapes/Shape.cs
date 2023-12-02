using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using GJP2.Optimization;

namespace GJP2;
public struct Shape
{
    public Vector2Fi Position
    {
        get => TruePosition;
        set
        {
            if(value == TruePosition) return;

            TruePosition = value;
            if(this.ShapeType == Type.Circle)
            {
                Should.updateArea = true;
                return;
            }
            Should.updateModel = true;
            Should.updateArea = true;
        }
    }

    public Vector2Fi CenterPoint
    {
        get => TrueCenterPoint;
        set
        {
            if(value == TrueCenterPoint) return;

            TrueCenterPoint = value;
            if(this.ShapeType == Type.Circle)
            {
                Should.updateArea = true;
                return;
            }
            Should.updateModel = true;
            Should.updateArea = true;
        }
    }
    /// <summary>
    /// Rotation in degrees.
    /// </summary>
    public FInt Rotation
    {
        get => TrueRotation;
        set
        {
            if(value == TrueRotation | this.ShapeType == Type.Circle) return;
            
            TrueRotation = value;
            Should.updateModel = true;
            Should.updateArea = true;
            Should.updateNormals = ShapeType == Shape.Type.Convex;
        }
    }

    public Vector2Fi Scale
    {
        get => TrueScale;
        set
        {
            if(value == TrueScale) return;
            if(this.ShapeType == Type.Circle)
            {
                if (value.x != value.y)
                    throw new Exception("In a circle shape the scale vector must have its x equal to y.");

                TrueScale = value;
                Should.updateArea = true;
                
                return;
            }

            TrueScale = value;
            Should.updateModel = true;
            Should.updateArea = true;
        }
    }

    public bool Disposed {get => DisposedActual;}
    Vector2Fi TruePosition;
    Vector2Fi TrueCenterPoint;
    FInt TrueRotation;
    Vector2Fi TrueScale;
    public readonly Shape.Type ShapeType;
    readonly VecMemBlock OriginModel;
    VecMemBlock BakedModel;
    VecMemBlock Normals;
    AABB Area;
    private bool DisposedActual = false;

    (bool updateModel, bool updateArea, bool updateNormals) Should;

    public void BakeShape()
    {
        if(DisposedActual) return;

        if(Should.updateModel)
        {
            UpdateModel(BakedModel.AsSpan(), TruePosition, TrueCenterPoint, TrueRotation, TrueScale);
            Should.updateModel = false;
        }

        if(Should.updateArea)
        {
            UpdateArea();
            Should.updateArea = false;
        }

        if(Should.updateNormals)
        {
            UpdateNormals();
            Should.updateNormals = false;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public void IntersectsInfo(Shape with, ref CollisionResult result)
    {
        short l = (short)this.ShapeType;
        l += (short) with.ShapeType;
        //Convex is 1, circle is 8.
        switch (l)
        {
            //convex -> convex
            case 2:
                this.ConvexConvexIntersectsInfo(with, ref result);
                break;
            //convex <-> circle
            case 9:
                if(this.ShapeType == Shape.Type.Convex)
                    ConvexCircleIntersectsInfo(with, ref result);
                else
                {
                    with.ConvexCircleIntersectsInfo(this, ref result);
                    result.Separation *= -1;
                    result.SeparationDirection *= -1;
                }
                break;
            
            //circle -> circle
            case 16:
                this.CircleCircleIntersectsInfo(with, ref result);
                break;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public void IntersectsInfoWithContactPoint(Shape with, ref CollisionResult result)
    {
        short l = (short)this.ShapeType;
        l += (short) with.ShapeType;
        //Convex is 1, circle is 8.
        switch (l)
        {
            //convex -> convex
            case 2:
                this.ConvexConvexWithContact(with, ref result);
                break;
            //convex <-> circle
            case 9:
                if(this.ShapeType == Shape.Type.Convex)
                    ConvexCircleWithContact(with, ref result);
                else
                {
                    with.ConvexCircleWithContact(this, ref result);
                    result.Separation *= -1;
                    result.SeparationDirection *= -1;
                }
                break;
            
            //circle -> circle
            case 16:
                this.CircleCircleWithContact(with, ref result);
                break;
        }
    }

    private void UpdateModel (Span<Vector2Fi> baked, Vector2Fi pos, Vector2Fi center, FInt rot, Vector2Fi scale)
    {
        var origin = OriginModel.AsSpan();
        for(int i = 0; i < origin.Length; ++i)
        {
            Vector2Fi curr = origin[i] * scale;
            baked[i] = Vector2Fi.RotateVec(curr, center, rot) + pos + TrueCenterPoint;
        }
    }

    private void UpdateArea()
    {
        if(ShapeType == Shape.Type.Circle)
        {
            Vector2Fi center = TruePosition + TrueCenterPoint;
            Area.TopLeft = center - TrueScale;
            Area.BottomRight = center + TrueScale;
            return;
        }

        Vector2Fi low = new Vector2Fi(FInt.MaxValue, FInt.MaxValue);
        Vector2Fi high = new Vector2Fi(FInt.MinValue, FInt.MinValue);

        //dictatorial way of looping, very unclear, very irresponsable, very fast.

        Span<Vector2Fi> baked = BakedModel.AsSpan();

        for(int i = 0; i < baked.Length; ++i)
        {
            Vector2Fi curr = baked[i];
            low = new Vector2Fi(
                DeterministicMath.Min(curr.x, low.x),
                DeterministicMath.Min(curr.y, low.y)
            );
            high = new Vector2Fi(
                DeterministicMath.Max(curr.x, high.x),
                DeterministicMath.Max(curr.x, high.x)
            );
        }

        Area.TopLeft = low;
        Area.BottomRight = high;
    }

    private void UpdateNormals()
    {
        int len = BakedModel.Length - 1;

        Vector2Fi p1, p2;
        FInt normalx, normaly;
        
        for(int i = 0; i< len; ++i)
        {
            p1 = BakedModel[i];
            p2 = BakedModel[i+1];

            normalx = -(p2.y - p1.y);
                
            normaly = p2.x - p1.x;

            Normals[i] = new Vector2Fi(normalx, normaly).Normalized();
        }

        p1 = BakedModel[len];
        p2 = BakedModel[0];

        normalx = -(p2.y - p1.y);
                
        normaly = p2.x - p1.x;

        Normals[len] = new Vector2Fi(normalx, normaly).Normalized();

        //NormalsAction = DoNothing;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    [SkipLocalsInit]
    private void ConvexConvexIntersectsInfo(Shape poly, ref CollisionResult result)
    {
        result.Intersects = false;

        var mA = this.BakedModel.AsSpan();
        var mB = poly.BakedModel.AsSpan();

        Vector2Fi aPosition = TruePosition + TrueCenterPoint;

        int aLength = mA.Length;
        int bLength = mB.Length;

        #region Bring vertices close to Vector2.ZERO to prevent overflow.

        Span<Vector2Fi> a = stackalloc Vector2Fi[aLength];
        Span<Vector2Fi> b = stackalloc Vector2Fi[bLength];

        for(int i = 0; i< aLength; ++i)
        {
            a[i] = mA[i] - aPosition;
        }

        for(int i = 0; i< bLength; ++i)
        {
            b[i] = mB[i] - aPosition;
        }

        #endregion

        Vector2Fi vector = new Vector2Fi();

        FInt distance = FInt.MaxValue;

        FInt minA, maxA, minB, maxB;
        
        Vector2Fi normal;

        for(int polyi = 0; polyi < 2; ++polyi)
        {
            int polyLength = polyi == 0 ? aLength : bLength;
            Span<Vector2Fi> normals = polyi == 0 ? this.Normals.AsSpan() : poly.Normals.AsSpan();

            for(int i1 = 0; i1 < polyLength; ++i1)
            {
                //int i2 = (i1 + 1) % polygon.Length;

                normal = normals[i1];
                
                minA = FInt.MaxValue;
                maxA = FInt.MinValue;

                
                //Projects verts for poly 'a' for min max.
                for(int ai = 0; ai < aLength; ++ai)
                {
                    FInt projected = Vector2Fi.DotProduct(normal, a[ai]);

                    if( projected < minA ) minA = projected;
                    if( projected > maxA ) maxA = projected;
                }

                //Projects verts for poly 'b' for min max.
                minB = FInt.MaxValue;
                maxB = FInt.MinValue;
                for(int bi = 0; bi < bLength; ++bi)
                {
                    FInt projected = Vector2Fi.DotProduct(normal, b[bi]);

                    if( projected < minB ) minB = projected;
                    if( projected > maxB ) maxB = projected;
                }

                if(maxA < minB | maxB < minA) return;

                //FInt distMin = DeterministicMath.Min(maxA, maxB) - DeterministicMath.Max(minA, minB);
                FInt distMin = maxB - minA;

                FInt distMinAbs = DeterministicMath.Abs(distMin);

                if (distMinAbs < distance)
                {
                    vector = normal;

                    distance = distMinAbs;
                }
            }
        }

        FInt factor;

        factor.RawValue = 4140L;

        result.Separation = vector * distance * factor;

        result.SeparationDirection = vector;

        result.Intersects = true;
    }

    private void ConvexCircleIntersectsInfo(Shape circle, ref CollisionResult result)
    {
        //The only way a circle is intersecting a polygon is
        //if it colides with any of the poly's lines
        //OR if the circle itself is inside the polygon

        result.Separation = Vector2Fi.ZERO;

        Vector2Fi polyPos = TruePosition + TrueCenterPoint;

        Span<Vector2Fi> vertsRaw = this.BakedModel.AsSpan();

        int vertsAmount = vertsRaw.Length;

        Span<Vector2Fi> verts = stackalloc Vector2Fi[vertsAmount];

        for(int i = 0; i<vertsAmount; ++i)
        {
            verts[i] = vertsRaw[i] - polyPos;
        }

        Vector2Fi circlePos = circle.TruePosition + circle.TrueCenterPoint - polyPos;

        FInt circleArea = circle.TrueScale.x;

        FInt circleAreaSquared = circleArea * circleArea;

        for(int i12 = 0; i12 < 2; ++i12)
        {
            FInt lowestDistanceSqr = FInt.MaxValue;

            Vector2Fi lineColPoint = new Vector2Fi();

            for(int i1 = 0; i1 < vertsAmount; ++i1)
            {
                int i2 = (i1 + 1) % vertsAmount;

                Vector2Fi colPoint;

                FInt distSquared = Vector2Fi.LinePointDistSqr(verts[i1], verts[i2], circlePos, out colPoint);

                if(distSquared < lowestDistanceSqr)
                {
                    lineColPoint = colPoint;
                    lowestDistanceSqr = distSquared;
                }
            }

            bool IsInside = PointInConvexPolygon(circlePos, verts);

            if(lowestDistanceSqr > circleAreaSquared && !IsInside)
            {
                //In case one interation passed, it did intersect.
                result.Intersects = i12 == 1;
                return;
            }

            if(IsInside)
            {
                //The direction from the line to the circle middle.
                var direction = circlePos - lineColPoint;

                var dir = direction.Normalized();

                result.Separation += dir * (circleArea + DeterministicMath.Sqrt(lowestDistanceSqr));

                result.SeparationDirection = dir;
            }
            else
            {

                //The direction from the circle middle to the line.
                var direction = lineColPoint - circlePos;

                var dir = direction.Normalized();

                result.Separation += dir * (circleArea - DeterministicMath.Sqrt(lowestDistanceSqr));

                result.SeparationDirection = dir;
            }

            circlePos -= result.Separation;
        }

        result.Intersects = true;
    }

    private void CircleCircleIntersectsInfo (Shape circle, ref CollisionResult result)
    {
        Vector2Fi pos1 = Vector2Fi.ZERO;
        FInt area1 = this.TrueScale.x;

        Vector2Fi pos2 = circle.TruePosition + circle.TrueCenterPoint - (this.TruePosition + TrueCenterPoint);
        FInt area2 = circle.TrueScale.x;

        FInt areaTotal = area1 + area2;

        FInt distanceSqr = Vector2Fi.DistanceSquared(pos1, pos2);

        bool intersects = false;

        if(distanceSqr <= areaTotal * areaTotal)
        {
            var normalized = pos2.Normalized();

            intersects = true;

            result.Separation = normalized * (DeterministicMath.Sqrt(distanceSqr) - areaTotal);

            result.SeparationDirection = normalized;
        }
        
        result.Intersects = intersects;
    }

    private static bool PointInConvexPolygon(Vector2Fi testPoint, Span<Vector2Fi> polygon)
    {
        //From: https://stackoverflow.com/questions/1119627/how-to-test-if-a-point-is-inside-of-a-convex-polygon-in-2d-integer-coordinates

        //n>2 Keep track of cross product sign changes
        var pos = 0;
        var neg = 0;

        for (var i = 0; i < polygon.Length; i++)
        {
            //If point is in the polygon
            if (polygon[i] == testPoint) break;

            //Form a segment between the i'th point
            var x1 = polygon[i].x;
            var y1 = polygon[i].y;

            //And the i+1'th, or if i is the last, with the first point
            var i2 = (i+1)%polygon.Length;

            var x2 = polygon[i2].x;
            var y2 = polygon[i2].y;

            var x = testPoint.x;
            var y = testPoint.y;

            //Compute the cross product
            var d = (x - x1)*(y2 - y1) - (y - y1)*(x2 - x1);

            if (d > 0) pos++;
            if (d < 0) neg++;

            //If the sign changes, then point is outside
            if (pos > 0 && neg > 0)
                return false;
        }

        //If no change in direction, then on same side of all segments, and thus inside
        return true;
    }

    private void ConvexConvexWithContact(Shape poly, ref CollisionResult result)
    {
        result.Intersects = false;
        result.ContactCount = 0;

        var mA = this.BakedModel.AsSpan();
        var mB = poly.BakedModel.AsSpan();

        Vector2Fi aPosition = TruePosition;

        int aLength = mA.Length;
        int bLength = mB.Length;

        #region Bring vertices close to Vector2.ZERO to prevent overflow.

        Span<Vector2Fi> a = stackalloc Vector2Fi[aLength];
        Span<Vector2Fi> b = stackalloc Vector2Fi[bLength];

        for(int i = 0; i< aLength; ++i)
        {
            a[i] = mA[i] - aPosition;
        }

        for(int i = 0; i< bLength; ++i)
        {
            b[i] = mB[i] - aPosition;
        }

        #endregion

        Vector2Fi vector = new Vector2Fi();

        FInt distance = FInt.MaxValue;

        FInt minA, maxA, minB, maxB;
        
        Vector2Fi normal;

        for(int polyi = 0; polyi < 2; ++polyi)
        {
            int polyLength = polyi == 0 ? aLength : bLength;
            Span<Vector2Fi> normals = polyi == 0 ? this.Normals.AsSpan() : poly.Normals.AsSpan();

            for(int i1 = 0; i1 < polyLength; ++i1)
            {
                //int i2 = (i1 + 1) % polygon.Length;

                normal = normals[i1];
                
                minA = FInt.MaxValue;
                maxA = FInt.MinValue;

                
                //Projects verts for poly 'a' for min max.
                for(int ai = 0; ai < aLength; ++ai)
                {
                    FInt projected = Vector2Fi.DotProduct(normal, a[ai]);

                    if( projected < minA ) minA = projected;
                    if( projected > maxA ) maxA = projected;
                }

                //Projects verts for poly 'b' for min max.
                minB = FInt.MaxValue;
                maxB = FInt.MinValue;
                for(int bi = 0; bi < bLength; ++bi)
                {
                    FInt projected = Vector2Fi.DotProduct(normal, b[bi]);

                    if( projected < minB ) minB = projected;
                    if( projected > maxB ) maxB = projected;
                }

                if(maxA < minB | maxB < minA) return;

                //FInt distMin = DeterministicMath.Min(maxA, maxB) - DeterministicMath.Max(minA, minB);
                FInt distMin = maxB - minA;

                FInt distMinAbs = DeterministicMath.Abs(distMin);

                if (distMinAbs < distance)
                {
                    vector = normal;

                    distance = distMinAbs;
                }
            }
        }

        FInt factor;

        factor.RawValue = 4140L;

        result.Separation = vector * distance * factor;

        result.SeparationDirection = vector;

        result.Intersects = true;

        Vector2Fi contact1 = Vector2Fi.ZERO;
        Vector2Fi contact2 = Vector2Fi.ZERO;
        int contactCount = 0;

        FInt minDistSq = FInt.MaxValue;

        for(int i = 0; i < a.Length; i++)
        {
            Vector2Fi p = a[i];

            for(int j = 0; j < b.Length; j++)
            {
                Vector2Fi va = b[j];
                Vector2Fi vb = b[(j + 1) % b.Length];

                var colAn = Vector2Fi.LinePointColAnalisis(p, va, vb);
                FInt distSq = colAn.distanceSquared;
                Vector2Fi colP = colAn.collisionPoint;
                FInt near = new FInt(){RawValue = 3L};

                if(DeterministicMath.NearlyEqual(distSq, minDistSq, near))
                {
                    if (!Vector2Fi.NearlyEqual(colP, contact1, near) &&
                        !Vector2Fi.NearlyEqual(colP, contact2, near))
                    {
                        contact2 = colP;
                        contactCount = 2;
                    }
                }
                else if(distSq < minDistSq)
                {
                    minDistSq = distSq;
                    contactCount = 1;
                    contact1 = colP;
                }
            }
        }

        for (int i = 0; i < b.Length; i++)
        {
            Vector2Fi p = b[i];

            for (int j = 0; j < a.Length; j++)
            {
                Vector2Fi va = a[j];
                Vector2Fi vb = a[(j + 1) % a.Length];

                var colAn = Vector2Fi.LinePointColAnalisis(p, va, vb);
                FInt distSq = colAn.distanceSquared;
                Vector2Fi colP = colAn.collisionPoint;
                FInt near = new FInt(){RawValue = 3L};

                if (DeterministicMath.NearlyEqual(distSq, minDistSq, near))
                {
                    if (!Vector2Fi.NearlyEqual(colP, contact1, near) &&
                        !Vector2Fi.NearlyEqual(colP, contact2, near))
                    {
                        contact2 = colP;
                        contactCount = 2;
                    }
                }
                else if (distSq < minDistSq)
                {
                    minDistSq = distSq;
                    contactCount = 1;
                    contact1 = colP;
                }
            }
        }

        result.ContactCount = contactCount;
        result.Contact1 = aPosition + contact1;
        result.Contact2 = aPosition + contact2;
    }

    private void ConvexCircleWithContact(Shape circle, ref CollisionResult result)
    {
        result.ContactCount = 0;

        //The only way a circle is intersecting a polygon is
        //if it colides with any of the poly's lines
        //OR if the circle itself is inside the polygon

        result.Separation = Vector2Fi.ZERO;

        Vector2Fi polyPos = TruePosition + TrueCenterPoint;

        Span<Vector2Fi> vertsRaw = this.BakedModel.AsSpan();

        int vertsAmount = vertsRaw.Length;

        Span<Vector2Fi> verts = stackalloc Vector2Fi[vertsAmount];

        for(int i = 0; i<vertsAmount; ++i)
        {
            verts[i] = vertsRaw[i] - polyPos;
        }

        Vector2Fi circlePos = circle.TruePosition + circle.TrueCenterPoint - polyPos;

        FInt circleArea = circle.TrueScale.x;

        FInt circleAreaSquared = circleArea * circleArea;

        for(int i12 = 0; i12 < 2; ++i12)
        {
            FInt lowestDistanceSqr = FInt.MaxValue;

            Vector2Fi lineColPoint = new Vector2Fi();

            for(int i1 = 0; i1 < vertsAmount; ++i1)
            {
                int i2 = (i1 + 1) % vertsAmount;

                Vector2Fi colPoint;

                FInt distSquared = Vector2Fi.LinePointDistSqr(verts[i1], verts[i2], circlePos, out colPoint);

                if(distSquared < lowestDistanceSqr)
                {
                    lineColPoint = colPoint;
                    lowestDistanceSqr = distSquared;
                }
            }

            bool IsInside = PointInConvexPolygon(circlePos, verts);

            if(lowestDistanceSqr > circleAreaSquared && !IsInside)
            {
                //In case one interation passed, it did intersect.
                result.Intersects = i12 == 1;
                return;
            }

            if(IsInside)
            {
                //The direction from the line to the circle middle.
                var direction = circlePos - lineColPoint;

                var dir = direction.Normalized();

                result.Separation += dir * (circleArea + DeterministicMath.Sqrt(lowestDistanceSqr));

                result.SeparationDirection = dir;
            }
            else
            {

                //The direction from the circle middle to the line.
                var direction = lineColPoint - circlePos;

                var dir = direction.Normalized();

                result.Separation += dir * (circleArea - DeterministicMath.Sqrt(lowestDistanceSqr));

                result.SeparationDirection = dir;
            }

            circlePos -= result.Separation;
        }

        result.Intersects = true;

        Vector2Fi circleCenter = circlePos;
        //FInt circleRadius = circle.TrueScale.x;
        Vector2Fi polygonCenter = Vector2Fi.ZERO;
        Span<Vector2Fi> polygonVertices = verts;

        Vector2Fi colPointReal = Vector2Fi.ZERO;

        FInt minDistSq = FInt.MaxValue;

        for(int i = 0; i < polygonVertices.Length; i++)
        {
            Vector2Fi va = polygonVertices[i];
            Vector2Fi vb = polygonVertices[(i + 1) % polygonVertices.Length];

            var res = Vector2Fi.LinePointColAnalisis(circleCenter, va, vb);
            var distSq = res.distanceSquared;
            var contact = res.collisionPoint;

            if(distSq < minDistSq)
            {
                minDistSq = distSq;
                colPointReal = contact;
            }
        }

        result.ContactCount = 1;
        result.Contact1 = colPointReal + polyPos;
    }

    private void CircleCircleWithContact(Shape circle, ref CollisionResult result)
    {
        result.ContactCount = 0;

        Vector2Fi raw1 = this.TruePosition + TrueCenterPoint;

        Vector2Fi pos1 = Vector2Fi.ZERO;
        FInt area1 = this.TrueScale.x;

        Vector2Fi pos2 = circle.TruePosition + circle.TrueCenterPoint - raw1;
        FInt area2 = circle.TrueScale.x;

        FInt areaTotal = area1 + area2;

        FInt distanceSqr = Vector2Fi.DistanceSquared(pos1, pos2);

        bool intersects = false;

        if(distanceSqr <= areaTotal * areaTotal)
        {
            var normalized = pos2.Normalized();

            intersects = true;

            result.Separation = normalized * (DeterministicMath.Sqrt(distanceSqr) - areaTotal);

            result.SeparationDirection = normalized;

            result.ContactCount = 1;

            result.Contact1 = raw1 + result.Separation;
        }
        
        result.Intersects = intersects;
    }

    //reminder. shape model vectors need to be clockwise.
    private Shape(Vector2Fi pos, Vector2Fi center, FInt rotation, Vector2Fi scale, Shape.Type type, Span<Vector2Fi> originalModel)
    {
        if(type == Type.Circle)
        {
            TruePosition = pos;
            TrueCenterPoint = center;
            TrueRotation = rotation;
            TrueScale = scale;
            ShapeType = type;
            OriginModel = VecMemBlock.NullBlock();
            BakedModel = VecMemBlock.NullBlock();
            Normals = VecMemBlock.NullBlock();
            Should = (false, true, false);
            return;
        }

        TruePosition = pos;
        TrueCenterPoint = center;
        TrueRotation = rotation;
        TrueScale = scale;
        ShapeType = type;
        OriginModel = ShapeVecPool.Allocate(originalModel.Length);
        OriginModel.CopyFrom(originalModel, originalModel.Length);
        BakedModel = ShapeVecPool.Allocate(originalModel.Length);
        Normals = ShapeVecPool.Allocate(originalModel.Length);
        Should = (true, true, true);
        //UpdateModel(BakedModel.AsSpan(), TruePosition, TrueCenterPoint, TrueRotation, TrueScale);
        //UpdateArea();
        //UpdateNormals();
    }

    private static Shape NewRaw(Vector2Fi pos, Vector2Fi center, FInt rotation, Vector2Fi scale, Shape.Type type, Span<Vector2Fi> originalModel)
    {
        return new Shape(pos, center, rotation, scale, type, originalModel);
    }

    public static Shape NewConvex(Vector2Fi pos, Vector2Fi center, FInt rotation, Vector2Fi scale, params Vector2Fi[] vertices)
    {
        return new Shape
        (
            pos,
            center,
            rotation,
            scale,
            Shape.Type.Convex,
            vertices.AsSpan()
        );
    }

    public static Shape NewRectangle(Vector2Fi pos, Vector2Fi center, FInt rotation, Vector2Fi sizeRect, Vector2Fi scale)
    {
        Vector2Fi sizeFinal = sizeRect * FInt.Half;

        return new Shape
        (
            pos,
            center,
            rotation,
            scale,
            Shape.Type.Convex,
            stackalloc Vector2Fi[4]
            {
                //From top left to clockwise.
                -sizeFinal,
                new Vector2Fi(sizeFinal.x, -sizeFinal.y),
                sizeFinal,
                new Vector2Fi(-sizeFinal.x, sizeFinal.y)
            }
        );
    }
    /// <summary>
    /// INFO: 'middleVertice' is how far up/down and right/left the middle vertice should be'.
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="center"></param>
    /// <param name="scale"></param>
    /// <param name="length"></param>
    /// <param name="topVertice"></param>
    /// <returns></returns>
    public static Shape NewTriangle(Vector2Fi pos, Vector2Fi center, FInt rotation, Vector2Fi scale, FInt length, Vector2Fi middleVertice)
    {
        //returns
        //    [0]
        // [2]   [1]
        //or
        // [0]   [1]
        //    [2]

        FInt halfWidth = length * FInt.Half;
        FInt halfHeight = middleVertice.y * FInt.Half;

        if(middleVertice.y < 0) return new Shape
        (
            pos,
            center,
            rotation,
            scale,
            Shape.Type.Convex,
            stackalloc Vector2Fi[3]
            {
                new Vector2Fi(halfWidth + middleVertice.x,halfHeight),
                new Vector2Fi(halfWidth, -halfHeight),
                new Vector2Fi(-halfWidth, -halfHeight)
            }
        );

        return new Shape
        (
            pos,
            center,
            rotation,
            scale,
            Shape.Type.Convex,
            stackalloc Vector2Fi[3]
            {
                new Vector2Fi(-halfWidth, -halfHeight),
                new Vector2Fi(halfWidth, -halfHeight),
                new Vector2Fi(halfWidth + middleVertice.x ,halfHeight)
            }
        );
    }

    public static Shape NewCircle(Vector2Fi pos, Vector2Fi center, FInt radius)
    {

        return new Shape
        (
            pos,
            center,
            new FInt(0),
            new Vector2Fi(radius, radius),
            Shape.Type.Circle,
            null
        );
    }

    public void Dispose()
    {
        if(DisposedActual) return;
        Clear();
    }

    private void Clear()
    {
        Should = (false, false, false);
        DisposedActual = true;
        OriginModel.Dispose();
        BakedModel.Dispose();
        Normals.Dispose();
    }

    public struct AABB
    {
        public Vector2Fi TopLeft;
        public Vector2Fi BottomRight;
    }

    public enum Type : short
    {
        Convex = 1,
        Circle = 8
    }
}