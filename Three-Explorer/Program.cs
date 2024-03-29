﻿using System.Numerics;
using System.Text.Json;
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
            if (args.Length > 1)
            {
                if (args[1] == "dir")
                {
                    // Lets get all files in the directory
                    string[] files = Directory.GetFiles(Directory.GetCurrentDirectory());
                    Console.WriteLine($"Processing {files.Length} files...");
                    for (int i = 0; i < files.Length; i++)
                    {
                        Console.WriteLine($"File {i + 1}...");
                        ProcessFile(files[i], arg);
                    }
                }
                else
                {
                    string[] files = args[1].Split(",");
                    for (int i = 0; i < files.Length; i++)
                    {
                        ProcessFile(files[i], args[0]);
                    }
                }
                
            }
            
        }
    }

    public static void ProcessFile(string filename, string arg)
    {
        if (System.IO.File.Exists(filename))
        {
            string fname = Path.GetFileName(filename);
            fname = fname.Substring(0, fname.Length - 4);
            Console.WriteLine("Reading File...");
            byte[] bytes = File.ReadAllBytes(filename);
            Console.WriteLine("Parsing Data...");
            RawSimulationDataCollection data = ThreeBodySimulationData.LoadDataCollection(bytes);
            if (arg == "gif")
            {
                Console.WriteLine("Saving animation...");
                Console.WriteLine("Deserializing to Map...");
                (SimulationState state, MultiFrameProbabilityMap mfmap) = ThreeBodySimulationData.GetData(data);
                Console.WriteLine("Generating Animation...");
                ImageGen.GenerateAnimation(mfmap, fname, Brightness, BinFactor);
            } else if (arg == "sequence")
            {
                Console.WriteLine("Deserializing to Map...");
                (SimulationState state, MultiFrameProbabilityMap mfmap) = ThreeBodySimulationData.GetData(data);
                Console.WriteLine("Generating PNG Sequence...");
                ImageGen.GeneratePNGSequence(mfmap, fname, Brightness, BinFactor);
            } else if (arg == "stats")
            {
                Console.WriteLine("Deserializing to Map...");
                (ProbabilityMap map, VelocityMap _) = ThreeBodySimulationData.AsMapPair(data, ThreeBodySimulationData.LoadFrame(data.Frames[^1].Data).Item1);
                Console.WriteLine("Statistics for \"" + fname +"\"");
                
                Console.WriteLine("SimCount: " + data.Simulations);
                Console.WriteLine("Size: " + map.Size);
                
                Console.WriteLine("MAX A: " + map.MaxA);
                Console.WriteLine("MAX B: " + map.MaxB);
                Console.WriteLine("MAX C: " + map.MaxC);
                
                Console.WriteLine("A Certainty: " + (map.MaxA / (float)data.Simulations * 100).ToString("F5") + "%");
                Console.WriteLine("B Certainty: " + (map.MaxB / (float)data.Simulations * 100).ToString("F5") + "%");
                Console.WriteLine("C Certainty: " + (map.MaxC / (float)data.Simulations * 100).ToString("F5") + "%");
                            
            } else if (arg == "state")
            {
                SimulationState state = ThreeBodySimulationData.GetState(data);
                InitialState istate = new InitialState()
                {
                    A = state.Bodies[0].Position,
                    B = state.Bodies[1].Position,
                    C = state.Bodies[2].Position,
                };
                string idata = JsonSerializer.Serialize(istate);
                Console.WriteLine("--- Initial Simulation Data ---");
                Console.WriteLine(idata);
            }
            else
            {
                Console.WriteLine("Unknown command '" + arg + "'");
            }
        }
        else
        {
            Console.WriteLine("File not found");
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