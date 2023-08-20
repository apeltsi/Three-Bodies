using SolidCode.Atlas.Standard;

namespace ThreeBodies;

public class SimulationState
{
    public BodyState[] Bodies;
    public string Name;
    public SimulationState()
    {
        Bodies = new BodyState[3];
        Bodies[0] = new();
        Bodies[1] = new();
        Bodies[2] = new();
        string name = "";
        foreach (var b in Bodies)
        {
            name += b.Position.GetHashCode();
        }

        Name = name;

    }
}

public class BodyState
{
    public Vec2 Position = new(ARandom.Range(-1f, 1f), ARandom.Range(-1f, 1f));

    public Vec2 SamplePosition()
    {
        Random r = new();
        return Position + new Vec2(Program.NormalDistribution.Sample(r), Program.NormalDistribution.Sample(r)) * 0.01;
    }
}