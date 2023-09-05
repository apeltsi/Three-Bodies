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
    private Thread? _analysisThread;
    private Vector2[] _dataToAnalyse;
    private MultiFrameProbabilityMap _map = new MultiFrameProbabilityMap(Program.FrameCount, Program.Resolution);
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
        _sw = Stopwatch.StartNew();
        Status = "";
        for (int i = 0; i < Program.CThreadGroups; i++)
        {
            TickScheduler.RequestTick().Wait();
            _model.Dispatch(Program.CThreadCount / 128, 1,1); // We divide by 128 because we have 128 threads per group
            _sims += Program.CThreadCount;
            _analysisThread?.Join();
            _dataToAnalyse = _model.GetBuffer();
            _analysisThread = new Thread(ProcessData);
            _analysisThread.Start();
            TickScheduler.FreeThreads();
            
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
        _analysisThread?.Join();
        
        Status = "Serializing data...";
        ThreeBodySimulationData.SaveToFile(State, _map);
        Status = "";

        Debug.Log("Data Generated");
    }

    private void ProcessData()
    {
        for (int i = 0; i < _dataToAnalyse.Length; i++)
        {
            int frame = (int)Math.Floor(i / (float)(Program.CThreadCount * 3));

            if (frame > Program.FrameCount - 1)
            {
                frame = Program.FrameCount - 1;
            }
            _map.AddAt(frame, new Vec2(_dataToAnalyse[i]), i % 3);
        }
    }
}