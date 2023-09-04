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
                A = state.Bodies[0].Position.AsVector2(),
                B = state.Bodies[1].Position.AsVector2(),
                C = state.Bodies[2].Position.AsVector2(),

                // We'll have to map the 2D array to a 1D array of arrays
                AMap = RemapArray(map.Maps[i].MapA),
                BMap = RemapArray(map.Maps[i].MapB),
                CMap = RemapArray(map.Maps[i].MapC),

                AMax = map.Maps[i].MaxA,
                BMax = map.Maps[i].MaxB,
                CMax = map.Maps[i].MaxC,
            };
        }

        return new RawSimulationDataCollection()
        {
            Frames = frames,
            Simulations = state.SimCount,
            MapScale = map.Maps[0].MapSize
        };
    }

    public static (SimulationState, MultiFrameProbabilityMap) GetData(RawSimulationDataCollection data)
    {
        SimulationState state = new(data.Simulations);
        ProbabilityMap[] maps = new ProbabilityMap[data.Frames.Length];
        for (int i = 0; i < data.Frames.Length; i++)
        {
            maps[i] = new ProbabilityMap();
            state.Bodies[0].Position = new Vec2(data.Frames[i].A.X, data.Frames[i].A.Y);
            state.Bodies[1].Position = new Vec2(data.Frames[i].B.X, data.Frames[i].B.Y);
            state.Bodies[2].Position = new Vec2(data.Frames[i].C.X, data.Frames[i].C.Y);
            state.RegenName();

            maps[i].MapA = UnmapArray(data.Frames[i].AMap);
            maps[i].MapB = UnmapArray(data.Frames[i].BMap);
            maps[i].MapC = UnmapArray(data.Frames[i].CMap);
            maps[i].MaxA = data.Frames[i].AMax;
            maps[i].MaxB = data.Frames[i].BMax;
            maps[i].MaxC = data.Frames[i].CMax;
            maps[i].MapSize = data.MapScale;
        }

        MultiFrameProbabilityMap mfmap = new MultiFrameProbabilityMap();
        mfmap.SetFrames(maps);
        return (state, mfmap);
    }

    private static int[][] RemapArray(int[,] input)
    {
        int count = 0;
        return input.Cast<int>()
            .GroupBy(x => count++ / input.GetLength(1))
            .Select(g => g.ToArray()).ToArray();
    }

    private static int[,] UnmapArray(int[][] input)
    {
        int[,] output = new int[input.Length, input[0].Length];
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
        byte[] bytes = GetCompressedBytes(simData);
        if (!Directory.Exists(Path.Join(Atlas.AppDirectory, "output")))
            Directory.CreateDirectory(Path.Join(Atlas.AppDirectory, "output"));

        File.WriteAllBytes(Path.Join(Atlas.AppDirectory, "output/" + state.Name + ".3bp"), bytes);
    }
}

public struct RawSimulationDataCollection
{
    public RawSimulationData[] Frames { get; set; }
    public int Simulations { get; set; }
    public double MapScale { get; set; }
}

public struct RawSimulationData
{
    // Start positionerna
    public Vector2 A { get; set; }
    public Vector2 B { get; set; }
    public Vector2 C { get; set; }

    // ProbabilityMaps
    public int[][] AMap { get; set; }
    public int[][] BMap { get; set; }
    public int[][] CMap { get; set; }

    // Högsta värden på varje ProbabilityMap
    public int AMax { get; set; }
    public int BMax { get; set; }
    public int CMax { get; set; }
}