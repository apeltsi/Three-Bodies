
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
        this.X = 0;
        this.Y = 0;
    }

    public Vector2 AsVector2()
    {
        return new Vector2((float) X, (float) Y);
    }
    
    public static implicit operator Vector2(Vec2 v)
    {
        return v.AsVector2();
    }

    public static implicit operator string(Vec2 v)
    {
        return "<X: " + v.X + ", Y: " + v.Y + ">";
    }
}