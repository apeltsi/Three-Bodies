using System.Numerics;
using SolidCode.Atlas;
using SolidCode.Atlas.AssetManagement;
using SolidCode.Atlas.Components;
using SolidCode.Atlas.ECS;
using SolidCode.Atlas.Standard;

namespace ThreeBodies;

public class Body : Component
{
    public Vec2 Position = new(ARandom.Range(-1f, 1f), ARandom.Range(-1f, 1f));
    public Vec2 MidPosition = new(); // Används för midpoint metoden
    public Vec2 Velocity = new();
    
    public Vec2 Acceleration = new();
    public double Mass = 50;
    private Transform _transform; // Vi sparar transformen

    public void Start()
    {
        // Vi lägger till en komponent som kan ritas ut
        SpriteRenderer sr = Entity.AddComponent<SpriteRenderer>();
        sr.Sprite = AssetManager.GetTexture("node");
        sr.Color = new Vector4(ARandom.Value(), ARandom.Value(), ARandom.Value(), 1);
        _transform = GetComponent<Transform>();
        _transform.Scale = new Vector2(0.05f, 0.05f);
    }

    private static double _lastTime = 0;
    public void Tick()
    {
        double dt = Program.Dt;
        if (_lastTime != Time.tickTime)
        {
            // Först måste vi uppdatera alla kroppars MidPoint position
            foreach (Body body in Program.Bodies)
            {
                body.UpdateMidpoint(dt);
            }

            _lastTime = Time.tickTime;
        }
        // Se https://en.wikipedia.org/wiki/Midpoint_method
        
        // temporär hastighet (halvt steg framåt)
        Vec2 tempVelocity = new Vec2(
            Velocity.X + 0.5 * dt * Acceleration.X,
            Velocity.Y + 0.5 * dt * Acceleration.Y
        );
        
        // temporär position (halvt steg framåt)
        MidPosition = new Vec2(
            Position.X + 0.5 * dt * tempVelocity.X,
            Position.Y + 0.5 * dt * tempVelocity.Y
        );
        
        // räkna ut accelerationen vid den temporära positionen
        Position = MidPosition;
        UpdateAcceleration();

        // uppdatera hastigheten och positionen
        Velocity.X += dt * Acceleration.X;
        Velocity.Y += dt * Acceleration.Y;
        
        Position.X += dt * tempVelocity.X;
        Position.Y += dt * tempVelocity.Y;
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
            if (r < 1e-10) continue; // Vi kan inte dividera med 0
            double f = 0.0000000000667 * Mass * body.Mass / (r * r); // G * m1 * m2 / r^2
            Acceleration.X += f * dx / r;
            Acceleration.Y += f * dy / r;
        }
    }
    
    private void UpdateMidpoint(double dt)
    {
        MidPosition = new Vec2(Position.X + 0.5 * dt * Velocity.X,
            Position.Y + 0.5 * dt * Velocity.Y);
    }
    
}