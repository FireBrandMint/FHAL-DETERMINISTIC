using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GJP2;
public struct CollisionResult
{
    public bool Intersects = false;

    public Vector2Fi Separation = new Vector2Fi();

    public Vector2Fi SeparationDirection = new Vector2Fi();

    public bool FindContacts = false;

    public int ContactCount = 0;
    public Vector2Fi Contact1, Contact2;

    public CollisionResult()
    {
        
    }
}
