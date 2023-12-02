using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GJP2;

public class PhysicsSimulation2Fi
{
    ShapeCook Cook = new ShapeCook();

    public static bool CreatingRigid = false;

    public RigidBody2Fi NewRigidBody()
    {
        var rigi = new RigidBody2Fi();
    }

    /// <summary>
    /// <para>Creates a shape.</para>
    /// IMPORTANT: the shape is bound to this physics simulation
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="center"></param>
    /// <param name="rotation"></param>
    /// <param name="scale"></param>
    /// <param name="sizeRect"></param>
    /// <returns></returns>
    public Shape NewRectangle(Vector2Fi pos, Vector2Fi center, FInt rotation, Vector2Fi sizeRect, Vector2Fi scale)
    {
        Shape subject = Shape.NewRectangle(pos, center, rotation, sizeRect, scale);

        return subject;
    }
    /// <summary>
    /// <para>Creates a shape.</para>
    /// IMPORTANT: the shape is bound to this physics simulation
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="center"></param>
    /// <param name="rotation"></param>
    /// <param name="scale"></param>
    /// <param name="length"></param>
    /// <param name="middleVertice"></param>
    /// <returns></returns>
    public Shape NewTriangle(Vector2Fi pos, Vector2Fi center, FInt rotation, Vector2Fi scale, FInt length, Vector2Fi middleVertice)
    {
        Shape subject = Shape.NewTriangle(pos, center, rotation, scale, length, middleVertice);

        return subject;
    }
    /// <summary>
    /// <para>Creates a shape.</para>
    /// IMPORTANT: the shape is bound to this physics simulation
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="center"></param>
    /// <param name="radius"></param>
    /// <returns></returns>
    public Shape NewCircle(Vector2Fi pos, Vector2Fi center, FInt radius)
    {
        Shape subject = Shape.NewCircle(pos, center, radius);

        return subject;
    }
}

public class RigidBody2Fi
{
    #pragma warning disable 0169
    int ID;

    PhysicsSimulation2Fi Simulation;

    private RigidBody2Fi()
    {

    }

    public RigidBody2Fi(PhysicsSimulation2Fi simulation, int id)
    {
        Simulation = simulation;
    }
}