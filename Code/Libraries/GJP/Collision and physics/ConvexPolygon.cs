using System;
using System.Runtime.CompilerServices;
using FHAL.Math;

namespace GJP;

public sealed class ConvexPolygon : Shape
{
    bool Updated = false;

    bool NormalsUpdated = false;

    bool RangeUpdated = false;

    long[] GridIdentifier;

    Vector2Fi[] OriginalModel;

    Vector2Fi _pos;

    public override sealed Vector2Fi Position
    {
        get{return _pos;} 
        set
        {
            Updated = Updated && value == _pos;

            //if(!Updated) ModelAction = UpdateModel;

            _pos = value;

            UpdateModel();

            /*if(value != _pos)
            {
                _pos = value;
                UpdateModel();
            }*/
        }
    }

    Vector2Fi _scale = new Vector2Fi(1,1);

    public Vector2Fi Scale
    {
        get => _scale;
        set
        {
            Updated = Updated && value == _scale;

            _scale = value;

            UpdateModel();
        }
    }

    FInt _rot;

    ///<summary>
    ///Rotation in degrees.
    ///</summary>
    public FInt Rotation
    {
        get{return _rot;} 
        set 
        {
            Updated = Updated && value == _rot;
            NormalsUpdated = NormalsUpdated && value == _rot;
            RangeUpdated = RangeUpdated && value == _rot;

            //if(!Updated) ModelAction = UpdateModel;
            //if(!NormalsUpdated) NormalsAction = UpdateNormals;

            _rot = value;

            UpdateModel();
            UpdateNormals();

            /*if(value != _rot)
            {
                _rot = value;

                UpdateModel();
                UpdateNormals();
            }*/
        }
    }

    Vector2Fi[] ResultModel;

    Vector2Fi[] Normals;

    public Vector2Fi Range;

    public static ConvexPolygon CreateRect(Vector2Fi length, Vector2Fi scale, FInt rotation, Vector2Fi position, CollisionAntenna objectUsingIt)
    {
        FInt x = length.x * FInt.Half;
        FInt y = length.y * FInt.Half;

        ConvexPolygon poly;

        if(ShapeCashe.TryGetConvex(4, out poly))
        {
            var originalModel = poly.GetOriginalModel();

            originalModel[0] = new Vector2Fi(-x, -y);
            originalModel[1] = new Vector2Fi(-x, y);
            originalModel[2] = new Vector2Fi(x, y);
            originalModel[3] = new Vector2Fi(x, -y);

            poly.Position = position;
            poly.LastPosition = position;
            poly.Scale = scale;
            poly.Rotation = rotation;
            poly.ObjectUsingIt = objectUsingIt;

            poly.Reuse();

            return poly;
        }

        return new ConvexPolygon(
            new Vector2Fi[]
            {
                //top left
                new Vector2Fi(-x, -y),
                //bottom left
                new Vector2Fi(-x, y),
                //bottom right
                new Vector2Fi(x, y),
                //top right
                new Vector2Fi(x, -y),
            },
            position,
            scale,
            rotation,
            objectUsingIt
            );
    }
    
    public static ConvexPolygon CreateTriangle(Vector2Fi length, Vector2Fi scale, FInt rotation, Vector2Fi position, CollisionAntenna objectUsingIt)
    {
        FInt x = length.x * FInt.Half;
        FInt y = length.y * FInt.Half;

        ConvexPolygon poly;

        if(ShapeCashe.TryGetConvex(3, out poly))
        {
            var originalModel = poly.GetOriginalModel();

            originalModel[0] = new Vector2Fi(new FInt(), -y);
            originalModel[1] = new Vector2Fi(-x, y);
            originalModel[2] = new Vector2Fi(x, y);

            poly.Position = position;
            poly.LastPosition = position;
            poly.Scale = scale;
            poly.Rotation = rotation;
            poly.ObjectUsingIt = objectUsingIt;

            poly.Reuse();

            return poly;
        }

        return new ConvexPolygon(
            new Vector2Fi[]
            {
                //top
                new Vector2Fi(new FInt(), -y),
                //bottom left
                new Vector2Fi(-x, y),
                //bottom right
                new Vector2Fi(x, y)
            },
            position,
            scale,
            rotation,
            objectUsingIt
            );
    }

    public ConvexPolygon(Vector2Fi[] model, Vector2Fi position, Vector2Fi scale, FInt rotation, CollisionAntenna _objectUsingIt)
    {
        OriginalModel = model;

        _pos = position;

        LastPosition = position;

        _rot = rotation;

        _scale = scale;

        ResultModel = new Vector2Fi[model.Length];

        Normals = new Vector2Fi[model.Length];

        ObjectUsingIt = _objectUsingIt;

        //ModelAction = UpdateModel;

        //NormalsAction = UpdateNormals;

        UpdateModel();

        UpdateNormals();
    }

    void UpdateModel()
    {
        if (Updated || Disposed) return;

        //solves rotation and scale
        Vector2Fi center = Vector2Fi.ZERO;

        for(int i = 0; i< OriginalModel.Length; ++i)
        {
            Vector2Fi curr = OriginalModel[i] * _scale;

            ResultModel[i] = Vector2Fi.RotateVec(curr, center, _rot);
        }

        //solves range
        UpdateRange();

        //solves position
        for(int i = 0; i< ResultModel.Length; ++i)
        {
            ResultModel[i] = ResultModel[i] + Position;
        }

        MoveActive();

        Updated = true;

        //ModelAction = DoNothing;
    }

    private void UpdateRange()
    {
        if(RangeUpdated) return;

        FInt rangX = FInt.MinValue;
        FInt rangY = FInt.MinValue;

        for(int i = 0; i < ResultModel.Length; ++i)
        {
            Vector2Fi curr = ResultModel[i];

            FInt currX = DeterministicMath.Abs(curr.x);
            FInt currY = DeterministicMath.Abs(curr.y);

            if(rangX < currX) rangX = currX;

            if(rangY < currY) rangY = currY;
        }

        Range = new Vector2Fi(rangX, rangY);

        RangeUpdated = true;
    }

    private void UpdateNormals()
    {
        if(NormalsUpdated || Disposed) return;

        int len = ResultModel.Length - 1;

        Vector2Fi p1, p2;
        FInt normalx, normaly;
        
        for(int i = 0; i< len; ++i)
        {
            p1 = ResultModel[i];
            p2 = ResultModel[i+1];

            normalx = -(p2.y - p1.y);
                
            normaly = p2.x - p1.x;

            Normals[i] = new Vector2Fi(normalx, normaly).Normalized();
        }

        p1 = ResultModel[len];
        p2 = ResultModel[0];

        normalx = -(p2.y - p1.y);
                
        normaly = p2.x - p1.x;

        Normals[len] = new Vector2Fi(normalx, normaly).Normalized();

        NormalsUpdated = true;

        //NormalsAction = DoNothing;
    }

    public Vector2Fi[] GetModel()
    {
        return ResultModel;
    }

    public Vector2Fi[] GetOriginalModel () => OriginalModel;

    public Vector2Fi[] GetNormals()
    {
        return Normals;
    }

    public override sealed Vector2Fi GetRange()
    {
        return Range;
    }

    public override long[] GetGridIdentifier()
    {
        return GridIdentifier;
    }

    public override void SetGridIdentifier(long[] newValue)
    {
        GridIdentifier = newValue;
    }

    public bool PolyIntersects(ConvexPolygon poly)
    {
        #region Bring vertices close to Vector2.ZERO to prevent overflow.

        Vector2Fi aPosition = Position;

        Vector2Fi[] mA = GetModel();
        Vector2Fi[] mB = poly.GetModel();

        int aLength = mA.Length;
        int bLength = mB.Length;

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

        for(int polyi = 0; polyi < 2; ++polyi)
        {
            Span<Vector2Fi> polygon = polyi == 0 ? a : b;

            for(int i1 = 0; i1 < polygon.Length; ++i1)
            {
                int i2 = (i1 + 1) % polygon.Length;

                Vector2Fi normal = polygon[i2] - polygon[i1];
                
                FInt minA = FInt.MaxValue;
                FInt maxA = FInt.MinValue;


                //Projects verts for poly 'a' for min max.
                for(int ai = 0; ai < a.Length; ++ai)
                {
                    FInt projected = Vector2Fi.DotProduct(normal, a[ai]);

                    if( projected < minA ) minA = projected;
                    if( projected > maxA ) maxA = projected;
                }

                //Projects verts for poly 'b' for min max.
                FInt minB = FInt.MaxValue;
                FInt maxB = FInt.MinValue;
                for(int bi = 0; bi < b.Length; ++bi)
                {
                    FInt projected = Vector2Fi.DotProduct(normal, b[bi]);

                    if( projected < minB ) minB = projected;
                    if( projected > maxB ) maxB = projected;
                }

                if(maxA < minB || maxB < minA) goto end;
            }
        }

        return true;

        end:
        return false;
    }

    public void PolyIntersectsInfoSlow(ConvexPolygon poly, ref CollisionResult result)
    {
        result.Intersects = true;

        Vector2Fi[] a = GetModel();
        Vector2Fi[] b = poly.GetModel();

        FInt distance = FInt.MaxValue;

        FInt shortestDist = FInt.MaxValue;

        Vector2Fi vector = new Vector2Fi();

        for(int polyi = 0; polyi < 2; ++polyi)
        {
            Vector2Fi[] polygon = polyi == 0 ? a : b;

            for(int i1 = 0; i1 < polygon.Length; ++i1)
            {
                int i2 = (i1 + 1) % polygon.Length;

                FInt normalx = -(polygon[i2].y - polygon[i1].y);
                
                FInt normaly = polygon[i2].x - polygon[i1].x;

                Vector2Fi normal = new Vector2Fi(normalx, normaly).Normalized();
                
                FInt minA = FInt.MaxValue;
                FInt maxA = FInt.MinValue;


                //Projects verts for poly 'a' for min max.
                for(int ai = 0; ai < a.Length; ++ai)
                {
                    FInt projected = Vector2Fi.DotProduct(normal, a[ai]);

                    if( projected < minA ) minA = projected;
                    if( projected > maxA ) maxA = projected;
                }

                //Projects verts for poly 'b' for min max.
                FInt minB = FInt.MaxValue;
                FInt maxB = FInt.MinValue;
                for(int bi = 0; bi < b.Length; ++bi)
                {
                    FInt projected = Vector2Fi.DotProduct(normal, b[bi]);

                    if( projected < minB ) minB = projected;
                    if( projected > maxB ) maxB = projected;
                }

                if(maxA < minB || maxB < minA) goto doesntIntersect;

                //FInt distMin = DeterministicMath.Min(maxA, maxB) - DeterministicMath.Max(minA, minB);
                FInt distMin = maxB - minA;
                distMin *= -1 + polyi * 2;

                FInt distMinAbs = DeterministicMath.Abs(distMin);

                if (distMinAbs < shortestDist)
                {
                    shortestDist = distMinAbs;
                    distance = distMinAbs;

                    vector = normal;
                }
            }
        }

        result.Separation = vector * distance * -1;

        return;

        doesntIntersect:
        result.Intersects = false;
    }
    public void PolyIntersectsInfo(ConvexPolygon poly, ref CollisionResult result)
    {
        result.Intersects = false;

        var mA = GetModel();
        var mB = poly.GetModel();

        Vector2Fi aPosition = Position;

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

        FInt distance = FInt.MaxValue;

        Vector2Fi vector = new Vector2Fi();

        FInt minA, maxA, minB, maxB;
        
        Vector2Fi normal;

        for(int polyi = 0; polyi < 2; ++polyi)
        {
            Span<Vector2Fi> polygon = polyi == 0 ? a : b;
            Vector2Fi[] normals;

            if(polyi == 0)
            {
                normals = this.GetNormals();
            }
            else
            {
                normals = poly.GetNormals();
            }

            for(int i1 = 0; i1 < polygon.Length; ++i1)
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

                if(maxA < minB || maxB < minA) return;

                //FInt distMin = DeterministicMath.Min(maxA, maxB) - DeterministicMath.Max(minA, minB);
                FInt distMin = maxB - minA;

                FInt distMinAbs = DeterministicMath.Abs(distMin);

                if (distMinAbs < distance)
                {
                    distance = distMinAbs;

                    vector = normal;
                }
            }
        }

        FInt factor;

        factor.RawValue = 4140L;

        result.Separation = vector * distance * factor;

        result.SeparationDirection = vector;

        result.Intersects = true;
    }

    public bool CircleIntersects(CircleShape circle)
    {
        //The only way a circle is intersecting a polygon is
        //if it colides with any of the poly's lines
        //OR if the circle itself is inside the polygon

        Vector2Fi polyPos = Position;

        Vector2Fi[] vertsRaw = GetModel();

        int vertsAmount = vertsRaw.Length;

        Vector2Fi[] verts = new Vector2Fi[vertsAmount];

        for(int i = 0; i<vertsAmount; ++i)
        {
            verts[i] = vertsRaw[i] - polyPos;
        }

        Vector2Fi circlePos = circle.Position - polyPos;

        FInt circleArea = circle.Area;

        FInt circleAreaSquared = circleArea * circleArea;

        bool result = false;

        for(int i1 = 0; i1 < vertsAmount; ++i1)
        {
            int i2 = (i1 + 1) % vertsAmount;

            FInt distSquared = Vector2Fi.LinePointDistSqr(verts[i1], verts[i2], circlePos);

            result = result || distSquared <= circleAreaSquared;
        }

        //If circle is not touching one of the shape's lines,
        //then the only way they intersect is if the circle is inside.
        if(result!)
        {
            return Shape.PointInConvexPolygon(circlePos, verts);
        }

        return result;
    }

    public void CircleIntersectsInfo(CircleShape circle, ref CollisionResult result)
    {
        //The only way a circle is intersecting a polygon is
        //if it colides with any of the poly's lines
        //OR if the circle itself is inside the polygon

        result.Separation = Vector2Fi.ZERO;

        Vector2Fi polyPos = Position;

        Vector2Fi[] vertsRaw = GetModel();

        int vertsAmount = vertsRaw.Length;

        Vector2Fi[] verts = new Vector2Fi[vertsAmount];

        for(int i = 0; i<vertsAmount; ++i)
        {
            verts[i] = vertsRaw[i] - polyPos;
        }

        Vector2Fi circlePos = circle.Position - polyPos;

        FInt circleArea = circle.Area;

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
                result.Intersects = false;
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

    protected override void Dispose(bool disposing)
    {
        if(disposing)
        {
            ShapeCashe.TryCasheConvex(this);
        }
        else
        {
            Disposed = false;

            UpdateModel();

            UpdateNormals();
        }
    }
}