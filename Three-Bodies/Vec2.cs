
using System.Numerics;

namespace ThreeBodies;

public class Vec2
{
    public double X;
    public double Y;

    public Vec2(double x, double y)
    {
        this.X = x;
        this.Y = y;
    }

    public Vec2()
    {
        
    }

    public Vector2 AsVector2()
    {
        return new Vector2((float) X, (float) Y);
    }
}