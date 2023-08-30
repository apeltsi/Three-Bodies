using System.Diagnostics;
using System.Numerics;
using SolidCode.Atlas;
using Three_core;
using Three_Core;
using ThreeBodies.Utils;
using Debug = SolidCode.Atlas.Debug;

// A simple class managing the GPU compute simulation

namespace ThreeBodies;

public class GSim
{
    public SimulationState State;
    private static ComputeModelHalf _model = new();
    
    public GSim()
    {
        State = new(Program.CThreadCount * Program.CThreadGroups);
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
            _model.Dispatch(Program.CThreadCount / 128, 1,1); // We divide by 100 because we have 100 threads per group
            data.AddRange(_model.GetBuffer());
            TickScheduler.FreeThreads();
            SolidCode.Atlas.Debug.Log("Progress: " + (i / (float)Program.CThreadGroups * 100).ToString("F1") + "%");
            Program.AddSimulations(Program.CThreadCount);
        }
        sw.Stop();
        Debug.Log("Compute Executed :) after " + sw.ElapsedMilliseconds + "ms");
        ProbabilityMap map = new ProbabilityMap();
        for (int i = 0; i < data.Count; i++)
        {
            map.AddAt(new Vec2(data[i]), i % 3);
        }

        ImageGen.GenerateImage(map, "test", 99);
        ThreeBodySimulationData.SaveToFile(State, map);
        
        Debug.Log("Data Generated");
    }
}