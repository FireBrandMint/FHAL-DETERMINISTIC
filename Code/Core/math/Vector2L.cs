using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FHAL.Math;
public struct Vector2L
{
    public readonly long x, y;

    public Vector2L(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public Vector2L(long x, long y)
    {
        this.x = x;
        this.y = y;
    }

    public static Vector2L operator + (Vector2L v1, Vector2L v2)
    {
        return new Vector2L (v1.x + v2.x, v1.y + v2.y);
    }

    public static Vector2L operator - (Vector2L v1, Vector2L v2)
    {
        return new Vector2L (v1.x - v2.x, v1.y - v2.y);
    }

    public static Vector2L operator * (Vector2L v1, Vector2L v2)
    {
        return new Vector2L (v1.x * v2.x, v1.y * v2.y);
    }

    public static Vector2L operator * (Vector2L v1, int d2)
    {
        return new Vector2L (v1.x * d2, v1.y * d2);
    }

    public static Vector2L operator / (Vector2L v1, Vector2L v2)
    {
        return new Vector2L (v1.x / v2.x, v1.y / v2.y);
    }

    public static Vector2L operator / (Vector2L v1, int d2)
    {
        return new Vector2L (v1.x / d2, v1.y / d2);
    }

    public static Vector2L operator % (Vector2L v1, Vector2L v2)
    {
        long tempX;
        long tempY;

        tempX = v1.x % v2.x;
        tempY = v1.y % v2.y;

        return new Vector2L(tempX, tempY);
    }

    public static Vector2L operator % (Vector2L v1, int d2)
    {
        long tempX;
        long tempY;

        tempX = v1.x % d2;
        tempY = v1.y % d2;

        return new Vector2L(tempX, tempY);
    }

    public static Vector2L operator << (Vector2L v1, int d2)
    {
        return new Vector2L(v1.x << d2, v1.y << d2);
    }

    public static Vector2L operator >> (Vector2L v1, int d2)
    {
        return new Vector2L(v1.x >> d2, v1.y >> d2);
    }

    public static Vector2L operator - (Vector2L v1)
    {
        return new Vector2L(-v1.x, -v1.y);
    }

    public static bool operator == (Vector2L v1, Vector2L v2)
    {
        return v1.x == v2.x & v1.y == v2.y;
    }

    public static bool operator != (Vector2L v1, Vector2L v2)
    {
        return v1.x != v2.x & v1.y != v2.y;
    }

    public static bool operator == (Vector2L v1, int d2)
    {
        return v1.x == d2 & v1.y == d2;
    }

    public static bool operator != (Vector2L v1, int d2)
    {
        return v1.x != d2 & v1.y != d2;
    }

    public static bool operator > (Vector2L v1, Vector2L v2)
    {
        return v1.x > v2.x & v1.y > v2.y;
    }

    public static bool operator < (Vector2L v1, Vector2L v2)
    {
        return v1.x < v2.x & v1.y < v2.y;
    }

    public static bool operator >= (Vector2L v1, Vector2L v2)
    {
        return v1.x >= v2.x & v1.y >= v2.y;
    }

    public static bool operator <= (Vector2L v1, Vector2L v2)
    {
        return v1.x <= v2.x & v1.y <= v2.y;
    }

    public static explicit operator Vector2Fi( Vector2L src )
    {
        return new Vector2Fi((FInt)src.x, (FInt)src.y);
    }

    public Vector2L Parse (Vector2Fi src)
    {
        return new Vector2L((int)src.x, (int)src.y);
    }

    public override bool Equals (object o)
    {
        if(o is Vector2L l) return l == this;
        return false;
    }

    public override int GetHashCode()
    {
        long hx = x;
        long hy = y << 4;

        long final = hx + hy;

        final <<= 32;
        final >>= 32;

        return (int)final;
    }
}