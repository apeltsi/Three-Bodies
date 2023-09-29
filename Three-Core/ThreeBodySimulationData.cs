using System.Numerics;
using System.Text;
using SolidCode.Atlas;
using System.Text.Json;
using System.Text.Json.Serialization;
using Three_Core;
using ZstdSharp;

namespace Three_core;

public static class ThreeBodySimulationData
{
    public static RawSimulationDataCollection CreateData(SimulationState state, MultiFrameProbabilityMap map)
    {
        RawSimulationData[] frames = new RawSimulationData[map.FrameCount];
        for (int i = 0; i < map.FrameCount; i++)
        {
            map.Maps[i].CalculateMaxValues();
            frames[i] = new RawSimulationData
            {
                
                // We'll have to map the 2D array to a 1D array of arrays
                AMap = RemapArray(map.Maps[i].MapA),
                BMap = RemapArray(map.Maps[i].MapB),
                CMap = RemapArray(map.Maps[i].MapC),
                
                AVelMap = RemapArray(map.VMaps[i].MapA),
                BVelMap = RemapArray(map.VMaps[i].MapB),
                CVelMap = RemapArray(map.VMaps[i].MapC),

                AMax = map.Maps[i].MaxA,
                BMax = map.Maps[i].MaxB,
                CMax = map.Maps[i].MaxC,
            };
        }

        return new RawSimulationDataCollection()
        {
            Frames = frames,
            Simulations = state.SimCount,
            MapScale = map.Maps[0].MapSize,
            A = state.Bodies[0].Position,
            B = state.Bodies[1].Position,
            C = state.Bodies[2].Position,
        };
    }

    public static (SimulationState, MultiFrameProbabilityMap) GetData(RawSimulationDataCollection data)
    {
        SimulationState state = new(data.Simulations);
        state.Bodies[0].Position = new Vec2(data.A.X, data.A.Y);
        state.Bodies[1].Position = new Vec2(data.B.X, data.B.Y);
        state.Bodies[2].Position = new Vec2(data.C.X, data.C.Y);

        ProbabilityMap[] maps = new ProbabilityMap[data.Frames.Length];
        VelocityMap[] vmaps = new VelocityMap[data.Frames.Length];
        for (int i = 0; i < data.Frames.Length; i++)
        {
            maps[i] = new ProbabilityMap(data.Frames[i].AMap.Length);
            vmaps[i] = new VelocityMap(data.Frames[i].AMap.Length);
            state.RegenName();

            maps[i].MapA = UnmapArray(data.Frames[i].AMap);
            maps[i].MapB = UnmapArray(data.Frames[i].BMap);
            maps[i].MapC = UnmapArray(data.Frames[i].CMap);
            maps[i].MaxA = data.Frames[i].AMax;
            maps[i].MaxB = data.Frames[i].BMax;
            maps[i].MaxC = data.Frames[i].CMax;
            maps[i].MapSize = data.MapScale;
            vmaps[i].MapSize = data.MapScale;
            vmaps[i].MapA = UnmapArray(data.Frames[i].AVelMap);
            vmaps[i].MapB = UnmapArray(data.Frames[i].BVelMap);
            vmaps[i].MapC = UnmapArray(data.Frames[i].CVelMap);
        }

        MultiFrameProbabilityMap mfmap = new MultiFrameProbabilityMap(data.Frames.Length, maps[0].Size);
        mfmap.SetFrames(maps);
        mfmap.SetVelocityFrames(vmaps);
        return (state, mfmap);
    }

    private static T[][] RemapArray<T>(T[,] input)
    {
        int count = 0;
        return input.Cast<T>()
            .GroupBy(x => count++ / input.GetLength(1))
            .Select(g => g.ToArray()).ToArray();
    }
    
    

    private static T[,] UnmapArray<T>(T[][] input)
    {
        T[,] output = new T[input.Length, input[0].Length];
        for (int x = 0; x < input.Length; x++)
        {
            for (int y = 0; y < input[0].Length; y++)
            {
                output[x, y] = input[x][y];
            }
        }

        return output;
    }

    public static byte[] GetCompressedBytes(RawSimulationDataCollection data)
    {
        byte[] bytes = GetDataBytes(data);
        return new Compressor(15).Wrap(bytes).ToArray();
    }

    public static RawSimulationDataCollection LoadData(byte[] data)
    {
        byte[] bytes = new Decompressor().Unwrap(data.AsSpan()).ToArray();
        Console.WriteLine("Uncompressed size is " + (bytes.Length / (1000 * 1000)).ToString("F1") + "Mb");
        string jsonString = Encoding.UTF8.GetString(bytes);
        Console.WriteLine("Deserializing to intermediary format...");
        return JsonSerializer.Deserialize<RawSimulationDataCollection>(jsonString);
    }

    private static byte[] GetDataBytes(RawSimulationDataCollection data)
    {
        string jsonString = JsonSerializer.Serialize(data);
        return Encoding.UTF8.GetBytes(jsonString);
    }

    public static void SaveToFile(SimulationState state, MultiFrameProbabilityMap map)
    {
        RawSimulationDataCollection simData = CreateData(state, map);
        Debug.Log("Compressing...");
        byte[] bytes = GetCompressedBytes(simData);
        if (!Directory.Exists(Path.Join(Atlas.AppDirectory, "output")))
            Directory.CreateDirectory(Path.Join(Atlas.AppDirectory, "output"));
        Debug.Log("Writing to file");
        File.WriteAllBytes(Path.Join(Atlas.AppDirectory, "output/" + state.Name + ".3bp"), bytes);
    }
}

public struct RawSimulationDataCollection
{
    public RawSimulationData[] Frames { get; set; }
    public int Simulations { get; set; }
    public double MapScale { get; set; }
    public Vector2 A { get; set; }
    public Vector2 B{ get; set; }
    public Vector2 C{ get; set; }
}

public struct RawSimulationData
{

    // ProbabilityMaps
    public int[][] AMap { get; set; }
    public int[][] BMap { get; set; }
    public int[][] CMap { get; set; }
    
    public Vector2[][] AVelMap { get; set; }
    public Vector2[][] BVelMap { get; set; }
    public Vector2[][] CVelMap { get; set; }


    // Högsta värden på varje ProbabilityMap
    public int AMax { get; set; }
    public int BMax { get; set; }
    public int CMax { get; set; }
}