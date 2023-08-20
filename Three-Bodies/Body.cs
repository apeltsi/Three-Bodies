﻿using System.Numerics;
using SolidCode.Atlas;
using SolidCode.Atlas.AssetManagement;
using SolidCode.Atlas.Components;
using SolidCode.Atlas.ECS;
using SolidCode.Atlas.Standard;

namespace ThreeBodies;

public class Body
{
    public Vec2 Position;
    public Vec2 MidPosition = new(); // Används för midpoint metoden
    public Vec2 Velocity = new();
    
    public Vec2 Acceleration = new();
    public double Mass = 50;
    private Simulation _simulation;
    public Body(Simulation simulation, BodyState state)
    {
        _simulation = simulation;
        Position = state.SamplePosition();
    }

    private static double _lastTime = 0;
    public void Tick()
    {
        double dt = Program.Dt;
        if (_lastTime != Time.tickTime)
        {
            // Först måste vi uppdatera alla kroppars MidPoint position
            foreach (Body body in _simulation.Bodies)
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
    
    private void UpdateAcceleration() // Vi måste uppdatera alla samtidigt
    {
        Acceleration.X = 0;
        Acceleration.Y = 0;
        foreach (Body body in _simulation.Bodies)
        {
            if (body == this) continue; // Vi kan inte påverka oss själva
            double dx = body.Position.X - Position.X;
            double dy = body.Position.Y - Position.Y;
            double softeningFactor = 0.001; // Vi använder en softening factor för att undvika singulariteter
            double r = Math.Sqrt(dx * dx + dy * dy + softeningFactor * softeningFactor);
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