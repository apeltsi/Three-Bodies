
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
    
    public static Vec2 operator +(Vec2 a, Vec2 b)
    {
        return new Vec2(a.X + b.X, a.Y + b.Y);
    }

    public static Vec2 operator *(Vec2 a, double b)
    {
        return new Vec2(a.X * b, a.Y * b);
    }
    
    public override int GetHashCode()
    {
        unchecked // Overflow is fine, just wrap
        {
            int hash = 17;
            // Suitable nullity checks etc, of course :)
            hash = hash * 23 + X.GetHashCode();
            hash = hash * 23 + Y.GetHashCode();
            return hash;
        }
    }
}