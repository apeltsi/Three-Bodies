using SolidCode.Atlas.Standard;
using Three_Core;

namespace Three_core;

public class SimulationState
{
    public BodyState[] Bodies;
    public string Name;
    public int SimCount;
    public SimulationState(int simCount)
    {
        SimCount = simCount;
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

    public void RegenName()
    {
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
}