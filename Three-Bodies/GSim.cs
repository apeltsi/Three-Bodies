using System.Diagnostics;
using System.Numerics;
using SolidCode.Atlas;
using SolidCode.Atlas.Mathematics;
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
    public static int SimsPerSecond = 0;
    public static string Status = "";
    private int _sims = 0;
    private Stopwatch _sw;
    
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
        _sw = Stopwatch.StartNew();
        Status = "";
        for (int i = 0; i < Program.CThreadGroups; i++)
        {
            TickScheduler.RequestTick().Wait();
            _model.Dispatch(Program.CThreadCount / 128, 1,1); // We divide by 128 because we have 128 threads per group
            _sims += Program.CThreadCount;
            data.AddRange(_model.GetBuffer());
            TickScheduler.FreeThreads();
            SolidCode.Atlas.Debug.Log("Progress: " + (i / (float)Program.CThreadGroups * 100).ToString("F1") + "%");
            Program.AddSimulations(Program.CThreadCount);
            if (_sw.ElapsedMilliseconds > 100)
            {
                int millis = (int)_sw.ElapsedMilliseconds;
                SimsPerSecond = AMath.RoundToInt(_sims / (millis / 1000f));
                _sims = 0;
                _sw.Restart();
            }
        }
        sw.Stop();
        Debug.Log("Compute Executed :) after " + sw.ElapsedMilliseconds + "ms");
        Status = "Analyzing data...";
        MultiFrameProbabilityMap map = new MultiFrameProbabilityMap(Program.FrameCount);
        for (int i = 0; i < data.Count; i++)
        {
            int tI = (i % (Program.CThreadCount * 3 * Program.FrameCount));
            int frame = (int)Math.Floor(tI / (float)(Program.CThreadCount * 3));
            map.AddAt(frame, new Vec2(data[i]), i % 3);
        }
        Status = "Serializing data...";
        ThreeBodySimulationData.SaveToFile(State, map);
        Status = "";

        Debug.Log("Data Generated");
    }
}