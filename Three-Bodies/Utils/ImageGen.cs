namespace ThreeBodies.Utils;

public static class ImageGen
{
    public static void GenerateImage(ProbabilityMap map, string name)
    {
        using (Image<Rgba32> image = new Image<Rgba32>(ProbabilityMap.Size, ProbabilityMap.Size))
        {
            for (int x = 0; x < ProbabilityMap.Size; x++)
            {
                for (int y = 0; y < ProbabilityMap.Size; y++)
                {
                    float a =  map.MapA[x, y] / (float) map.MaxA;
                    float b = map.MapB[x, y] / (float) map.MaxB;
                    float c = map.MapC[x, y] / (float) map.MaxC;
                    image[x, y] = new Rgba32(a , b, c);
                }
            }

            image.Save("output/" + name + ".png");
        }
        
    }
}