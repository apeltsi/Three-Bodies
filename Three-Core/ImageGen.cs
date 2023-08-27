using System.Numerics;
using SolidCode.Atlas;

namespace Three_Core;

public static class ImageGen
{
    public static void GenerateImage(ProbabilityMap map, string name, int brightness = 1)
    {
        using (Image<Rgba64> image = new Image<Rgba64>(map.Size, map.Size))
        {
            for (int x = 0; x < map.Size; x++)
            {
                for (int y = 0; y < map.Size; y++)
                {
                    float a =  Adjust(map.MapA[x, y] / (float) map.MaxA, brightness);
                    float b = Adjust(map.MapB[x, y] / (float) map.MaxB, brightness);
                    float c = Adjust(map.MapC[x, y] / (float) map.MaxC, brightness);
                    image[x, y] = new Rgba64(new Vector4(a, b, c, 1f));
                }
            }
            
            image.Save(name + ".png");
        }
        
    }
    
    private static float Adjust(float value, int brightness)
    {
        return (float)Math.Pow(value, 1 * Math.Clamp(1f - (brightness - 1) / 100f, 0.01f, 1f));
    }
}