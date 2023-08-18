using SolidCode.Atlas;
using SolidCode.Atlas.AssetManagement;
using SolidCode.Atlas.ECS;
using SolidCode.Atlas.Rendering;
using SolidCode.Atlas.Rendering.PostProcess;
using SolidCode.Atlas.Standard;

namespace ThreeBodies
{
    public static class Program
    {
        public static Body[] Bodies;

        public static void Main(string[] args)
        {
            Atlas.StartCoreFeatures("ThreeBodies", new FrameworkConfiguration()
            {
                ECS = new ECSSettings()
                {
                    Threads = new []
                    {
                        new ECSThreadSettings()
                        {
                            Name = "Main",
                            Frequency = 1000,
                            Sync = true
                        }
                    }
                }
            }); // Starta Atlas
            // Vi laddar in alla resurser som vi behöver för visualiseringen
            var pack = new AssetPack("main");
            pack.Load();
            
            Renderer.AddPostProcessEffect(new BloomEffect()); // Grafisk effekt
            GenerateBodies();
            CameraController.GetCamera();
            Atlas.Start();
        }

        public static void GenerateBodies()
        {
            Bodies = new Body[3];
            Bodies[0] = GetBody();
            Bodies[1] = GetBody();
            Bodies[2] = GetBody();
            Body.UpdateAccelerations();
        }

        private static Body GetBody()
        {
            Entity e = new Entity("Body");
            return e.AddComponent<Body>();
        }
    }
}