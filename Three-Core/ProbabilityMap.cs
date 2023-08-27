using SolidCode.Atlas;

namespace Three_Core;

public class ProbabilityMap
{
    public int[,] MapA;
    public int[,] MapB;
    public int[,] MapC;
    public double MapSize = 100;
    public int MaxA;
    public int MaxB;
    public int MaxC;
    public int Size;

    public ProbabilityMap(int size = 2048)
    {
        Size = size;
        MapA = new int[size, size];
        MapB = new int[size, size];
        MapC = new int[size, size];
    }

    public void AddAt(Vec2 pos, int body)
    {
        if (double.IsNaN(pos.Y) || double.IsNaN(pos.X))
            return;
        int posX = (int) Math.Clamp(Math.Round((pos.X + MapSize / 2.0) * (Size - 1) / MapSize), 0, Size - 1);
        int posY = (int) Math.Clamp(Math.Round((pos.Y + MapSize / 2.0) * (Size - 1) / MapSize), 0, Size - 1);
        try
        {
            switch (body)
            {
                case 0:
                    MapA[posX, posY]++;
                    break;
                case 1:
                    MapB[posX, posY]++;
                    break;
                case 2:
                    MapC[posX, posY]++;
                    break;
            }
        }
        catch (Exception e)
        {
            
        }
    }
    
    public static ProbabilityMap operator +(ProbabilityMap a, ProbabilityMap b)
    {
        ProbabilityMap c = new ProbabilityMap(b.Size);
        for (int x = 0; x < c.Size; x++)
        {
            for (int y = 0; y < c.Size; y++)
            {
                c.MapA[x, y] = a.MapA[x, y] + b.MapA[x, y];
                c.MapB[x, y] = a.MapB[x, y] + b.MapB[x, y];
                c.MapC[x, y] = a.MapC[x, y] + b.MapC[x, y];
            }
        }

        return c;
    }

    public void CalculateMaxValues()
    {
        MaxA = 0;
        MaxB = 0;
        MaxC = 0;
        for (int x = 0; x < Size; x++)
        {
            for (int y = 0; y < Size; y++)
            {
                if (MapA[x, y] > MaxA)
                {
                    MaxA = MapA[x, y];
                }
                if (MapB[x, y] > MaxB)
                {
                    MaxB = MapB[x, y];
                }
                if (MapC[x, y] > MaxC)
                {
                    MaxC = MapC[x, y];
                }
            }
        }
    }

    public ProbabilityMap BinDown(int factor)
    {
        var map = new ProbabilityMap(Size / factor);
        for (int x = 0; x < Size; x++)
        {
            for (int y = 0; y < Size; y++)
            {
                map.MapA[x / factor, y / factor] += MapA[x, y];
                map.MapB[x / factor, y / factor] += MapB[x, y];
                map.MapC[x / factor, y / factor] += MapC[x, y];
            }
        }
        map.CalculateMaxValues();
        return map;
    }
}