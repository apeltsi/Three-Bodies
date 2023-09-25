using Three_core;

namespace Three_Core;

public class MultiFrameProbabilityMap
{
    public ProbabilityMap[] Maps;
    public int FrameCount => Maps.Length;
    public MultiFrameProbabilityMap(int frameCount = 100, int size = 2048)
    {
        Maps = new ProbabilityMap[frameCount];
        for (int i = 0; i < frameCount; i++)
        {
            Maps[i] = new ProbabilityMap(size);
        }
    }

    public void SetFrames(ProbabilityMap[] maps)
    {
        Maps = maps;
    }

    public void AddAt(int frame, Vec2 pos, int body)
    {
        Maps[frame].AddAt(pos, body);
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
}