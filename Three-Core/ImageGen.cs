using System.Numerics;
using SixLabors.ImageSharp.Formats.Gif;
using SolidCode.Atlas;
using Color = SixLabors.ImageSharp.Color;

namespace Three_Core;

public static class ImageGen
{
    public static Image<Rgba64> GetImage(ProbabilityMap map, int brightness = 1)
    {
        Image<Rgba64> image = new Image<Rgba64>(map.Size, map.Size);
        
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

            return image;
        
        
    }

    public static void GenerateImage(ProbabilityMap map, string name, int brightness = 1)
    {
        using (var image = GetImage(map, brightness))
            image.Save(name + ".png");
    }
    
    public static void GenerateAnimation(MultiFrameProbabilityMap map, string name, int brightness = 1, int binFactor = 1)
    {
        int size = map.Maps[0].Size / binFactor;
        using Image<Rgba64> gif = new(size, size, Color.Black);
        GifFrameMetadata metadata = gif.Frames.RootFrame.Metadata.GetGifMetadata();
        metadata.FrameDelay = 20;
        gif.Metadata.GetGifMetadata().RepeatCount = 0;
        for (int i = 0; i < map.FrameCount; i++)
        {
            ProbabilityMap m = map.Maps[i];
            if (binFactor != 1)
            {
                m = m.BinDown(binFactor);
            }

            using (var image = GetImage(m, brightness))
            {
                metadata = image.Frames.RootFrame.Metadata.GetGifMetadata();
                metadata.FrameDelay = 20;
                gif.Frames.AddFrame(image.Frames.RootFrame);
            }
        }
        gif.SaveAsGif(name + ".gif");
    }
    
    private static float Adjust(float value, int brightness)
    {
        return (float)Math.Pow(value, 1 * Math.Clamp(1f - (brightness - 1) / 100f, 0.01f, 1f));
    }
}