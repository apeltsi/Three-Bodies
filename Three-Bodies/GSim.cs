using System.Diagnostics;
using System.Numerics;
using SolidCode.Atlas;
using ThreeBodies.Utils;

namespace ThreeBodies;

public class GSim
{
    public SimulationState State;
    private static ComputeModelHalf _model = new();
    
    public GSim()
    {
        State = new();
    }
    
    public void RunSimulation()
    {
        TickScheduler.RequestTick().Wait();
        _model.UpdateUniformBuffer(State);
        TickScheduler.FreeThreads();

        Program.ResetSimulations();
        Stopwatch sw = Stopwatch.StartNew();
        List<Vector2> data = new List<Vector2>();
        for (int i = 0; i < Program.CThreadGroups; i++)
        {
            TickScheduler.RequestTick().Wait();
            _model.Dispatch(Program.CThreadCount, 1,1);
            data.AddRange(_model.GetBuffer());
            TickScheduler.FreeThreads();
            SolidCode.Atlas.Debug.Log("Progress: " + (i / (float)Program.CThreadGroups * 100).ToString("F1") + "%");
            Program.AddSimulations(Program.CThreadCount);
        }
        sw.Stop();
        SolidCode.Atlas.Debug.Log("Compute Executed :) after " + sw.ElapsedMilliseconds + "ms");
        ProbabilityMap map = new ProbabilityMap();
        for (int i = 0; i < data.Count; i += 3)
        {
            map.AddAt(new Vec2(data[i]), 0);
            map.AddAt(new Vec2(data[i + 1]), 1);
            map.AddAt(new Vec2(data[i + 2]), 2);
        }
        map.CalculateMaxValues();
        ImageGen.GenerateImage(map, State.Name);
        SolidCode.Atlas.Debug.Log("Data Generated");
    }
}