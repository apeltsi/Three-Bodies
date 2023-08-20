using SolidCode.Atlas;
using SolidCode.Atlas.AssetManagement;
using SolidCode.Atlas.ECS;
using SolidCode.Atlas.Rendering;
using SolidCode.Atlas.Rendering.PostProcess;
using SolidCode.Atlas.Standard;
using ThreeBodies.Utils;

namespace ThreeBodies
{
    public static class Program
    {
        public const double Dt = 0.2;
        public const int SimCount = 1000;
        public const int TickCount = 20_000;
        public const int ThreadCount = 50;
        public const int TotalSimulations = ThreadCount * SimCount;
        private static int _simulationsPerformed = 0;
        private static int ThreadsAlive = 0;
        public static ProbabilityMap ProbabilityMap = new();
        public static NormalDistribution NormalDistribution = new (0f, 0.5f);
        public static int SimulationsPerformed => _simulationsPerformed;
        public static SimulationState State;
        public static bool quit = false;
        public static void Main(string[] args)
        {
            // Starta Atlas
            Atlas.StartCoreFeatures("ThreeBodies", new FrameworkConfiguration()
            {
                ECS = new ECSSettings()
                {
                    Threads = new []
                    {
                        new ECSThreadSettings()
                        {
                            Name = "Main",
                            Frequency = 1,
                            Sync = true
                        }
                    }
                }
            });
            bool performanceMode = true;
            EntityComponentSystem.RegisterUpdateAction(() =>
            {
                if (Window.Focused && performanceMode == true)
                {
                    performanceMode = false;
                    Window.MaxFramerate = 15;
                }
                else if(!Window.Focused && performanceMode == false)
                {
                    performanceMode = true;
                    Window.MaxFramerate = 2;
                }
            });
            // Vi laddar in alla resurser som vi behöver för visualiseringen
            var pack = new AssetPack("main");
            pack.Load();
            var worker = new Thread(StartThreads);
            worker.Start();
            Progress.GetText();
            Renderer.AddPostProcessEffect(new BloomEffect()); // Grafisk effekt
            CameraController.GetCamera();
            Atlas.Start();
            quit = true;
            worker.Join();
        }

        public static void StartThreads()
        {
            _simulationsPerformed = 0;
            ProbabilityMap = new();
            State = new SimulationState();
            for (int i = 0; i < ThreadCount; i++)
            {
                var t = new Thread(ThreadEntry);
                t.Start();
            }
            Thread.Sleep(1000);
            while (ThreadsAlive != 0)
            {
                Thread.Sleep(1000);
                
            }

            if (quit)
            {
                return;
            }
            StartThreads();
        }
        
        public static void ThreadEntry()
        {
            Interlocked.Increment(ref ThreadsAlive);
            int sims = 0;
            int lastAdd = 0;
            var map = new ProbabilityMap();
            while (sims < SimCount)
            {
                var sim = new Simulation(State);
                sim.RunSimulation(map);
                sims++;
                if (sims - lastAdd == 10)
                {
                    Interlocked.Add(ref _simulationsPerformed, 10);
                    lastAdd = sims;
                }
                if (quit)
                {
                    Interlocked.Decrement(ref ThreadsAlive);
                    return;
                }
            }

            int toAdd = sims - lastAdd;
            
            if(toAdd > 0)
                Interlocked.Add(ref _simulationsPerformed, toAdd);

            lock (ProbabilityMap)
            {
                ProbabilityMap += map;
            }
            int left = Interlocked.Decrement(ref ThreadsAlive);
            if (left == 0)
            {
                ProbabilityMap.CalculateMaxValues();
                ImageGen.GenerateImage(ProbabilityMap, State.Name);
            }
        }
    }
}