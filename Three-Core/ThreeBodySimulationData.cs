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
    public static RawSimulationData CreateData(SimulationState state, ProbabilityMap map)
    {
        map.CalculateMaxValues();
        return new RawSimulationData
        {
            A = state.Bodies[0].Position.AsVector2(),
            B = state.Bodies[1].Position.AsVector2(),
            C = state.Bodies[2].Position.AsVector2(),

            // We'll have to map the 2D array to a 1D array of arrays
            AMap = RemapArray(map.MapA),
            BMap = RemapArray(map.MapB),
            CMap = RemapArray(map.MapC),
            
            AMax = map.MaxA,
            BMax = map.MaxB,
            CMax = map.MaxC,
            
            Simulations = state.SimCount,
            MapScale = map.MapSize
        };
    }
    
    public static (SimulationState, ProbabilityMap) GetData(RawSimulationData data)
    {
        SimulationState state = new(data.Simulations);
        state.Bodies[0].Position = new Vec2(data.A.X, data.A.Y);
        state.Bodies[1].Position = new Vec2(data.B.X, data.B.Y);
        state.Bodies[2].Position = new Vec2(data.C.X, data.C.Y);
        state.RegenName();
        ProbabilityMap map = new();
        map.MapA = UnmapArray(data.AMap);
        map.MapB = UnmapArray(data.BMap);
        map.MapC = UnmapArray(data.CMap);
        map.MaxA = data.AMax;
        map.MaxB = data.BMax;
        map.MaxC = data.CMax;
        map.MapSize = data.MapScale;
        return (state, map);
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
        for(int x = 0; x < input.Length; x++)
        {
            for(int y = 0; y < input[0].Length; y++)
            {
                output[x, y] = input[x][y];
            }
        }
        return output;
    }

    public static byte[] GetCompressedBytes(RawSimulationData data)
    {
        byte[] bytes = GetDataBytes(data);
        return new Compressor(15).Wrap(bytes).ToArray();
    }

    public static RawSimulationData LoadData(byte[] data)
    {
        byte[] bytes = new Decompressor().Unwrap(data.AsSpan()).ToArray();
        string jsonString = Encoding.UTF8.GetString(bytes);
        return JsonSerializer.Deserialize<RawSimulationData>(jsonString);
    }
    
    private static byte[] GetDataBytes(RawSimulationData data) {
        string jsonString = JsonSerializer.Serialize(data);
        return Encoding.UTF8.GetBytes(jsonString);
    }

    public static void SaveToFile(SimulationState state, ProbabilityMap map)
    {
        RawSimulationData simData = CreateData(state, map);
        byte[] bytes = GetCompressedBytes(simData);
        if (!Directory.Exists(Path.Join(Atlas.AppDirectory, "output")))
            Directory.CreateDirectory(Path.Join(Atlas.AppDirectory, "output"));

        File.WriteAllBytes(Path.Join(Atlas.AppDirectory, "output/" + state.Name + ".3bp"), bytes);
    }
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
    
    public int Simulations { get; set; }
    public double MapScale { get; set; }
}