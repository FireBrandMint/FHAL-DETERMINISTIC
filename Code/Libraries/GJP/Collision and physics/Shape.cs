using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace GJP;

/// <summary>
/// Shape class, collision detection with GetShapesInGrid
/// can be deterministic as long as the shapes keep their IDS.
/// </summary>
public class Shape: XYBoolHolder
{
    /// <summary>
    /// Value that determines entity ID, please, if you're using GGPO change
    /// this value back to the original of the original tick wich
    /// you're loading state from, and also the IDNEXT of the
    /// DeterministicSimulation.
    /// </summary>
    
    int _ID;

    bool _Detecting = false;

    public int ID 
    {
        get => _ID;

        set 
        {

            if(Active)
            {
                if(_ID == value) return;

                Deactivate();
                _ID = value;
                Activate();
                return;
            }

            _ID = value;
        }
    }

    /// <summary>
    /// Wether or not the shape is detecting other shapes in the simulation.
    /// Only for objects that need to detect, do not use on objects that just need to be detected.
    /// Only effective if the simulation is not NULL and IsActive is true.
    /// When this is set to true in a simulation, PastPosition is set to the Position value.
    /// When the Shape enters a simulation with this set to true, PastPosition is set to the Position value.
    /// </summary>
    public bool Detecting
    {
        get => _Detecting;
        set
        {
            if(value == _Detecting) return;

            if(Simulation == null)
            {
                _Detecting = value;
                return;
            }

            if(value)
            {
                Simulation.TickShape(ID, this);
                LastPosition = Position;
            }
            else
            {
                Simulation.DontTickShape(ID);
            }

            _Detecting = value;
        }
    }

    protected bool Disposed = false;

    #region grid AKA simulation

    private DtCollisionSimulation Simulation = null;

    bool Active = false;

    bool GridIdentifierSet = false;

    /// <summary>
    /// The object that is using this shape for collision.
    /// </summary>
    public CollisionAntenna ObjectUsingIt = null;

    //Hairball code function below, only able to be read my me (Guliver Jham)
    protected static long [] GridAddShape (Shape s, XYList<Shape> shapeGrid)
    {
        Vector2Fi pos = s.Position;

        Vector2Fi range = s.GetRange();

        Vector2Fi topLeft = pos - range;

        Vector2Fi bottomRight = pos + range;

        return shapeGrid.AddNode(s, shapeGrid.GetRanges(topLeft, bottomRight, s.GetGridIdentifier()));
    }

    //Hairball code function below, only able to be read my me (Guliver Jham)
    protected static long[] GridMoveShape (Shape s, XYList<Shape> shapeGrid)
    {
        long[] identifier = s.GetGridIdentifier();

        Vector2Fi pos = s.Position;

        Vector2Fi range = s.GetRange();

        Vector2Fi topLeft = pos - range;

        Vector2Fi bottomRight = pos + range;

        bool different = shapeGrid.RangesDiffer(identifier, topLeft, bottomRight);

        if(different)
        {
            shapeGrid.RemoveValue(identifier, s);

            shapeGrid.GetRanges(topLeft, bottomRight, identifier);

            shapeGrid.AddNode(s, identifier);
        }

        return identifier;
    }

    protected static void GridRemoveShape (Shape s, XYList<Shape> shapeGrid)
    {
        shapeGrid.RemoveValue(s.GetGridIdentifier(), s);
    }

    protected static Shape[] GetShapesInGrid (long[] identifier, XYList<Shape> shapeGrid)
    {
        return shapeGrid.GetValues(identifier);
    }

    /// <summary>
    /// Extreme internal function, if you don't want to end up crying in the bathroom, please don't use it.
    /// </summary>
    /// <param name="s"></param>
    /// <param name="shapeGrid"></param>
    /// <returns></returns>
    public static Shape[] GetShapesInGrid (Shape s, XYList<Shape> shapeGrid)
    {
        return shapeGrid.GetValues(s.GetGridIdentifier());
    }

    #endregion

    private bool _Selected = false;
    public bool SelectedC {get => _Selected; set => _Selected = value;}

    public virtual Vector2Fi Position{get;set;}

    /// <summary>
    /// Value only used for calculations on objects that take more than 1 Step
    /// to move. 
    /// </summary>
    public Vector2Fi LastPosition;

    /// <summary>
    /// Segmented collision only functions if the object is Detecting and this value is higher than 1.
    /// Any values below or equal to one mean that the object's collision ISN'T segmented.
    /// Segmented collision is for objects that are very fast still be able to collide with everything
    /// between it's new position and it's old position.
    /// </summary>
    public int StepAmount = 1;

    /// <summary>
    /// Wether or not it does not solve collisions by calling CollisionAntenna.ResolveOverlap and instead
    /// just detects if another object is inside.
    /// </summary>
    public bool IntersectOnly = false;

    ///<summary>
    ///Range is a vector
    ///X is the highest absolute x coord of the points list in position Vector.ZERO
    ///Y is the highest absolute y coord of the points list in position Vector.ZERO
    /// DO NOT USE
    ///</summary>
    public virtual Vector2Fi GetRange() => throw new NotImplementedException();
    /// <summary>
    /// DO NOT USE.
    /// </summary>
    /// <returns></returns>
    public virtual long[] GetGridIdentifier() => throw new NotImplementedException();

    /// <summary>
    /// DO NOT USE.
    /// </summary>
    /// <param name="newValue"></param>
    public virtual void SetGridIdentifier(long[] newValue) => throw new NotImplementedException();

    /// <summary>
    /// Teleports object instead of moving it.
    /// Because moving it will probably trigger all objects between
    /// its new position and its old position if it is Detecting in a Simulation.
    /// </summary>
    /// <param name="newPos"></param>
    public void Teleport (Vector2Fi newPos)
    {
        Position = newPos;
        LastPosition = newPos;
    }

    /// <summary>
    /// Sets the simulation the shape lives in.
    /// If makeNewId is false, then it will set its id to the _id parameter instead!
    /// </summary>
    /// <param name="_simulation"></param>
    /// <param name="makeNewId"></param>
    /// <param name="_id"></param>
    public void SetSimulation(DtCollisionSimulation _simulation, bool makeNewId = true, int _id = -1)
    {
        bool wasActive = Active;

        if(Simulation != null)
        {
            //Detecting = false;

            if (wasActive)
            {
                Deactivate();
                wasActive = true;
            }
            
            if (_Detecting) Simulation.DontTickShape(ID);
        }

        if(_simulation != null)
        {
            if (makeNewId)
            {
                ID = _simulation.GetId();
            }
            else ID = _id;

            if(wasActive) Activate();

            if (_Detecting) 
            {
                Simulation.TickShape(ID, this);
                LastPosition = Position;
            }
        }

        Simulation = _simulation;
    } 
    /// <summary>
    /// Activates the object so it can be detected (or optionally detect) in the simulation.
    /// DEPRECATED, try changing the value of IsActive instead!
    /// </summary>
    private void Activate()
    {
        if(Simulation == null) return;

        if(!Active)
        {
            if(!GridIdentifierSet)
            {
                SetGridIdentifier(new long[4]);
                GridIdentifierSet = true;
            }

            SetGridIdentifier(GridAddShape(this, Simulation.Grid));
        }
    }

    /// <summary>
    /// Deactivates the object so it can't be detected or detect in the simulation.
    /// DEPRECATED, try changing the value of IsActive instead!
    /// </summary>
    private void Deactivate()
    {
        if(Simulation == null) return;

        if(Active) GridRemoveShape(this, Simulation.Grid);
    }

    protected void MoveActive()
    {
        if(Active && Simulation != null) SetGridIdentifier(GridMoveShape(this, Simulation.Grid));
    }

    /// <summary>
    /// Can the shape be detected (or when this is true, detect with Detecting bool) in the simulation?
    /// </summary>
    /// <returns></returns>
    public bool IsActive
    {
        get => Active;
        set
        {
            if (value == Active) return;

            if (Simulation == null)
            {
                Active = value;
                return;
            }

            if(value)
            {
                Activate();
            }
            else
            {
                Deactivate();
            }
            
            Active = value;
        }
    }

    /// <summary>
    /// Used internally on the DtCollisionSimulation class, please don't use it.
    /// It detects if the objects are intersecting, and if they are, it tells by how
    /// much they should be separated.
    /// </summary>
    /// <param name="poly"></param>
    /// <param name="result"></param>
    public void IntersectsInfo(Shape poly, ref CollisionResult result)
    {
        Vector2Fi
        bRange = poly.GetRange(),
        bPosition = poly.Position;

        Vector2Fi
        aRange = GetRange(),
        aPosition = Position;

        Vector2Fi r = aRange + bRange;

        Vector2Fi d = aPosition - bPosition;

        FInt dx = d.x, dy = d.y;

        if(dx < 0) dx = -dx;
        if(dy < 0) dy = -dy;

        if(dx > r.x || dy > r.y) return;

        if(this is ConvexPolygon thisShape)
        {
            if(poly is ConvexPolygon convPoly)
            {
                thisShape.PolyIntersectsInfo(convPoly, ref result);
                return;
            }
            else if(poly is CircleShape circle)
            {
                thisShape.CircleIntersectsInfo(circle, ref result);
                return;
            }
        }
        else if(this is CircleShape thisShape2)
        {
            if(poly is ConvexPolygon convPoly)
            {
                thisShape2.PolyIntersectsInfo(convPoly, ref result);
                return;
            }
            else if(poly is CircleShape circle)
            {
                thisShape2.CircleIntersectsInfo(circle, ref result);
                return;
            }
        }
        
        throw new System.Exception($"Shape not implemented! Shape ids: {this.GetType()}, {poly.GetType()}.");
    }

    /// <summary>
    /// Only detects if 2 objects intersect.
    /// </summary>
    /// <param name="poly"></param>
    /// <returns></returns>
    public bool Intersect(Shape poly)
    {
        Vector2Fi
        bRange = poly.GetRange(),
        bPosition = poly.Position;

        Vector2Fi
        aRange = GetRange(),
        aPosition = Position;

        Vector2Fi r = aRange + bRange;

        Vector2Fi d = aPosition - bPosition;

        FInt dx = d.x, dy = d.y;

        if(dx < 0) dx = -dx;
        if(dy < 0) dy = -dy;

        if(dx > r.x || dy > r.y) return false;

        switch(this)
        {
            case ConvexPolygon:
            switch(poly)
            {
                case ConvexPolygon:
                return ((ConvexPolygon)this).PolyIntersects((ConvexPolygon) poly);
                
                case CircleShape:
                return ((ConvexPolygon)this).CircleIntersects((CircleShape) poly);
            }
            break;

            case CircleShape:
            switch(poly)
            {
                case ConvexPolygon:
                return ((CircleShape)this).PolyIntersects((ConvexPolygon) poly);
                
                case CircleShape:
                return ((CircleShape)this).CircleIntersects((CircleShape) poly);
            }
            break;
        }

        throw new System.Exception($"Shape not implemented! Shape id: {this.GetType()}, {poly.GetType()}.");
    }

    /*public (Shape[] arr, int count) GetIntersectionInfos (List<Shape> shapes)
    {
        Shape[] arr = new Shape[10];
        
        int arrSize = 0;

        Vector2
        aRange = GetRange(),
        aPosition = Position;

        for(int i = 0; i < shapes.Count; ++i)
        {
            Shape curr = shapes[i];

            if(curr == this) continue;

            Vector2
            bRange = curr.GetRange(),
            bPosition = curr.Position;

            Vector2 r = aRange + bRange;

            Vector2 d = aPosition - bPosition;

            FInt dx = d.x, dy = d.y;

            if(dx < 0) dx = -dx;
            if(dy < 0) dy = -dy;

            if(dx > r.x || dy > r.y) continue;
            arr[arrSize] = curr;

            ++arrSize;

            if(arr.Length - 1 == arrSize)
            {
                Array.Resize(ref arr, arr.Length + 10);
            }
            
        }

        return (arr, arrSize);
    }*/

    protected static bool PointInConvexPolygon(Vector2Fi testPoint, Span<Vector2Fi> polygon)
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

    public void Dispose ()
    {
        if (Disposed) return;
        
        Dispose(true);

        ObjectUsingIt = null;

        SetSimulation(null);

        Disposed = true;
    }

    protected virtual void Dispose (bool disposing)
    {
        //TODO: Implement object pooling for non cataclismic failure on the computer
        //on top of this convoluted code, if it's ugly, at least it gotta be
        //optimized........

        //TODO: Reminding myself to do object pooling for the XYList too,
        //whoever doesn't want their pc to explode with a big game would
        //would much appreciate it.
    }

    protected void Reuse()
    {
        Dispose(false);
    }
}