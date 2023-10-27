using System;

namespace GJP;

public class UIConvexPolygon : UIShape
{
    public Vector2Fi Scale = new Vector2Fi(1,1);

    Vector2Fi[] OriginalModel;

    public static UIConvexPolygon CreateRect (Vector2Fi position, Vector2Fi scale, UIAdjustmentMode mode)
    {
        FInt x = scale.x >> 1;
        FInt y = scale.y >> 1;

        Vector2Fi[] model = new Vector2Fi[]
        {
            //top left
            new Vector2Fi(-x, -y),
            //bottom left
            new Vector2Fi(-x, y),
            //bottom right
            new Vector2Fi(x, y),
            //top right
            new Vector2Fi(x, -y),
        };

        return new UIConvexPolygon(position, model, mode);
    }

    public static UIConvexPolygon CreateTriangle (Vector2Fi position, Vector2Fi scale, UIAdjustmentMode mode)
    {
        FInt x = scale.x >> 1;
        FInt y = scale.y >> 1;

        Vector2Fi[] model = new Vector2Fi[]
        {
            //bottom left
            new Vector2Fi(-x, y),
            //bottom right
            new Vector2Fi(x, y),
            //top
            new Vector2Fi((FInt)0, -y),
        };

        return new UIConvexPolygon(position, model, mode);
    }

    /// <summary>
    /// Position is the position of the polygon with vector
    /// corresponding to porcentage of the screen 0 to 100.
    /// Model is the model of the convex polygon with vector
    /// corresponding to porcentages of the screen -100 to 100.
    /// </summary>
    /// <param name="Model"></param>
    public UIConvexPolygon (Vector2Fi position, Vector2Fi[] Model, UIAdjustmentMode mode)
    {
        OriginalModel = Model;
        Position = position;

        Mode = mode;
    }

    public override bool IsColliding(Vector2Fi mousePoint)
    {
        Vector2Fi viewSize = UIGetter.GetUISize();

        if(viewSize.x == -1) throw(new Exception("UIGetter function not set, please use UIGetter.SetUISizeGetter to set the function that gets the view size of the program."));

        int lenght = OriginalModel.Length;

        Vector2Fi[] ProducedModel = new Vector2Fi[lenght];

        if(Mode == UIAdjustmentMode.Compact)
        {
            if(viewSize.x < viewSize.y)
            {
                viewSize = new Vector2Fi(viewSize.x, viewSize.x);
            }
            else
            {
                viewSize = new Vector2Fi(viewSize.y, viewSize.y);
            }
        }

        Vector2Fi currPos = (Position * viewSize) / 100;

        for(int i = 0; i < lenght; ++i)
        {
            ProducedModel[i] = currPos + (( OriginalModel[i] * viewSize) / 100) * Scale;
        }

        return PointInConvexPolygon(mousePoint, ProducedModel);
    }

    public static bool PointInConvexPolygon(Vector2Fi testPoint, Vector2Fi[] polygon)
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
}