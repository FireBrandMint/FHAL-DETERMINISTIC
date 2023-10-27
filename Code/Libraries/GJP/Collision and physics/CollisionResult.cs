
namespace GJP;

public struct CollisionResult
{
    public bool Intersects = false;

    public Vector2Fi Separation = new Vector2Fi();

    public Vector2Fi SeparationDirection = new Vector2Fi();

    public CollisionResult()
    {
        
    }
}