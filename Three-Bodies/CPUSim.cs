using System.Numerics;
using SolidCode.Atlas;
using SolidCode.Atlas.AssetManagement;
using SolidCode.Atlas.Components;
using SolidCode.Atlas.ECS;
using Three_core;
using Three_Core;

namespace ThreeBodies;

public class CPUSim
{
    public const float G = 0.0000000000667f;
    public const float Mass = 50f;
    public const int Ticks = 1000;
    public const float TimeStep = 0.8f;
    struct Body
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public Vector2 Acceleration;
    }

    private Vector2[] CalculateAccelerations(Body[] bodies)
    {
        Vector2[] accelerations = new Vector2[3];
        for (int i = 0; i < 3; i++)
        {
            Vector2 netAcceleration = Vector2.Zero;
            for (int j = 0; j < 3; j++)
            {
                if (i != j)
                {
                    Vector2 r = bodies[j].Position - bodies[i].Position;
                    float softeningFactor = 0.01f;
                    float distance = r.Length() + softeningFactor;
                    if (distance < 1e-10) continue;
                    Vector2 direction = r / distance;
                    float magnitude = G * (Mass * Mass / (distance * distance));
                    netAcceleration += direction * magnitude;
                }
            }

            accelerations[i] = netAcceleration;
        }

        return accelerations;
    }
    
    public static void RunSimulation()
    {
        CPUSim sim = new();
        SimulationState state = new(Program.CThreadCount * Program.CThreadGroups);
        sim.DoSim(state);
        ProbabilityMap map = new ProbabilityMap();
        for (int i = 0; i < 3; i++)
        {
            map.AddAt(state.Bodies[i].Position, i);
        }
        ThreeBodySimulationData.SaveToFile(state, map);
    }
    
    private Vector2[] DoSim(SimulationState state)
    {
        Vector2[][] paths = new[] {new Vector2[Ticks], new Vector2[Ticks], new Vector2[Ticks]};
        Body[] bodies = new []
        {
            new Body()
            {
                Position = state.Bodies[0].Position.AsVector2(),
                Velocity = Vector2.Zero,
                Acceleration = Vector2.Zero
            },
            new Body()
            {
                Position = state.Bodies[1].Position.AsVector2(),
                Velocity = Vector2.Zero,
                Acceleration = Vector2.Zero
            },
            new Body()
            {
                Position = state.Bodies[2].Position.AsVector2(),
                Velocity = Vector2.Zero,
                Acceleration = Vector2.Zero
            },
        };

        for (int t = 0; t < Ticks; t++)
        {
            Vector2[] initialAccelerations = CalculateAccelerations(bodies);

            Body[] midbodies = new Body[3];
            for (int i = 0; i < 3; i++)
            {
                midbodies[i].Position = bodies[i].Position + bodies[i].Velocity * TimeStep * 0.5f;
                midbodies[i].Velocity = bodies[i].Velocity + initialAccelerations[i] * TimeStep * 0.5f;
            }
            
            Vector2[] midAccelerations = CalculateAccelerations(midbodies);

            for (int i = 0; i < 3; i++)
            {
                bodies[i].Position += midbodies[i].Velocity * TimeStep;
                paths[i][t] = bodies[i].Position;
                
                bodies[i].Velocity += midAccelerations[i] * TimeStep;
            }
            
        }

        {
            // Lets create the visualizers
            for (int i = 0; i < 3; i++)
            {
                Entity e = new Entity("Body " + i, null, new Vector2(0.05f, 0.05f));
                e.AddComponent<PathFollower>().Path = paths[i];
                SpriteRenderer sr = e.AddComponent<SpriteRenderer>();
                sr.Sprite = AssetManager.GetTexture("node");
                sr.Color = i switch
                {
                    0 => new Vector4(1f, 0f, 0f, 1f),
                    1 => new Vector4(0f, 1f, 0f, 1f),
                    _ => new Vector4(0f, 0f, 1f, 1f)
                };
            }
        }

        return new[] { bodies[0].Position, bodies[1].Position, bodies[2].Position};
    }
}