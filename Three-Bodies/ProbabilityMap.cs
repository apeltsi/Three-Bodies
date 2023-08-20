namespace ThreeBodies;

public class ProbabilityMap
{
    public const int Size = 512;
    public int[,] MapA = new int[Size, Size];
    public int[,] MapB = new int[Size, Size];
    public int[,] MapC = new int[Size, Size];
    public const double MapSize = 20;
    public int MaxA;
    public int MaxB;
    public int MaxC;

    public void AddAt(Vec2 pos, int body)
    {
        int posX = (int) Math.Clamp(Math.Round((pos.X + MapSize / 2) * (Size - 1) / MapSize), 0, Size - 1);
        int posY = (int) Math.Clamp(Math.Round((pos.Y + MapSize / 2) * (Size - 1) / MapSize), 0, Size - 1);
        
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
    
    public static ProbabilityMap operator +(ProbabilityMap a, ProbabilityMap b)
    {
        ProbabilityMap c = new ProbabilityMap();
        for (int x = 0; x < Size; x++)
        {
            for (int y = 0; y < Size; y++)
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
}