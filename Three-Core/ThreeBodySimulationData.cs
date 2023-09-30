using System.Numerics;
using SolidCode.Atlas;
using ProtoBuf;
using Three_Core;
using ZstdSharp;

namespace Three_core;

public static class ThreeBodySimulationData
{
    public static RawSimulationDataCollection CreateData(SimulationState state, MultiFrameProbabilityMap map)
    {
        EncodedFrame[] frames = new EncodedFrame[map.FrameCount];
        IndexedThreadPool pool = new IndexedThreadPool(map.Maps.Length, 8, i =>
        {
            map.Maps[i].CalculateMaxValues();
            var data = new RawSimulationData
            {
                
                // We'll have to map the 2D array to a 1D array of arrays
                AMap = RemapIntArray(map.Maps[i].MapA),
                BMap = RemapIntArray(map.Maps[i].MapB),
                CMap = RemapIntArray(map.Maps[i].MapC),
                
                AVelMap = RemapVectorArray(map.VMaps[i].MapA),
                BVelMap = RemapVectorArray(map.VMaps[i].MapB),
                CVelMap = RemapVectorArray(map.VMaps[i].MapC),

                AMax = map.Maps[i].MaxA,
                BMax = map.Maps[i].MaxB,
                CMax = map.Maps[i].MaxC,
            };
            frames[i] = new EncodedFrame()
            {
                Data = GetCompressedBytes(data),
                Index = i
            };
        });
        pool.RunSync();
        

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

    public static SimulationState GetState(RawSimulationDataCollection data)
    {
        SimulationState state = new(data.Simulations);
        state.Bodies[0].Position = new Vector2(data.A.X, data.A.Y);
        state.Bodies[1].Position = new Vector2(data.B.X, data.B.Y);
        state.Bodies[2].Position = new Vector2(data.C.X, data.C.Y);
        state.RegenName();
        return state;
    }

    public static (SimulationState, MultiFrameProbabilityMap) GetData(RawSimulationDataCollection data)
    {
        SimulationState state = GetState(data);

        ProbabilityMap[] maps = new ProbabilityMap[data.Frames.Length];
        VelocityMap[] vmaps = new VelocityMap[data.Frames.Length];

        IndexedThreadPool pool = new IndexedThreadPool(data.Frames.Length, 8, i =>
        {
            RawSimulationData rdat = LoadFrame(data.Frames[i].Data);
            (maps[i], vmaps[i]) = AsMapPair(data, rdat);
        });
        pool.RunSync();
        MultiFrameProbabilityMap mfmap = new MultiFrameProbabilityMap(data.Frames.Length, maps[0].Size);
        mfmap.SetFrames(maps);
        mfmap.SetVelocityFrames(vmaps);
        return (state, mfmap);
    }

    public static (ProbabilityMap, VelocityMap) AsMapPair(RawSimulationDataCollection data, RawSimulationData rdat)
    {
         
        ProbabilityMap map = new ProbabilityMap(rdat.AMap.Length);
        VelocityMap vmap = new VelocityMap(rdat.AMap.Length);
            

        map.MapA = UnmapIntArray(rdat.AMap);
        map.MapB = UnmapIntArray(rdat.BMap);
        map.MapC = UnmapIntArray(rdat.CMap);
            
        map.MaxA = rdat.AMax;
        map.MaxB = rdat.BMax;
        map.MaxC = rdat.CMax;
            
        map.MapSize = data.MapScale;
        vmap.MapSize = data.MapScale;
            
        vmap.MapA = UnmapVectorArray(rdat.AVelMap);
        vmap.MapB = UnmapVectorArray(rdat.BVelMap);
        vmap.MapC = UnmapVectorArray(rdat.CVelMap);
        return (map, vmap);
    }

    private static IntColumn[] RemapIntArray(int[,] input)
    {
        IntColumn[] columns = new IntColumn[input.GetLength(0)];
        for (int x = 0; x < input.GetLength(0); x++)
        {
            columns[x] = new IntColumn();
            columns[x].Array = new int[input.GetLength(1)];
            for (int y = 0; y < input.GetLength(1); y++)
            {
                columns[x].Array[y] = input[x, y];
            }
        }

        return columns;
    }
    
    

    private static int[,] UnmapIntArray(IntColumn[] input)
    {
        int[,] output = new int[input.Length, input[0].Array.Length];
        for (int x = 0; x < input.Length; x++)
        {
            for (int y = 0; y < input[0].Array.Length; y++)
            {
                output[x, y] = input[x].Array[y];
            }
        }   

        return output;
    }
    
    private static VectorColumn[] RemapVectorArray(Vector2[,] input)
    {
        VectorColumn[] columns = new VectorColumn[input.GetLength(0)];
        for (int x = 0; x < input.GetLength(0); x++)
        {
            columns[x] = new VectorColumn();
            columns[x].Array = new SerializableVector2[input.GetLength(1)];
            for (int y = 0; y < input.GetLength(1); y++)
            {
                columns[x].Array[y] = input[x, y];
            }
        }

        return columns;
    }
    
    

    private static Vector2[,] UnmapVectorArray(VectorColumn[] input)
    {
        Vector2[,] output = new Vector2[input.Length, input[0].Array.Length];
        for (int x = 0; x < input.Length; x++)
        {
            for (int y = 0; y < input[0].Array.Length; y++)
            {
                output[x, y] = input[x].Array[y];
            }
        }

        return output;
    }
    
    public static byte[] GetCompressedBytes(RawSimulationData data)
    {
        byte[] bytes = GetDataBytes(data);
        return new Compressor(15).Wrap(bytes).ToArray();
    }

    public static RawSimulationData LoadFrame(byte[] data)
    {
        ReadOnlySpan<byte> bytes = new Decompressor().Unwrap(data.AsSpan()).ToArray();
        return Serializer.Deserialize<RawSimulationData>(bytes);
    }

    private static byte[] GetDataBytes(RawSimulationData data)
    {
        Debug.Log("Serializing to intermediary format...");
        byte[] bytes;
        using var ms = new MemoryStream();
        Serializer.Serialize(ms, data);
        bytes = ms.ToArray();

        return bytes;
    }

    public static void SaveToFile(SimulationState state, MultiFrameProbabilityMap map)
    {
        RawSimulationDataCollection simData = CreateData(state, map);
        Debug.Log("Compressing...");
        using var ms = new MemoryStream();
        Serializer.Serialize(ms, simData);
        byte[] bytes = ms.ToArray();
        if (!Directory.Exists(Path.Join(Atlas.AppDirectory, "output")))
            Directory.CreateDirectory(Path.Join(Atlas.AppDirectory, "output"));
        Debug.Log("Writing to file");
        File.WriteAllBytes(Path.Join(Atlas.AppDirectory, "output/" + state.Name + ".3bp"), bytes);
    }

    public static RawSimulationDataCollection LoadDataCollection(ReadOnlySpan<byte> data)
    {
        return Serializer.Deserialize<RawSimulationDataCollection>(data);
    }
}
[ProtoContract]
public struct RawSimulationDataCollection
{
    [ProtoMember(1)]
    public EncodedFrame[] Frames { get; set; }
    
    [ProtoMember(2)]
    public int Simulations { get; set; }
    
    [ProtoMember(3)]
    public double MapScale { get; set; }
    
    [ProtoMember(4)]
    public SerializableVector2 A { get; set; }
    
    [ProtoMember(5)]
    public SerializableVector2 B{ get; set; }
    
    [ProtoMember(6)]
    public SerializableVector2 C{ get; set; }
}

[ProtoContract]
public struct EncodedFrame
{
    [ProtoMember(1)]
    public byte[] Data { get; set; }

    [ProtoMember(2)] public int Index;
}

[ProtoContract]
public struct RawSimulationData
{

    // ProbabilityMaps
    [ProtoMember(1)]
    public IntColumn[] AMap { get; set; }
    
    [ProtoMember(2)]
    public IntColumn[] BMap { get; set; }
    
    [ProtoMember(3)]
    public IntColumn[] CMap { get; set; }
    
    
    [ProtoMember(4)]
    public VectorColumn[] AVelMap { get; set; }
    
    [ProtoMember(5)]
    public VectorColumn[] BVelMap { get; set; }
    
    [ProtoMember(6)]
    public VectorColumn[] CVelMap { get; set; }


    // Högsta värden på varje ProbabilityMap
    [ProtoMember(7)]
    public int AMax { get; set; }
    
    [ProtoMember(8)]
    public int BMax { get; set; }
    
    [ProtoMember(9)]
    public int CMax { get; set; }
}

[ProtoContract]
public struct SerializableVector2
{
    [ProtoMember(1)] public float X { get; set; }
    [ProtoMember(2)] public float Y { get; set; }

    public SerializableVector2(float x, float y)
    {
        X = x;
        Y = y;
    }

    public static implicit operator Vector2(SerializableVector2 v)
    {
        return new Vector2(v.X, v.Y);
    }

    public static implicit operator SerializableVector2(Vector2 v)
    {
        return new SerializableVector2(v.X, v.Y);
    }
}

[ProtoContract]
public struct IntColumn
{
    [ProtoMember(1)] public int[] Array { get; set; }
}

[ProtoContract]
public struct VectorColumn
{
    [ProtoMember(1)] public SerializableVector2[] Array { get; set; }
}