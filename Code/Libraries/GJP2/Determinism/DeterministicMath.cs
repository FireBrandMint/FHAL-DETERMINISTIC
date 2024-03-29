using System;
using System.Runtime.CompilerServices;

namespace FHAL.Math;

public class DeterministicMath
{
    //https://stackoverflow.com/questions/605124/fixed-point-math-in-c

    static DeterministicMath ()
    {
        
    }

    #region PI, DoublePI
    public static FInt PI = FInt.Create( 12868, false ); //PI x 2^12
    public static FInt TwoPIF = PI * 2; //radian equivalent of 260 degrees
    public static FInt PIOver180F = PI / (FInt)180; //PI / 180
    #endregion

    #region Sqrt
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static FInt Sqrt( FInt f, int NumberOfIterations )
    {
        if ( f.RawValue < 0 ) //NaN in Math.Sqrt
            throw new ArithmeticException( "Input Error" );
        if ( f.RawValue == 0 )
            return (FInt)0;
        FInt k = f + FInt.OneF >> 1;
        for ( int i = 0; i < NumberOfIterations; i++ )
            k = ( k + ( f / k ) ) >> 1;

        if ( k.RawValue < 0 ) throw new ArithmeticException( "Overflow" );
            
        return k;
    }

    public static FInt Sqrt( FInt f )
    {
        //9625600001L = 2.350.000 FInt
        if(f.RawValue < 9625600001L)
        {
            FInt result;
            result.RawValue = (long)(sqrtfx12((ulong)f.RawValue));
            return result;
        }

        byte numberOfIterations = 8;

        //0x64000 = 409600L
        if ( f.RawValue > 409600L )
        {
            numberOfIterations = 12;
        
            //0x3e8000 = 4096000L
            if ( f.RawValue > 4096000L )
            numberOfIterations = 16;
        }
        
        //Less than 0 is NaN in Math.Sqrt.
        if ( f.RawValue < 0 ) throw new ArithmeticException( "Input Error" );
        
        if ( f.RawValue == 0 ) return (FInt)0;



        FInt k = f + new FInt(1) >> 1;
        for ( int i = 0; i < numberOfIterations; ++i )
        {
            k = ( k + f / k ) >> 1;
        }

        if ( k.RawValue < 0 ) throw new ArithmeticException( "Overflow" );
        
        return k;
    }
    #endregion

    /// <summary>
    /// Faster sqrt that can process numbers up to 2.350.000 FInt.
    /// </summary>
    /// <returns></returns>
    public static FInt OptSqrt( FInt f )
    {
        FInt result;
        result.RawValue = (long)(sqrtfx12((ulong)f.RawValue));
        return result;
    }

    //from chmike/fpsqrt
    private static ulong sqrtfx12(ulong v)
    {
        ulong t, q, b, r;
        r = v; 
        q = 0;          
        b = 0x40000000UL;
        
        if( r < 0x4000200 )
        {
            while( b != 0x40 )
            {
                t = q + b;
                if( r >= t )
                {
                    r -= t;
                    q = t + b; // equivalent to q += 2*b
                }
                r <<= 1;
                b >>= 1;
            }
            q >>= 10;
            goto end;
        }

        goto cOp;
        end:;
        return q;

        cOp:;

        while( b > 0x40 )
        {
            t = q + b;
            if( r >= t )
            {
                r -= t;
                q = t + b; // equivalent to q += 2*b
            }
            
            if( r >= 0x80000000 )
            {
                goto special;
            }
            r <<= 1;
            b >>= 1;
        }

        goto skipSpecial;
        special:;

        q >>= 1;
        b >>= 1;
        r >>= 1;
        while( b > 0x20 )
        {
            t = q + b;
            if( r >= t )
            {
                r -= t;
                q = t + b;
            }
            r <<= 1;
            b >>= 1;
        }
        q >>= 9;
        goto end;

        skipSpecial:;
        
        q >>= 10;
        goto end;
    }

    #region Sin
    public static FInt Sin( FInt i )
    {
        FInt j = (FInt)0;
        for ( ; i < 0; i += FInt.Create( 25736, false ) ) ;
        if ( i > FInt.Create( 25736, false ) )
            i %= FInt.Create( 25736, false );
        FInt k = ( i * FInt.Create( 10, false ) ) / FInt.Create( 714, false );
        if ( i != 0 && i != FInt.Create( 6434, false ) && i != FInt.Create( 12868, false ) && 
            i != FInt.Create( 19302, false ) && i != FInt.Create( 25736, false ) )
            j = ( i * FInt.Create( 100, false ) ) / FInt.Create( 714, false ) - k * FInt.Create( 10, false );
        if ( k <= FInt.Create( 90, false ) )
            return sin_lookup( k, j );
        if ( k <= FInt.Create( 180, false ) )
            return sin_lookup( FInt.Create( 180, false ) - k, j );
        if ( k <= FInt.Create( 270, false ) )
            return sin_lookup( k - FInt.Create( 180, false ), j ).Inverse;
        else
            return sin_lookup( FInt.Create( 360, false ) - k, j ).Inverse;
    }
    
    private static FInt sin_lookup( FInt i, FInt j )
    {
        if ( j > 0 && j < FInt.Create( 10, false ) && i < FInt.Create( 90, false ) )
            return FInt.Create( SIN_TABLE[i.RawValue], false ) + 
                ( ( FInt.Create( SIN_TABLE[i.RawValue + 1], false ) - FInt.Create( SIN_TABLE[i.RawValue], false ) ) / 
                FInt.Create( 10, false ) ) * j;
        else
            return FInt.Create( SIN_TABLE[i.RawValue], false );
    }
    ///<summary>
    ///Sine with degrees as input.
    ///Recomended, it has better accuracy.
    ///</summary>
    public static FInt SinD(FInt degrees)
    {
        //SCRAPPED
        //Factor that represents
        //the divide in value of sin degree table.
        //FInt factor = FInt.Create(1) / 4;

        FInt maxAngle = FInt.Create(360);

        bool isNegative = degrees < 0;

        //If it's negative, make it calculable.
        if (isNegative) degrees *= -1;

        //If the angle is higher than 360, correct it. For example, 366 becomes 6.
        if (degrees > maxAngle) degrees = degrees % maxAngle;

        //if it's negative invert it back to positive, for example, -45 becomes 315
        if (isNegative) degrees = maxAngle - degrees;

        return fpsin(degrees);
        //SCRAPPED CODE BELOW
        /*
        
            //Calculate the rough position of the value you're looking for.
        int position = (int)(degrees.RawValue/ factor.RawValue);
        long impreciseDegreesRaw = position * factor.RawValue;

        //If the values isn't in the table correct it somehow, i don't remember how i did this.
        if (degrees.RawValue > impreciseDegreesRaw)
        {
            FInt t = FInt.Create(degrees.RawValue - impreciseDegreesRaw) / (int)factor.RawValue;

            FInt valu1 = FInt.Create(SIN_DEGREE_TABLE_4[position], false);
            FInt valu2 = FInt.Create(SIN_DEGREE_TABLE_4[position + 1], false);

            return Lerp(valu1, valu2, t);
        }

        return FInt.Create(SIN_DEGREE_TABLE_4[position], false);
        */
    }

    ///<summary>
    ///Sine with point as input
    /// (0 = 0 degrees, 1 = 360 degrees)
    ///Recomended, it has better accuracy.
    ///</summary>
    public static FInt SinPoint(FInt point)
    {
        //SCRAPPED
        //Factor that represents
        //the divide in value of sin degree table.
        //FInt factor = FInt.Create(1) / 4;

        FInt maxAngle = new FInt(1);

        bool isNegative = point < 0;

        //If it's negative, make it calculable.
        if (isNegative) point *= -1;

        //If the angle is higher than 360, correct it. For example, 366 becomes 6.
        if (point > maxAngle) point = point % maxAngle;

        //if it's negative invert it back to positive, for example, -45 becomes 315
        if (isNegative) point = maxAngle - point;

        return fpsinPoint(point);
    }

    //45
    //https://www.nullhardware.com/blog/fixed-point-sine-and-cosine-for-embedded-systems/
    //0-360 in degrees ONLY, highly unsafe otherwise.
    static FInt fpsin(FInt degrees)
    {
        //Converts from 0-360*4096 range of angle to 0-32767
        long semiConverted = degrees.RawValue / 45;
        short i = (short)(semiConverted == 0? semiConverted : semiConverted - 1);

        /* Convert (signed) input to a value between 0 and 8192. (8192 is pi/2, which is the region of the curve fit). */
        /* ------------------------------------------------------------------- */
        i <<= 1;
        bool c = i<0; //set carry for output pos/neg

        if(i == (i|0x4000)) // flip input value to corresponding value in range [0..8192)
            i = (short)(32768 - i);
        i = (short)((i & 0x7FFF) >> 1);
        /* ------------------------------------------------------------------- */

        /* The following section implements the formula:
        = y * 2^-n * ( A1 - 2^(q-p)* y * 2^-n * y * 2^-n * [B1 - 2^-r * y * 2^-n * C1 * y]) * 2^(a-q)
        Where the constants are defined as follows:
        */
        uint iu = (UInt32) i;
        ulong A1=3370945099UL, B1=2746362156UL, C1=292421UL;
        int n=13, p=32, q=31, r=3, a=12;

        uint y = (uint)((C1*(iu))>>n);
        y = (uint)(B1 - ((iu*y)>>r));
        y = (uint)(iu * (y>>n));
        y = (uint)(iu * (y>>n));
        y = (uint)(A1 - (y>>(p-q)));
        y = (uint)(iu * (y>>n));
        y = (uint)((y+(1UL<<(q-a-1)))>>(q-a)); // Rounding

        FInt toReturn;
        toReturn.RawValue = c ? -y : y;

        return toReturn;
    }

    static FInt fpsinPoint(FInt point)
    {
        //Converts from 0-360*4096 range of angle to 0-32767
        long semiConverted = point.RawValue << 3;
        short i = (short)(semiConverted == 0? semiConverted : semiConverted - 1);

        /* Convert (signed) input to a value between 0 and 8192. (8192 is pi/2, which is the region of the curve fit). */
        /* ------------------------------------------------------------------- */
        i <<= 1;
        bool c = i<0; //set carry for output pos/neg

        if(i == (i|0x4000)) // flip input value to corresponding value in range [0..8192)
            i = (short)(32768 - i);
        i = (short)((i & 0x7FFF) >> 1);
        /* ------------------------------------------------------------------- */

        /* The following section implements the formula:
        = y * 2^-n * ( A1 - 2^(q-p)* y * 2^-n * y * 2^-n * [B1 - 2^-r * y * 2^-n * C1 * y]) * 2^(a-q)
        Where the constants are defined as follows:
        */
        uint iu = (UInt32) i;
        ulong A1=3370945099UL, B1=2746362156UL, C1=292421UL;
        int n=13, p=32, q=31, r=3, a=12;

        uint y = (uint)((C1*(iu))>>n);
        y = (uint)(B1 - ((iu*y)>>r));
        y = (uint)(iu * (y>>n));
        y = (uint)(iu * (y>>n));
        y = (uint)(A1 - (y>>(p-q)));
        y = (uint)(iu * (y>>n));
        y = (uint)((y+(1UL<<(q-a-1)))>>(q-a)); // Rounding

        FInt toReturn;
        toReturn.RawValue = c ? -y : y;

        return toReturn;
    }

    ///<summary>
    ///Cosine with degrees as input.
    ///Recomended, it has better accuracy.
    ///</summary>
    public static FInt CosD(FInt degrees)
    {
        return SinD(degrees + FInt.Create(90));
    }

    public static FInt CosPoint(FInt point)
    {
        FInt deg90 = new FInt();
        deg90.RawValue = 1024;
        return SinPoint(point + deg90);
    }

    ///<summary>
    ///Tangent with degrees as input.
    ///Recomended, it has better accuracy.
    ///</summary>
    public static FInt TanD(FInt degrees)
    {
        return SinD( degrees ) / CosD( degrees );
    }

    private static int[] SIN_TABLE = {
        0, 71, 142, 214, 285, 357, 428, 499, 570, 641, 
        711, 781, 851, 921, 990, 1060, 1128, 1197, 1265, 1333, 
        1400, 1468, 1534, 1600, 1665, 1730, 1795, 1859, 1922, 1985, 
        2048, 2109, 2170, 2230, 2290, 2349, 2407, 2464, 2521, 2577, 
        2632, 2686, 2740, 2793, 2845, 2896, 2946, 2995, 3043, 3091, 
        3137, 3183, 3227, 3271, 3313, 3355, 3395, 3434, 3473, 3510, 
        3547, 3582, 3616, 3649, 3681, 3712, 3741, 3770, 3797, 3823, 
        3849, 3872, 3895, 3917, 3937, 3956, 3974, 3991, 4006, 4020, 
        4033, 4045, 4056, 4065, 4073, 4080, 4086, 4090, 4093, 4095, 
        4096
    };
    /*
    private static readonly int [] SIN_DEGREE_TABLE_4 =
    {
        0,17,35,53,71,89,107,125,142,160,178,196,214,232,250,267,285,303,321,339,356,374,392,410,428,445,463,481,499,516,534,552,570,587,605,623,640,658,676,693,711,728,746,764,781,799,816,834,851,869,886,903,921,938,956,973,990,1008,1025,1042,1060,1077,1094,1111,1129,1146,1163,1180,1197,1214,1231,1248,1265,1282,1299,1316,1333,1350,1367,1384,1400,1417,1434,1451,1467,1484,1501,1517,1534,1550,1567,1583,1600,1616,1633,1649,1665,1682,1698,1714,1731,1747,1763,1779,1795,1811,1827,1843,1859,1875,1891,1907,1922,1938,1954,1970,1985,2001,2016,2032,2047,2063,2078,2094,2109,2124,2140,2155,2170,2185,2200,2215,2230,2245,2260,2275,2290,2305,2319,2334,2349,2363,2378,2393,2407,2422,2436,2450,2465,2479,2493,2507,2521,2535,2549,2563,2577,2591,2605,2619,2632,2646,2660,2673,2687,2700,2714,2727,2740,2754,2767,2780,2793,2806,2819,2832,2845,2858,2870,2883,2896,2908,2921,2933,2946,2958,2971,2983,2995,3007,3019,3031,3043,3055,3067,3079,3091,3102,3114,3126,3137,3149,3160,3171,3183,3194,3205,3216,3227,3238,3249,3260,3271,3281,3292,3303,3313,3324,3334,3344,3355,3365,3375,3385,3395,3405,3415,3425,3435,3444,3454,3464,3473,3483,3492,3501,3510,3520,3529,3538,3547,3556,3564,3573,3582,3591,3599,3608,3616,3624,3633,3641,3649,3657,3665,3673,3681,3689,3696,3704,3712,3719,3727,3734,3741,3749,3756,3763,3770,3777,3784,3791,3797,3804,3810,3817,3823,3830,3836,3842,3848,3855,3861,3866,3872,3878,3884,3889,3895,3901,3906,3911,3917,3922,3927,3932,3937,3942,3947,3951,3956,3961,3965,3969,3974,3978,3982,3986,3991,3995,3998,4002,4006,4010,4013,4017,4020,4024,4027,4030,4033,4036,4039,4042,4045,4048,4051,4053,4056,4058,4060,4063,4065,4067,4069,4071,4073,4075,4077,4078,4080,4081,4083,4084,4086,4087,4088,4089,4090,4091,4092,4092,4093,4094,4094,4095,4095,4095,4095,4095,4096,4095,4095,4095,4095,4095,4094,4094,4093,4092,4092,4091,4090,4089,4088,4087,4086,4084,4083,4081,4080,4078,4077,4075,4073,4071,4069,4067,4065,4063,4060,4058,4056,4053,4051,4048,4045,4042,4039,4036,4033,4030,4027,4024,4020,4017,4013,4010,4006,4002,3998,3995,3991,3986,3982,3978,3974,3969,3965,3961,3956,3951,3947,3942,3937,3932,3927,3922,3917,3911,3906,3901,3895,3889,3884,3878,3872,3866,3861,3855,3848,3842,3836,3830,3823,3817,3810,3804,3797,3791,3784,3777,3770,3763,3756,3749,3741,3734,3727,3719,3712,3704,3696,3689,3681,3673,3665,3657,3649,3641,3633,3624,3616,3608,3599,3591,3582,3573,3564,3556,3547,3538,3529,3520,3510,3501,3492,3483,3473,3464,3454,3444,3435,3425,3415,3405,3395,3385,3375,3365,3355,3344,3334,3324,3313,3303,3292,3281,3271,3260,3249,3238,3227,3216,3205,3194,3183,3171,3160,3149,3137,3126,3114,3102,3091,3079,3067,3055,3043,3031,3019,3007,2995,2983,2971,2958,2946,2933,2921,2908,2896,2883,2870,2858,2845,2832,2819,2806,2793,2780,2767,2754,2740,2727,2714,2700,2687,2673,2660,2646,2632,2619,2605,2591,2577,2563,2549,2535,2521,2507,2493,2479,2465,2450,2436,2422,2407,2393,2378,2363,2349,2334,2319,2305,2290,2275,2260,2245,2230,2215,2200,2185,2170,2155,2140,2124,2109,2094,2078,2063,2047,2032,2016,2001,1985,1970,1954,1938,1922,1907,1891,1875,1859,1843,1827,1811,1795,1779,1763,1747,1731,1714,1698,1682,1665,1649,1633,1616,1600,1583,1567,1550,1534,1517,1501,1484,1467,1451,1434,1417,1400,1384,1367,1350,1333,1316,1299,1282,1265,1248,1231,1214,1197,1180,1163,1146,1129,1111,1094,1077,1060,1042,1025,1008,990,973,956,938,921,903,886,869,851,834,816,799,781,764,746,728,711,693,676,658,640,623,605,587,570,552,534,516,499,481,463,445,428,410,392,374,356,339,321,303,285,267,250,232,214,196,178,160,142,125,107,89,71,53,35,17,0,-17,-35,-53,-71,-89,-107,-125,-142,-160,-178,-196,-214,-232,-250,-267,-285,-303,-321,-339,-356,-374,-392,-410,-428,-445,-463,-481,-499,-516,-534,-552,-570,-587,-605,-623,-640,-658,-676,-693,-711,-728,-746,-764,-781,-799,-816,-834,-851,-869,-886,-903,-921,-938,-956,-973,-990,-1008,-1025,-1042,-1060,-1077,-1094,-1111,-1129,-1146,-1163,-1180,-1197,-1214,-1231,-1248,-1265,-1282,-1299,-1316,-1333,-1350,-1367,-1384,-1400,-1417,-1434,-1451,-1467,-1484,-1501,-1517,-1534,-1550,-1567,-1583,-1600,-1616,-1633,-1649,-1665,-1682,-1698,-1714,-1731,-1747,-1763,-1779,-1795,-1811,-1827,-1843,-1859,-1875,-1891,-1907,-1922,-1938,-1954,-1970,-1985,-2001,-2016,-2032,-2048,-2063,-2078,-2094,-2109,-2124,-2140,-2155,-2170,-2185,-2200,-2215,-2230,-2245,-2260,-2275,-2290,-2305,-2319,-2334,-2349,-2363,-2378,-2393,-2407,-2422,-2436,-2450,-2465,-2479,-2493,-2507,-2521,-2535,-2549,-2563,-2577,-2591,-2605,-2619,-2632,-2646,-2660,-2673,-2687,-2700,-2714,-2727,-2740,-2754,-2767,-2780,-2793,-2806,-2819,-2832,-2845,-2858,-2870,-2883,-2896,-2908,-2921,-2933,-2946,-2958,-2971,-2983,-2995,-3007,-3019,-3031,-3043,-3055,-3067,-3079,-3091,-3102,-3114,-3126,-3137,-3149,-3160,-3171,-3183,-3194,-3205,-3216,-3227,-3238,-3249,-3260,-3271,-3281,-3292,-3303,-3313,-3324,-3334,-3344,-3355,-3365,-3375,-3385,-3395,-3405,-3415,-3425,-3435,-3444,-3454,-3464,-3473,-3483,-3492,-3501,-3510,-3520,-3529,-3538,-3547,-3556,-3564,-3573,-3582,-3591,-3599,-3608,-3616,-3624,-3633,-3641,-3649,-3657,-3665,-3673,-3681,-3689,-3696,-3704,-3712,-3719,-3727,-3734,-3741,-3749,-3756,-3763,-3770,-3777,-3784,-3791,-3797,-3804,-3810,-3817,-3823,-3830,-3836,-3842,-3848,-3855,-3861,-3866,-3872,-3878,-3884,-3889,-3895,-3901,-3906,-3911,-3917,-3922,-3927,-3932,-3937,-3942,-3947,-3951,-3956,-3961,-3965,-3969,-3974,-3978,-3982,-3986,-3991,-3995,-3998,-4002,-4006,-4010,-4013,-4017,-4020,-4024,-4027,-4030,-4033,-4036,-4039,-4042,-4045,-4048,-4051,-4053,-4056,-4058,-4060,-4063,-4065,-4067,-4069,-4071,-4073,-4075,-4077,-4078,-4080,-4081,-4083,-4084,-4086,-4087,-4088,-4089,-4090,-4091,-4092,-4092,-4093,-4094,-4094,-4095,-4095,-4095,-4095,-4095,-4096,-4095,-4095,-4095,-4095,-4095,-4094,-4094,-4093,-4092,-4092,-4091,-4090,-4089,-4088,-4087,-4086,-4084,-4083,-4081,-4080,-4078,-4077,-4075,-4073,-4071,-4069,-4067,-4065,-4063,-4060,-4058,-4056,-4053,-4051,-4048,-4045,-4042,-4039,-4036,-4033,-4030,-4027,-4024,-4020,-4017,-4013,-4010,-4006,-4002,-3998,-3995,-3991,-3986,-3982,-3978,-3974,-3969,-3965,-3961,-3956,-3951,-3947,-3942,-3937,-3932,-3927,-3922,-3917,-3911,-3906,-3901,-3895,-3889,-3884,-3878,-3872,-3866,-3861,-3855,-3848,-3842,-3836,-3830,-3823,-3817,-3810,-3804,-3797,-3791,-3784,-3777,-3770,-3763,-3756,-3749,-3741,-3734,-3727,-3719,-3712,-3704,-3696,-3689,-3681,-3673,-3665,-3657,-3649,-3641,-3633,-3624,-3616,-3608,-3599,-3591,-3582,-3573,-3564,-3556,-3547,-3538,-3529,-3520,-3510,-3501,-3492,-3483,-3473,-3464,-3454,-3444,-3435,-3425,-3415,-3405,-3395,-3385,-3375,-3365,-3355,-3344,-3334,-3324,-3313,-3303,-3292,-3281,-3271,-3260,-3249,-3238,-3227,-3216,-3205,-3194,-3183,-3171,-3160,-3149,-3137,-3126,-3114,-3102,-3091,-3079,-3067,-3055,-3043,-3031,-3019,-3007,-2995,-2983,-2971,-2958,-2946,-2933,-2921,-2908,-2896,-2883,-2870,-2858,-2845,-2832,-2819,-2806,-2793,-2780,-2767,-2754,-2740,-2727,-2714,-2700,-2687,-2673,-2660,-2646,-2632,-2619,-2605,-2591,-2577,-2563,-2549,-2535,-2521,-2507,-2493,-2479,-2465,-2450,-2436,-2422,-2407,-2393,-2378,-2363,-2349,-2334,-2319,-2305,-2290,-2275,-2260,-2245,-2230,-2215,-2200,-2185,-2170,-2155,-2140,-2124,-2109,-2094,-2078,-2063,-2048,-2032,-2016,-2001,-1985,-1970,-1954,-1938,-1922,-1907,-1891,-1875,-1859,-1843,-1827,-1811,-1795,-1779,-1763,-1747,-1731,-1714,-1698,-1682,-1665,-1649,-1633,-1616,-1600,-1583,-1567,-1550,-1534,-1517,-1501,-1484,-1467,-1451,-1434,-1417,-1400,-1384,-1367,-1350,-1333,-1316,-1299,-1282,-1265,-1248,-1231,-1214,-1197,-1180,-1163,-1146,-1129,-1111,-1094,-1077,-1060,-1042,-1025,-1008,-990,-973,-956,-938,-921,-903,-886,-869,-851,-834,-816,-799,-781,-764,-746,-728,-711,-693,-676,-658,-640,-623,-605,-587,-570,-552,-534,-516,-499,-481,-463,-445,-428,-410,-392,-374,-356,-339,-321,-303,-285,-267,-250,-232,-214,-196,-178,-160,-142,-125,-107,-89,-71,-53,-35,-17,0
    };
    */
    
    #endregion

    private static FInt mul( FInt F1, FInt F2 )
    {
        return F1 * F2;
    }

    #region Cos, Tan, Asin
    public static FInt Cos( FInt i )
    {
        return Sin( i + FInt.Create( 6435, false ) );
    }

    public static FInt Tan( FInt i )
    {
        return Sin( i ) / Cos( i );
    }

    public static FInt Asin( FInt F )
    {
        bool isNegative = F < 0;
        F = Abs( F );

        if ( F > FInt.OneF )
            throw new ArithmeticException( "Bad Asin Input:" + F.ToDouble() );

        FInt f1 = mul( mul( mul( mul( new FInt{ RawValue = 145103 >> FInt.SHIFT_AMOUNT }, F ) -
            FInt.Create( 599880 >> FInt.SHIFT_AMOUNT, false ), F ) +
            FInt.Create( 1420468 >> FInt.SHIFT_AMOUNT, false ), F ) -
            FInt.Create( 3592413 >> FInt.SHIFT_AMOUNT, false ), F ) +
            FInt.Create( 26353447 >> FInt.SHIFT_AMOUNT, false );
        FInt f2 = (PI >> 1) - ( Sqrt( FInt.OneF - F ) * f1 );

        return isNegative ? f2.Inverse : f2;
    }
    #endregion

    #region ATan, ATan2
    public static FInt Atan( FInt F )
    {
        return Asin( F / Sqrt( FInt.OneF + ( F * F ) ) );
    }

    public static FInt Atan2( FInt F1, FInt F2 )
    {
        if ( F2.RawValue == 0 && F1.RawValue == 0 )
            return (FInt)0;

        FInt result = (FInt)0;
        if ( F2 > 0 )
            result = Atan( F1 / F2 );
        else if ( F2 < 0 )
        {
            if ( F1 >= 0 )
                result = ( PI - Atan( Abs( F1 / F2 ) ) );
            else
                result = ( PI - Atan( Abs( F1 / F2 ) ) ).Inverse;
        }
        else
            result = ( F1 >= 0 ? PI : PI.Inverse ) / FInt.Create( 2, true );

        return result;
    }
    #endregion

    #region Abs
    public static FInt Abs( FInt F )
    {
        return F < 0 ? -F : F;
    }
    #endregion

    public static FInt Lerp(FInt a, FInt b, FInt t)
    {
        return a + (b - a) * t;
    }

    public static bool Between (FInt value, FInt min, FInt max)
    {
        return value > min && value < max;
    }

    public static bool NotBetween (FInt value, FInt min, FInt max)
    {
        return value <= min && value >= max;
    }

    public static FInt Min(FInt a1, FInt a2)
    {
        return a1 < a2 ? a1 : a2;
    }

    public static FInt Max(FInt a1, FInt a2)
    {
        return a1 > a2 ? a1 : a2;
    }

    public static bool NearlyEqual(FInt a, FInt b, FInt tolerance)
    {
        return Abs(a - b) <= tolerance;
    }

}