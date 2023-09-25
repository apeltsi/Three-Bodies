using System.Numerics;

namespace Three_Core;

public class VelocityMap
{
    public Vector2[,] MapA;
    public Vector2[,] MapB;
    public Vector2[,] MapC;
    public double MapSize = 5;
    public int Size;
    public int MapSims = 0;

    public VelocityMap(int size = 2048)
    {
        Size = size;
        MapA = new Vector2[size, size];
        MapB = new Vector2[size, size];
        MapC = new Vector2[size, size];
    }

    public void AddA(Vector2 pos)
    {
        try
        {
            if (float.IsNaN(pos.Y) || float.IsNaN(pos.X))
                return;
            int posX = (int)Math.Clamp(Math.Round((pos.X + MapSize / 2.0) * (Size - 1) / MapSize), 0, Size - 1);
            int posY = (int)Math.Clamp(Math.Round((pos.Y + MapSize / 2.0) * (Size - 1) / MapSize), 0, Size - 1);
            MapA[posX, posY] += pos;
            MapSims++;
        }
        finally
        {
        }
    }

    public void AddB(Vector2 pos)
    {
        try
        {
            if (float.IsNaN(pos.Y) || float.IsNaN(pos.X))
                return;
            int posX = (int)Math.Clamp(Math.Round((pos.X + MapSize / 2.0) * (Size - 1) / MapSize), 0, Size - 1);
            int posY = (int)Math.Clamp(Math.Round((pos.Y + MapSize / 2.0) * (Size - 1) / MapSize), 0, Size - 1);
            MapB[posX, posY] += pos;
            MapSims++;
        }
        finally
        {
        }
    }

    public void AddC(Vector2 pos)
    {
        try
        {
            if (float.IsNaN(pos.Y) || float.IsNaN(pos.X))
                return;
            int posX = (int)Math.Clamp(Math.Round((pos.X + MapSize / 2.0) * (Size - 1) / MapSize), 0, Size - 1);
            int posY = (int)Math.Clamp(Math.Round((pos.Y + MapSize / 2.0) * (Size - 1) / MapSize), 0, Size - 1);
            MapC[posX, posY] += pos;
            MapSims++;
        }
        finally
        {
        }
    }

    public void NormalizeMap()
    {
        for (int i = 0; i < Size; i++)
        {
            for (int j = 0; j < Size; j++)
            {
                MapA[i, j] /= MapSims / 3f;
                MapB[i, j] /= MapSims / 3f;
                MapC[i, j] /= MapSims / 3f;
            }
        }
    }
}