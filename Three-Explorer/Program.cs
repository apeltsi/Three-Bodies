using SolidCode.Atlas;
using Three_core;
using Three_Core;

namespace ThreeExplorer;

public static class Program
{
    public static int BinFactor = 1;
    public static int Brightness = 1;
    
    public static void Main(string[] args)
    {
        Atlas.DisableMultiProcessDebugging();
        Console.WriteLine("Three Explorer");
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i].ToLower() == "--bin")
            {
                try
                {
                    if (args.Length > i + 1)
                        BinFactor = int.Parse(args[i + 1]);
                }
                catch (Exception e)
                {
                    Debug.Warning("Couldn't parse bin factor");
                }
            } else if (args[i].ToLower() == "--bright")
            {
                
                try
                {
                    if (args.Length > i + 1)
                        Brightness = int.Parse(args[i + 1]);
                }
                catch (Exception e)
                {
                    Debug.Warning("Couldn't parse brightness");
                }
            }
        }
        if (args.Length > 0)
        {
            string arg = args[0].ToLower();
            if (arg == "view" || arg == "imgen" || arg == "stats")
            {
                if (args.Length > 1)
                {
                    string filename = args[1];
                    if (System.IO.File.Exists(filename))
                    {
                        Console.WriteLine("Reading File...");
                        byte[] bytes = File.ReadAllBytes(filename);
                        Console.WriteLine("Parsing Data...");
                        RawSimulationDataCollection data = ThreeBodySimulationData.LoadData(bytes);
                        Console.WriteLine("Deserializing...");
                        (SimulationState state, MultiFrameProbabilityMap mfmap) = ThreeBodySimulationData.GetData(data);
                        
                        if (arg == "imgen")
                        {
                            Console.WriteLine("Saving image...");
                            ImageGen.GenerateAnimation(mfmap, state.Name, Brightness, BinFactor);
                        } else if (arg == "stats")
                        {
                            ProbabilityMap map = ProcessFrame(mfmap.Maps[^1]);
                            Console.WriteLine("Statistics for \"" + state.Name +"\"");
                            Console.WriteLine("SimCount: " + data.Simulations);
                            Console.WriteLine("Size: " + map.Size);
                            Console.WriteLine("MAX A: " + map.MaxA);
                            Console.WriteLine("MAX B: " + map.MaxB);
                            Console.WriteLine("MAX C: " + map.MaxC);
                            
                            Console.WriteLine("A Certainty: " + (map.MaxA / (float)data.Simulations * 100).ToString("F5") + "%");
                            Console.WriteLine("B Certainty: " + (map.MaxB / (float)data.Simulations * 100).ToString("F5") + "%");
                            Console.WriteLine("C Certainty: " + (map.MaxC / (float)data.Simulations * 100).ToString("F5") + "%");
                            
                        }

                    }
                    else
                    {
                        SolidCode.Atlas.Debug.Log("File not found");
                    }
                }
            }
        }
    }

    public static ProbabilityMap ProcessFrame(ProbabilityMap map)
    {
        if (BinFactor > 1)
        {
            map = map.BinDown(BinFactor);
        }

        return map;
    }
}