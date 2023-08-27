using System.Numerics;
using SolidCode.Atlas;

namespace ThreeBodies.Utils;

public static class ImageGen
{
    public static void GenerateImage(ProbabilityMap map, string name)
    {
        using (Image<Rgba64> image = new Image<Rgba64>(ProbabilityMap.Size, ProbabilityMap.Size))
        {
            for (int x = 0; x < ProbabilityMap.Size; x++)
            {
                for (int y = 0; y < ProbabilityMap.Size; y++)
                {
                    float a =  Adjust(map.MapA[x, y] / (float) map.MaxA);
                    float b = Adjust(map.MapB[x, y] / (float) map.MaxB);
                    float c = Adjust(map.MapC[x, y] / (float) map.MaxC);
                    image[x, y] = new Rgba64(new Vector4(a, b, c, 1f));
                }
            }

            if (!Directory.Exists(Path.Join(Atlas.AppDirectory, "output")))
                Directory.CreateDirectory(Path.Join(Atlas.AppDirectory, "output"));
            
            image.Save(Path.Join(Atlas.AppDirectory, "output/" + name + ".png"));
        }
        
    }
    
    private static float Adjust(float value)
    {
        return (float)Math.Pow(value, 0.15);
    }
}