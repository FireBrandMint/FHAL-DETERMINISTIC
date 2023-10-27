using System;
using System.Collections.Generic;

namespace GJP;

public class UIShape
{
    public Vector2Fi Position;

    public UIAdjustmentMode Mode;

    /// <summary>
    /// The object that is using this shape for collision.
    /// </summary>
    public CollisionAntenna ObjectUsingIt = null;

    public virtual bool IsColliding(Vector2Fi point)
    {
        throw new NotImplementedException();
    }
}