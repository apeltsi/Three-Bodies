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
        map.AddAt(Bodies[0].Position, 0);
        map.AddAt(Bodies[1].Position, 1);
        map.AddAt(Bodies[2].Position, 2);
    }
}