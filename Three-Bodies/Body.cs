using System.Numerics;
using SolidCode.Atlas;
using SolidCode.Atlas.AssetManagement;
using SolidCode.Atlas.Components;
using SolidCode.Atlas.ECS;
using SolidCode.Atlas.Standard;

namespace ThreeBodies;

public class Body : Component
{
    public Vec2 Position;
    public Vec2 Velocity;
    public Vec2 Acceleration;
    public double Mass;
    private Transform _transform; // Vi sparar transformen
    public Body()
    {
        Position = new Vec2(ARandom.Range(-1f, 1f), ARandom.Range(-1f, 1f));
        Velocity = new Vec2(ARandom.Range(-1f, 1f), ARandom.Range(-1f, 1f));
        Acceleration = new Vec2();
        Mass = 5;
    }

    public void Start()
    {
        // Vi lägger till en komponent som kan ritas ut
        SpriteRenderer sr = Entity.AddComponent<SpriteRenderer>();
        sr.Sprite = AssetManager.GetTexture("node");
        sr.Color = new Vector4(ARandom.Value(), ARandom.Value(), ARandom.Value(), 1);
        _transform = GetComponent<Transform>();
        _transform.Scale = new Vector2(0.05f, 0.05f);
    }
    
    public void Tick()
    {
        double dt = 0.0001f;
        Velocity.X += Acceleration.X * dt;
        Velocity.Y += Acceleration.Y * dt;
        Position.X += Velocity.X * dt;
        Position.Y += Velocity.Y * dt;
    }

    public void Update()
    {
        // Vi uppdaterar den grafiska positionen av kroppen
        _transform.Position = Position.AsVector2();
    }
    
    private void UpdateAcceleration() // Vi måste uppdatera alla samtidigt
    {
        Acceleration.X = 0;
        Acceleration.Y = 0;
        foreach (Body body in Program.Bodies)
        {
            if (body == this) continue; // Vi kan inte påverka oss själva
            double dx = body.Position.X - Position.X;
            double dy = body.Position.Y - Position.Y;
            double r = Math.Sqrt(dx * dx + dy * dy);
            double f = 0.0000000000667 * Mass * body.Mass / (r * r); // G * m1 * m2 / r^2
            Acceleration.X += f * dx / r;
            Acceleration.Y += f * dy / r;
        }
    }

    public static void UpdateAccelerations()
    {
        foreach (Body body in Program.Bodies)
        {
            body.UpdateAcceleration();
        }
    }
}