using System;
using FHAL.Math;

namespace GJP;

public sealed class CircleShape: Shape
{
    private Vector2Fi _Position;

    public override Vector2Fi Position {
        get
        {
            return _Position;
        }
        set
        {
            _Position = value;

            MoveActive();
        }
    }

    private FInt _Area;

    public FInt Area
    {
        get
        {
            return _Area;
        }
        set
        {
            _Area = value;

            MoveActive();
        }
    }

    long[] GridIdentifier;

    public static CircleShape CreateCircle(Vector2Fi position, FInt area, CollisionAntenna _objectUsingIt)
    {
        CircleShape circle;

        if(ShapeCashe.TryGetCircle(out circle))
        {
            circle._Position = position;
            circle.LastPosition = position;
            circle._Area = area;
            circle.ObjectUsingIt = _objectUsingIt;
            circle.Reuse();

            return circle;
        }

        return new CircleShape(position, area, _objectUsingIt);
    }

    private CircleShape (Vector2Fi position, FInt area, CollisionAntenna _objectUsingIt)
    {
        _Area = area;

        _Position = position;

        LastPosition = position;

        ObjectUsingIt = _objectUsingIt;
    }

    public override sealed Vector2Fi GetRange()
    {
        return new Vector2Fi(Area, Area);
    }

    public override long[] GetGridIdentifier()
    {
        return GridIdentifier;
    }

    public override void SetGridIdentifier(long[] newValue)
    {
        GridIdentifier = newValue;
    }

    public bool CircleIntersects (CircleShape circle)
    {
        Vector2Fi pos1 = Vector2Fi.ZERO;
        FInt area1 = Area;

        Vector2Fi pos2 = circle.Position - Position;
        FInt area2 = circle.Area;

        return Vector2Fi.InRange(pos1, pos2, area1 + area2);
    }

    public void CircleIntersectsInfo(CircleShape circle, ref CollisionResult result)
    {
        Vector2Fi pos1 = Vector2Fi.ZERO;
        FInt area1 = Area;

        Vector2Fi pos2 = circle.Position - Position;
        FInt area2 = circle.Area;

        FInt areaTotal = area1 + area2;

        FInt distanceSqr = Vector2Fi.DistanceSquared(pos1, pos2);



        if(distanceSqr <= areaTotal * areaTotal)
        {
            var normalized = pos2.Normalized();

            result.Intersects =  true;

            result.Separation = normalized * (DeterministicMath.Sqrt(distanceSqr) - areaTotal);

            return;
        }
        
        result.Intersects = false;
    }

    public bool PolyIntersects(ConvexPolygon poly)
    {
        //The only way a circle is intersecting a polygon is
        //if it colides with any of the poly's lines
        //OR if the circle itself is inside the polygon

        Vector2Fi polyPos = poly.Position;

        Vector2Fi[] vertsRaw = poly.GetModel();

        int vertsAmount = vertsRaw.Length;

        Span<Vector2Fi> verts = stackalloc Vector2Fi[vertsAmount];

        for(int i = 0; i<vertsAmount; ++i)
        {
            verts[i] = vertsRaw[i] - polyPos;
        }

        Vector2Fi circlePos = Position - polyPos;

        FInt circleArea = Area;

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

    public void PolyIntersectsInfo (ConvexPolygon poly, ref CollisionResult result)
    {
        //The only way a circle is intersecting a polygon is
        //if it colides with any of the poly's lines
        //OR if the circle itself is inside the polygon

        result.Separation = Vector2Fi.ZERO;

        Vector2Fi polyPos = poly.Position;

        Vector2Fi[] vertsRaw = poly.GetModel();

        int vertsAmount = vertsRaw.Length;

        Vector2Fi[] verts = new Vector2Fi[vertsAmount];

        for(int i = 0; i<vertsAmount; ++i)
        {
            verts[i] = vertsRaw[i] - polyPos;
        }

        Vector2Fi circlePos = Position - polyPos;

        FInt circleArea = Area;

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

            FInt factor = new FInt();
            factor.RawValue = 4150;

            if(IsInside)
            {
                //The direction from the circle to the line.
                var direction = lineColPoint - circlePos;

                var dir = direction.Normalized();

                result.Separation += dir * (circleArea + DeterministicMath.Sqrt(lowestDistanceSqr)) * factor;

                result.SeparationDirection = dir;
            }
            else
            {
                //The direction from the line to the circle middle.
                var direction =  circlePos - lineColPoint;

                var dir = direction.Normalized();

                result.Separation += dir * (circleArea - DeterministicMath.Sqrt(lowestDistanceSqr)) * factor;

                result.SeparationDirection = dir;
            }

            circlePos += result.Separation;
        }

        result.Intersects = true;
    }

    protected override void Dispose(bool disposing)
    {
        if(disposing)
        {
            ShapeCashe.CasheCircle(this);
        }
        else
        {
            Disposed = false;
        }
    }
}