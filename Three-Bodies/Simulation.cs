using Three_core;
using Three_Core;

namespace ThreeBodies;

public class Simulation
{
    public Body[] Bodies;
    public string Name;
    public Simulation(SimulationState state)
    {
        
        Bodies = new[]
        {
            new Body(this, state.Bodies[0]),
            new Body(this, state.Bodies[1]),
            new Body(this, state.Bodies[2])
        };
    }
    
    public void RunSimulation(ProbabilityMap map)
    {
        long ticks = 0;
        while (ticks < Program.TickCount)
        {
            foreach (var body in Bodies)
            {
                body.Tick();
            }
            ticks++;
        }
        map.AddA(Bodies[0].Position);
        map.AddB(Bodies[1].Position);
        map.AddC(Bodies[2].Position);
    }
}