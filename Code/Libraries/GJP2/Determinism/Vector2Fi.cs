using System;
using System.Drawing;
using System.Runtime.InteropServices;

///<summary>
///Deterministic Vector2D.
///</summary>

[StructLayout(LayoutKind.Sequential)]
public readonly struct Vector2Fi
{
    public static readonly Vector2Fi ZERO;

    static Vector2Fi ()
    {
        ZERO = new Vector2Fi(0,0);
    }

    public readonly FInt x,y;

    public FInt this[int index]
    {
        get
        {
            switch(index)
            {
                case 0: return x;
                case 1: return y;
            }

            throw new IndexOutOfRangeException();
        }
    }

    public Vector2Fi ()
    {
        x = (FInt) 0;
        y = (FInt) 0;
    }

    public Vector2Fi(FInt x_, FInt y_)
    {
        x=x_;
        y=y_;
    }

    public Vector2Fi(int x_, int y_)
    {
        x = (FInt)x_;
        y = (FInt)y_;
    }

    public Vector2Fi(int x_, FInt y_)
    {
        x=(FInt)x_;
        y=y_;
    }

    public Vector2Fi(FInt x_, int y_)
    {
        x=x_;
        y= (FInt)y_;
    }

    public Vector2Fi Create()
    {
        return new Vector2Fi(FInt.Create(0), FInt.Create(0));
    }

    public static Vector2Fi Lerp (Vector2Fi v1, Vector2Fi v2, FInt t)
    {
        FInt x = DeterministicMath.Lerp(v1.x, v2.x, t);
        FInt y = DeterministicMath.Lerp(v1.y, v2.y, t);

        return new Vector2Fi(x, y);
    }

    public Vector2Fi Lerp (Vector2Fi v2, FInt t)
    {
        FInt x = DeterministicMath.Lerp(this.x, v2.x, t);
        FInt y = DeterministicMath.Lerp(this.y, v2.y, t);

        return new Vector2Fi(x, y);
    }

    //The direction of angle of v1 pointing at v2
    public Vector2Fi AngleVector (Vector2Fi v1, Vector2Fi v2)
    {
        return (v2 - v1).Normalized();
    }

    public static FInt DotProduct (Vector2Fi normal, Vector2Fi pt2)
    {
        //FInt
        //x = normal.x * pt2.x,
        //y = normal.y * pt2.y;
        return normal.x * pt2.x + normal.y * pt2.y;
    }

    public static bool InRange(Vector2Fi v1, Vector2Fi v2, FInt range)
    {
        FInt dx = v1.x - v2.x;
        FInt dy = v1.y - v2.y;

        return dx*dx + dy*dy <=  range * range;
    }

    /// <summary>
    /// Same thing as Hypot2.
    /// </summary>
    public static FInt DistanceSquared(Vector2Fi v1, Vector2Fi v2)
    {
        FInt dx = v1.x - v2.x;
        FInt dy = v1.y - v2.y;

        return dx*dx + dy*dy;
    }

    public static FInt Distance(Vector2Fi v1, Vector2Fi v2)
    {
        FInt dx = v1.x - v2.x;
        FInt dy = v1.y - v2.y;

        return DeterministicMath.Sqrt(dx*dx + dy*dy);
    }
    /// <summary>
    /// Distance of line against a 'point'.
    /// </summary>
    public static FInt LinePointDistSqr(Vector2Fi linePt1, Vector2Fi linePt2, Vector2Fi point)
    {
        //From: https://stackoverflow.com/questions/1073336/circle-line-segment-collision-detection-algorithm

        var AC = point - linePt1;
        //Only way AB is 0 is if both line points are in the same place, AKA it's a point
        var AB = linePt2 - linePt1;

        var ZERO_ZERO = new FInt(0);

        //Is 0?
        bool ABx0 = AB.x == ZERO_ZERO;
        bool ABy0 = AB.y == ZERO_ZERO;

        //If it's a point them just return its distance
        if(ABx0 && ABy0) return DistanceSquared(linePt1, point);

        // Get point D by taking the projection of AC onto AB then adding the offset of linePt1
        //There's no chance that dotAB is 0
        var dotAB = Vector2Fi.DotProduct(AB, AB);
        var dotACAB = Vector2Fi.DotProduct(AC, AB);

        FInt kp = dotACAB / dotAB;

        var D = AB * kp + linePt1;

        var AD = D - linePt1;
        // D might not be on AB so calculate k of D down AB (aka solve AD = k * AB)
        // We can use either component, but choose larger value to eliminate the chance of dividing by zero
        // since the 'ABx0 && ABy0' if statement made sure one of the values of AB is not 0.
        FInt k = DeterministicMath.Abs(AB.x) > DeterministicMath.Abs(AB.y) ? AD.x / AB.x : AD.y / AB.y;

        // Check if D is off either end of the line segment

        if (k <= 0) {
            return DistanceSquared(point, linePt1);
        } else if (k >= 1) {
            return DistanceSquared(point, linePt2);
        }

        return DistanceSquared(point, D);
    }

    public static FInt LinePointDistSqr(Vector2Fi linePt1, Vector2Fi linePt2, Vector2Fi point, out Vector2Fi mesurementPoint)
    {
        //From: https://stackoverflow.com/questions/1073336/circle-line-segment-collision-detection-algorithm

        var AC = point - linePt1;
        //Only way AB is 0 is if both line points are in the same place, AKA it's a point
        var AB = linePt2 - linePt1;

        var ZERO_ZERO = new FInt(0);

        //Is 0?
        bool ABx0 = AB.x == ZERO_ZERO;
        bool ABy0 = AB.y == ZERO_ZERO;

        //If it's a point them just return its distance
        if(ABx0 && ABy0)
        {
            mesurementPoint = linePt1;
            return DistanceSquared(linePt1, point);
        }

        // Get point D by taking the projection of AC onto AB then adding the offset of linePt1
        //There's no chance that dotAB is 0
        var dotAB = Vector2Fi.DotProduct(AB, AB);
        var dotACAB = Vector2Fi.DotProduct(AC, AB);

        FInt kp = dotACAB / dotAB;

        var D = AB * kp + linePt1;

        var AD = D - linePt1;
        // D might not be on AB so calculate k of D down AB (aka solve AD = k * AB)
        // We can use either component, but choose larger value to eliminate the chance of dividing by zero
        // since the 'ABx0 && ABy0' if statement made sure one of the values of AB is not 0.
        FInt k = DeterministicMath.Abs(AB.x) > DeterministicMath.Abs(AB.y) ? AD.x / AB.x : AD.y / AB.y;

        // Check if D is off either end of the line segment

        if (k <= 0)
        {
            mesurementPoint = linePt1;
            return DistanceSquared(point, linePt1);
        }
        else if (k >= 1)
        {
            mesurementPoint = linePt2;
            return DistanceSquared(point, linePt2);
        }

        mesurementPoint = D;
        return DistanceSquared(point, D);
    }

    public static (FInt distanceSquared, Vector2Fi collisionPoint) LinePointColAnalisis(
        Vector2Fi point, Vector2Fi vertA, Vector2Fi vertB
        )
    {
        Vector2Fi colPoint;
        FInt distanceSquared;

        Vector2Fi ab = vertB - vertA;
        Vector2Fi ap = point - vertA;

        //If it's a point then just return a random segment vs the point
        if(ab == ZERO)
        {
            colPoint = vertA;
            goto end;
        }

        FInt proj = DotProduct(ap, ab);
        FInt abLenSq = ab.LengthSqr();
        FInt d = proj / abLenSq;

        if(d <= new FInt(0L))
        {
            colPoint = vertA;
        }
        else if(d >= FInt.OneF)
        {
            colPoint = vertB;
        }
        else
        {
            colPoint = vertA + ab * d;
        }

        end:;
        distanceSquared = Vector2Fi.DistanceSquared(point, colPoint);
        return (distanceSquared, colPoint);
    }

    public static bool NearlyEqual(Vector2Fi a, Vector2Fi b, FInt thresold)
    {
        return DeterministicMath.NearlyEqual(a.x, b.x, thresold) & DeterministicMath.NearlyEqual(a.y, b.y, thresold);
    }

    public static Vector2Fi RotateVec(Vector2Fi toRotate, Vector2Fi center, FInt degrees)
    {
        FInt sin = DeterministicMath.SinD(degrees);
        FInt cos = DeterministicMath.CosD(degrees);
 
        // Translate point back to origin
        FInt x = toRotate.x - center.x;
        FInt y = toRotate.y - center.y;
 
        // Rotate point
        FInt xnew = x * cos - y * sin;
        FInt ynew = x * sin + y * cos;
     
        // Translate point back
        Vector2Fi newPoint = new Vector2Fi(xnew + center.x, ynew + center.y);
        return newPoint;
    }
    /// <summary>
    /// Rotates with a point angle value. (1 pointAngle = pi, 0.5 pointAngle = pi/2)
    /// </summary>
    /// <param name="toRotate"></param>
    /// <param name="center"></param>
    /// <param name="pointAngle"></param>
    /// <returns></returns>
    public static Vector2Fi RotateVecP(Vector2Fi toRotate, Vector2Fi center, FInt pointAngle)
    {
        FInt sin = DeterministicMath.SinPoint(pointAngle);
        FInt cos = DeterministicMath.CosPoint(pointAngle);
 
        // Translate point back to origin
        FInt x = toRotate.x - center.x;
        FInt y = toRotate.y - center.y;
 
        // Rotate point
        FInt xnew = x * cos - y * sin;
        FInt ynew = x * sin + y * cos;
     
        // Translate point back
        Vector2Fi newPoint = new Vector2Fi(xnew + center.x, ynew + center.y);
        return newPoint;
    }

    public FInt Length ()
    {
        return DeterministicMath.Sqrt(x*x + y*y);
    }
    public FInt LengthSqr()
    {
        return x*x + y*y;
    }

    /// <summary>
    /// Faster length that can process numbers up to 900 on x and y.
    /// </summary>
    /// <returns></returns>
    public FInt OptLength ()
    {
        return DeterministicMath.Sqrt(x*x + y*y);
    }

    public Vector2Fi Normalized()
    {
        bool xZero, yZero;

        xZero = x.RawValue == 0;
        yZero = y.RawValue == 0;

        if(DeterministicMath.Abs(x) < 900 & DeterministicMath.Abs(y) < 900)
            return OptNormalized();

        //If below assures normalize doesn't calculate an answer it already has.
        if(xZero | yZero)
        {
            var o = new FInt(1);
            if (xZero & yZero) return ZERO;
            if (xZero) return new Vector2Fi(new FInt(), y < 0? -o:o);
            return new Vector2Fi(x < 0? -o:o, new FInt());
        }

        
        var length = (this).Length();

        return this / length;
    }
    /// <summary>
    /// Faster normalized that can process numbers up to 900 on x and y.
    /// </summary>
    /// <returns></returns>
    public Vector2Fi OptNormalized()
    {
        bool xZero, yZero;

        xZero = x.RawValue == 0;
        yZero = y.RawValue == 0;

        //If below assures normalize doesn't calculate an answer it already has.
        if(xZero | yZero)
        {
            var o = new FInt(1);
            if (xZero & yZero) return ZERO;
            if (xZero) return new Vector2Fi(new FInt(), y < 0? -o:o);
            return new Vector2Fi(x < 0? -o:o, new FInt());
        }

        FInt length = (this).OptLength();

        return this / length;
    }

    public Vector2Fi Abs()
    {
        return new Vector2Fi(DeterministicMath.Abs(x), DeterministicMath.Abs(y));
    }

    public override string ToString()
    {
        return x.ToString() + ':' + y.ToString();
    }

    public static Vector2Fi Parse (string s)
    {
        var arr = s.Split(':');

        return new Vector2Fi(FInt.Parse(arr[0]), FInt.Parse(arr[1]));
    }


    public static Vector2Fi operator + (Vector2Fi v1, Vector2Fi v2)
    {
        return new Vector2Fi (v1.x + v2.x, v1.y + v2.y);
    }

    public static Vector2Fi operator - (Vector2Fi v1, Vector2Fi v2)
    {
        return new Vector2Fi (v1.x - v2.x, v1.y - v2.y);
    }

    public static Vector2Fi operator * (Vector2Fi v1, Vector2Fi v2)
    {
        return new Vector2Fi (v1.x * v2.x, v1.y * v2.y);
    }

    public static Vector2Fi operator * (Vector2Fi v1, FInt d2)
    {
        return new Vector2Fi (v1.x * d2, v1.y * d2);
    }

    public static Vector2Fi operator * (Vector2Fi v1, int d2)
    {
        return new Vector2Fi (v1.x * d2, v1.y * d2);
    }


    public static Vector2Fi operator / (Vector2Fi v1, Vector2Fi v2)
    {
        return new Vector2Fi (v1.x / v2.x, v1.y / v2.y);
    }

    public static Vector2Fi operator / (Vector2Fi v1, FInt d2)
    {
        return new Vector2Fi (v1.x / d2, v1.y / d2);
    }

    public static Vector2Fi operator / (Vector2Fi v1, int d2)
    {
        return new Vector2Fi (v1.x / d2, v1.y / d2);
    }

    public static Vector2Fi operator % (Vector2Fi v1, Vector2Fi v2)
    {
        FInt tempX;
        FInt tempY;

        tempX.RawValue = v1.x.RawValue % v2.x.RawValue;
        tempY.RawValue = v1.y.RawValue % v2.y.RawValue;

        return new Vector2Fi(tempX, tempY);
    }

    public static Vector2Fi operator % (Vector2Fi v1, int d2)
    {
        FInt tempX;
        FInt tempY;

        tempX.RawValue = v1.x.RawValue % d2;
        tempY.RawValue = v1.y.RawValue % d2;

        return new Vector2Fi(tempX, tempY);
    }

    public static Vector2Fi operator << (Vector2Fi v1, int d2)
    {
        return new Vector2Fi(v1.x << d2, v1.y << d2);
    }

    public static Vector2Fi operator >> (Vector2Fi v1, int d2)
    {
        return new Vector2Fi(v1.x >> d2, v1.y >> d2);
    }

    public static Vector2Fi operator - (Vector2Fi v1)
    {
        return new Vector2Fi(-v1.x, -v1.y);
    }

    public static bool operator == (Vector2Fi v1, Vector2Fi v2)
    {
        return v1.x == v2.x && v1.y == v2.y;
    }

    public static bool operator != (Vector2Fi v1, Vector2Fi v2)
    {
        return v1.x != v2.x && v1.y != v2.y;
    }

    public static bool operator == (Vector2Fi v1, FInt d2)
    {
        return v1.x == d2 && v1.y == d2;
    }

    public static bool operator != (Vector2Fi v1, FInt d2)
    {
        return v1.x != d2 && v1.y != d2;
    }

    public override bool Equals (object o)
    {
        if(o is Vector2Fi l) return l == this;
        return false;
    }

    public override int GetHashCode()
    {
        long hx = x.RawValue;
        long hy = y.RawValue;

        if(x > int.MaxValue | x < int.MinValue) hx >>= 32;
        if(y > int.MaxValue | y < int.MinValue) hy >>= 32;

        return (int)hx + (int)hx;
    }

    public PointF ToPoint ()
    {
        return new PointF(x.ToFloat(), y.ToFloat());
    }
}