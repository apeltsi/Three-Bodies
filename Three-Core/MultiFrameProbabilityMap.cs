using System.Numerics;
using Three_core;

namespace Three_Core;

public class MultiFrameProbabilityMap
{
    public ProbabilityMap[] Maps;
    public VelocityMap[] VMaps;
    public int FrameCount => Maps.Length;
    public MultiFrameProbabilityMap(int frameCount = 100, int size = 2048)
    {
        Maps = new ProbabilityMap[frameCount];
        VMaps = new VelocityMap[frameCount];
        for (int i = 0; i < frameCount; i++)
        {
            Maps[i] = new ProbabilityMap(size);
            VMaps[i] = new VelocityMap(size);
        }
    }

    public void SetFrames(ProbabilityMap[] maps)
    {
        Maps = maps;
    }
    
    public void SetVelocityFrames(VelocityMap[] maps)
    {
        VMaps = maps;
    }

    
    // Ugly code, but faster because there is no branching :)
    public void AddA(int frame, Vector2 pos)
    {
        Maps[frame].AddA(pos);
    }
    
    public void AddB(int frame, Vector2 pos)
    {
        Maps[frame].AddB(pos);
    }
    
    public void AddC(int frame, Vector2 pos)
    {
        Maps[frame].AddC(pos);
    }
    
    // Ugly code part two
    public void AddVelA(int frame, Vector2 vel)
    {
        VMaps[frame].AddA(vel);
    }
    
    public void AddVelB(int frame, Vector2 vel)
    {
        VMaps[frame].AddB(vel);
    }
    
    public void AddVelC(int frame, Vector2 vel)
    {
        VMaps[frame].AddC(vel);
    }

    public float EvaluateInterest()
    {
        ProbabilityMap map = Maps[10];
        map.CalculateMaxValues();
        // Lets calculate our certainties to determine how interesting of a result we got
        float certA = map.MaxA / (float)map.MapSims;
        float certB = map.MaxB / (float)map.MapSims;
        float certC = map.MaxC / (float)map.MapSims;
        float certAvg = (certA + certB + certC) / 3f;
        return certAvg;
    }

    public void Normalize()
    {
        for (int i = 0; i < FrameCount; i++)
        {
            VMaps[i].NormalizeMap();
        }
    }
}